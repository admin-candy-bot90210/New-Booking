using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class ChatWindow : Window
    {
        private FirebaseService _firebaseService;
        private User _currentUser;
        private List<ChatMessage> _allMessages = new List<ChatMessage>();
        private List<User> _onlineUsers = new List<User>();
        private ChatChannel _currentChannel = ChatChannel.World;
        private string? _currentPrivateRecipient = null;
        private UserChatSettings _chatSettings;
        private DispatcherTimer _refreshTimer;
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private bool _isMinimizedToTray = false;

        public ChatWindow(FirebaseService firebaseService, User currentUser, bool stayOnTop = false)
        {
            InitializeComponent();
            _firebaseService = firebaseService;
            _currentUser = currentUser;

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;

            // Initialize chat settings
            LoadChatSettings();

            // Show admin channel only for admins
            if (_currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager)
            {
                AdminOnlyChannelButton.Visibility = Visibility.Visible;
            }

            // Show stats panel only for SysAdmin
            if (_currentUser.Role == UserRole.SysAdmin)
            {
                StatsPanel.Visibility = Visibility.Visible;
            }

            // Initialize system tray if enabled
            if (_chatSettings.MinimizeToTray)
            {
                InitializeSystemTray();
            }

            // Setup auto-refresh timer (every 5 seconds)
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(5);
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();

            // Initial load
            LoadMessages();
            LoadOnlineUsers();

            // Highlight selected channel
            SelectChannel(ChatChannel.World);
        }

        #region Initialization

        private async void LoadChatSettings()
        {
            try
            {
                _chatSettings = await _firebaseService.GetUserChatSettingsAsync(_currentUser.Username)
                               ?? new UserChatSettings { Username = _currentUser.Username };
            }
            catch
            {
                _chatSettings = new UserChatSettings { Username = _currentUser.Username };
            }
        }

        private void InitializeSystemTray()
        {
            try
            {
                _notifyIcon = new System.Windows.Forms.NotifyIcon();
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                _notifyIcon.Text = "DJ Booking Chat";
                _notifyIcon.Visible = false;

                // Double-click to restore
                _notifyIcon.MouseDoubleClick += (s, e) => RestoreFromTray();

                // Context menu
                var contextMenu = new System.Windows.Forms.ContextMenuStrip();
                contextMenu.Items.Add("Restore", null, (s, e) => RestoreFromTray());
                contextMenu.Items.Add("Exit", null, (s, e) => Application.Current.Shutdown());
                _notifyIcon.ContextMenuStrip = contextMenu;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize system tray: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Message Loading & Display

        private async void LoadMessages()
        {
            try
            {
                _allMessages = await _firebaseService.GetAllChatMessagesAsync();

                // Filter by current channel and apply mute/block lists
                DisplayMessages();
                UpdateStatistics();
                ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load messages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayMessages()
        {
            List<ChatMessage> messagesToDisplay;

            if (_currentChannel == ChatChannel.Private && !string.IsNullOrEmpty(_currentPrivateRecipient))
            {
                // Show private messages between current user and recipient
                messagesToDisplay = _allMessages.Where(m =>
                    m.Channel == ChatChannel.Private &&
                    ((m.SenderUsername == _currentUser.Username && m.RecipientUsername == _currentPrivateRecipient) ||
                     (m.SenderUsername == _currentPrivateRecipient && m.RecipientUsername == _currentUser.Username))
                ).ToList();
            }
            else if (_currentChannel == ChatChannel.ToAdmin)
            {
                // Show messages from users to admin + admin responses
                // Visible to: Message sender + All admins
                bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;
                messagesToDisplay = _allMessages.Where(m =>
                    m.Channel == ChatChannel.ToAdmin &&
                    (isAdmin || m.SenderUsername == _currentUser.Username)
                ).ToList();
            }
            else if (_currentChannel == ChatChannel.AdminOnly)
            {
                // Only admins can see
                messagesToDisplay = _allMessages.Where(m => m.Channel == ChatChannel.AdminOnly).ToList();
            }
            else // World
            {
                messagesToDisplay = _allMessages.Where(m => m.Channel == ChatChannel.World).ToList();
            }

            // Filter out blocked and muted users
            messagesToDisplay = messagesToDisplay.Where(m =>
                !_chatSettings.BlockedUsers.Contains(m.SenderUsername) &&
                !_chatSettings.MutedUsers.Contains(m.SenderUsername)
            ).ToList();

            // Sort by timestamp
            messagesToDisplay = messagesToDisplay.OrderBy(m => m.Timestamp).ToList();

            ChatMessagesControl.ItemsSource = messagesToDisplay;
        }

        private void UpdateStatistics()
        {
            if (_currentUser.Role == UserRole.SysAdmin)
            {
                TotalMessagesText.Text = $"Total: {_allMessages.Count}";
                ErrorCountText.Text = $"Errors: {_allMessages.Count(m => m.IsErrorMessage)}";
                OnlineUsersText.Text = $"Online: {_onlineUsers.Count}";
            }
        }

        #endregion

        #region Channel Selection

        private void SelectChannel(ChatChannel channel)
        {
            _currentChannel = channel;
            _currentPrivateRecipient = null;

            // Reset all channel button backgrounds
            WorldChannelButton.Background = System.Windows.Media.Brushes.Transparent;
            ToAdminChannelButton.Background = System.Windows.Media.Brushes.Transparent;
            AdminOnlyChannelButton.Background = System.Windows.Media.Brushes.Transparent;

            // Highlight selected channel
            switch (channel)
            {
                case ChatChannel.World:
                    WorldChannelButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                    HeaderTextBlock.Text = "🌍 World Chat";
                    SubHeaderTextBlock.Text = "Public chat - visible to all users";
                    break;
                case ChatChannel.ToAdmin:
                    ToAdminChannelButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                    HeaderTextBlock.Text = "🆘 To Admin";
                    SubHeaderTextBlock.Text = "Request help from administrators";
                    break;
                case ChatChannel.AdminOnly:
                    AdminOnlyChannelButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                    HeaderTextBlock.Text = "🔒 Admin Only";
                    SubHeaderTextBlock.Text = "Private admin channel";
                    break;
            }

            DisplayMessages();
        }

        private void WorldChannel_Click(object sender, RoutedEventArgs e)
        {
            SelectChannel(ChatChannel.World);
        }

        private void ToAdminChannel_Click(object sender, RoutedEventArgs e)
        {
            SelectChannel(ChatChannel.ToAdmin);
        }

        private void AdminOnlyChannel_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager)
            {
                SelectChannel(ChatChannel.AdminOnly);
            }
        }

        #endregion

        #region Private Messaging

        private async void StartPrivateChat_Click(object sender, RoutedEventArgs e)
        {
            // Show user selection dialog
            var users = await _firebaseService.GetAllUsersAsync();
            var otherUsers = users.Where(u => u.Username != _currentUser.Username).ToList();

            if (otherUsers.Count == 0)
            {
                MessageBox.Show("No other users available for private chat.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create simple input dialog (would be better as a custom window)
            var dialog = new Window
            {
                Title = "Select User for Private Chat",
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var listBox = new ListBox { Margin = new Thickness(10) };
            listBox.ItemsSource = otherUsers.Select(u => u.Username).ToList();

            var button = new Button { Content = "Start Chat", Margin = new Thickness(10), Padding = new Thickness(10, 5, 10, 5) };
            button.Click += (s, args) =>
            {
                if (listBox.SelectedItem != null)
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                }
            };

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = "Select a user:", Margin = new Thickness(10) });
            panel.Children.Add(listBox);
            panel.Children.Add(button);

            dialog.Content = panel;

            if (dialog.ShowDialog() == true && listBox.SelectedItem != null)
            {
                StartPrivateConversation(listBox.SelectedItem.ToString());
            }
        }

        private void StartPrivateConversation(string username)
        {
            _currentChannel = ChatChannel.Private;
            _currentPrivateRecipient = username;

            HeaderTextBlock.Text = $"💬 Private Chat with {username}";
            SubHeaderTextBlock.Text = "1-on-1 private conversation";

            DisplayMessages();
            UpdatePrivateConversationsList();
        }

        private void PrivateConversation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string username)
            {
                StartPrivateConversation(username);
            }
        }

        private async void UpdatePrivateConversationsList()
        {
            // Get list of users we have private conversations with
            var privateMessages = _allMessages.Where(m =>
                m.Channel == ChatChannel.Private &&
                (m.SenderUsername == _currentUser.Username || m.RecipientUsername == _currentUser.Username)
            ).ToList();

            var conversationPartners = privateMessages
                .Select(m => m.SenderUsername == _currentUser.Username ? m.RecipientUsername : m.SenderUsername)
                .Where(u => !string.IsNullOrEmpty(u))
                .Distinct()
                .Select(u => new { Username = u, UnreadCount = 0, HasUnread = false })
                .ToList();

            PrivateConversationsListBox.ItemsSource = conversationPartners;
        }

        #endregion

        #region Message Sending

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                SendMessage();
            }
        }

        private async System.Threading.Tasks.Task SendMessage()
        {
            string messageText = MessageTextBox.Text.Trim();

            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            // Check if channel requires admin permissions
            if (_currentChannel == ChatChannel.AdminOnly &&
                _currentUser.Role != UserRole.SysAdmin &&
                _currentUser.Role != UserRole.Manager)
            {
                MessageBox.Show("You don't have permission to send messages to the Admin Only channel.",
                    "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var message = new ChatMessage
                {
                    SenderUsername = _currentUser.Username,
                    SenderRole = _currentUser.Role.ToString(),
                    SenderIsDJ = _currentUser.IsDJ,
                    SenderIsVenueOwner = _currentUser.IsVenueOwner,
                    Message = messageText,
                    Timestamp = DateTime.Now,
                    IsErrorMessage = false,
                    Type = MessageType.Normal,
                    Channel = _currentChannel,
                    RecipientUsername = _currentPrivateRecipient,
                    IsRead = false
                };

                await _firebaseService.AddChatMessageAsync(message);

                MessageTextBox.Clear();
                LoadMessages();

                // Show notification to recipient if private message
                if (_currentChannel == ChatChannel.Private && !string.IsNullOrEmpty(_currentPrivateRecipient))
                {
                    ShowNotification($"New message from {_currentUser.Username}", messageText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Online Users & Actions

        private async void LoadOnlineUsers()
        {
            try
            {
                // Get all active users (in a real app, you'd track online status)
                _onlineUsers = await _firebaseService.GetAllUsersAsync();
                _onlineUsers = _onlineUsers.Where(u =>
                    u.IsActive &&
                    u.Username != _currentUser.Username &&
                    !_chatSettings.BlockedUsers.Contains(u.Username)
                ).ToList();

                OnlineUsersListBox.ItemsSource = _onlineUsers;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load online users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendPrivateMessage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string username)
            {
                StartPrivateConversation(username);
            }
        }

        private async void MuteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string username)
            {
                var result = MessageBox.Show($"Mute {username}?\n\nYou won't see messages from this user.",
                    "Mute User", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (!_chatSettings.MutedUsers.Contains(username))
                    {
                        _chatSettings.MutedUsers.Add(username);
                        await _firebaseService.UpdateUserChatSettingsAsync(_chatSettings);
                        DisplayMessages();
                        MessageBox.Show($"{username} has been muted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private async void BlockUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string username)
            {
                var result = MessageBox.Show($"Block {username}?\n\nThis user won't be able to send you private messages, and you won't see their messages.",
                    "Block User", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (!_chatSettings.BlockedUsers.Contains(username))
                    {
                        _chatSettings.BlockedUsers.Add(username);
                        await _firebaseService.UpdateUserChatSettingsAsync(_chatSettings);
                        DisplayMessages();
                        LoadOnlineUsers();
                        MessageBox.Show($"{username} has been blocked.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private async void ManageBlockedUsers_Click(object sender, RoutedEventArgs e)
        {
            if (_chatSettings.BlockedUsers.Count == 0)
            {
                MessageBox.Show("You haven't blocked any users.", "No Blocked Users", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Create simple dialog to unblock users
            var dialog = new Window
            {
                Title = "Manage Blocked Users",
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var listBox = new ListBox { Margin = new Thickness(10) };
            listBox.ItemsSource = _chatSettings.BlockedUsers.ToList();

            var unblockButton = new Button { Content = "Unblock Selected", Margin = new Thickness(10), Padding = new Thickness(10, 5, 10, 5) };
            unblockButton.Click += async (s, args) =>
            {
                if (listBox.SelectedItem != null)
                {
                    string username = listBox.SelectedItem.ToString();
                    _chatSettings.BlockedUsers.Remove(username);
                    await _firebaseService.UpdateUserChatSettingsAsync(_chatSettings);
                    listBox.ItemsSource = _chatSettings.BlockedUsers.ToList();
                    DisplayMessages();
                    LoadOnlineUsers();
                    MessageBox.Show($"{username} has been unblocked.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            var closeButton = new Button { Content = "Close", Margin = new Thickness(10), Padding = new Thickness(10, 5, 10, 5) };
            closeButton.Click += (s, args) => dialog.Close();

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = "Blocked Users:", Margin = new Thickness(10), FontWeight = FontWeights.Bold });
            panel.Children.Add(listBox);
            panel.Children.Add(unblockButton);
            panel.Children.Add(closeButton);

            dialog.Content = panel;
            dialog.ShowDialog();
        }

        private async void ReportUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string username)
            {
                // Create report dialog
                var dialog = new Window
                {
                    Title = $"Report User: {username}",
                    Width = 400,
                    Height = 450,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var panel = new StackPanel { Margin = new Thickness(15) };

                // Header
                panel.Children.Add(new TextBlock
                {
                    Text = $"Report {username} to administrators",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 15)
                });

                // Reason selection
                panel.Children.Add(new TextBlock { Text = "Reason:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 5) });
                var reasonComboBox = new ComboBox { Margin = new Thickness(0, 0, 0, 15) };
                reasonComboBox.Items.Add("Harassment");
                reasonComboBox.Items.Add("Spam");
                reasonComboBox.Items.Add("Inappropriate Content");
                reasonComboBox.Items.Add("Impersonation");
                reasonComboBox.Items.Add("Other");
                reasonComboBox.SelectedIndex = 0;
                panel.Children.Add(reasonComboBox);

                // Details
                panel.Children.Add(new TextBlock { Text = "Details (optional):", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 5) });
                var detailsTextBox = new TextBox
                {
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                    Height = 120,
                    Margin = new Thickness(0, 0, 0, 15),
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                panel.Children.Add(detailsTextBox);

                // Buttons
                var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

                var submitButton = new Button
                {
                    Content = "Submit Report",
                    Padding = new Thickness(15, 8, 15, 8),
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                    Foreground = new SolidColorBrush(Colors.White),
                    BorderThickness = new Thickness(0)
                };
                submitButton.Click += async (s, args) =>
                {
                    var report = new UserReport
                    {
                        ReportedUsername = username,
                        ReporterUsername = _currentUser.Username,
                        Reason = reasonComboBox.SelectedItem?.ToString() ?? "Other",
                        Details = detailsTextBox.Text.Trim(),
                        ReportedAt = DateTime.Now,
                        IsResolved = false
                    };

                    try
                    {
                        await _firebaseService.AddUserReportAsync(report);
                        dialog.Close();
                        MessageBox.Show($"Thank you. Your report about {username} has been submitted to the administrators.",
                            "Report Submitted", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to submit report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                var cancelButton = new Button
                {
                    Content = "Cancel",
                    Padding = new Thickness(15, 8, 15, 8),
                    Background = new SolidColorBrush(Color.FromRgb(149, 165, 166)),
                    Foreground = new SolidColorBrush(Colors.White),
                    BorderThickness = new Thickness(0)
                };
                cancelButton.Click += (s, args) => dialog.Close();

                buttonPanel.Children.Add(submitButton);
                buttonPanel.Children.Add(cancelButton);
                panel.Children.Add(buttonPanel);

                dialog.Content = panel;
                dialog.ShowDialog();
            }
        }

        #endregion

        #region Refresh & Settings

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMessages();
            LoadOnlineUsers();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadMessages();
            LoadOnlineUsers();
        }

        private void ChatSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chat Settings:\n\n" +
                $"✓ Notifications: {(_chatSettings.EnableNotifications ? "Enabled" : "Disabled")}\n" +
                $"✓ Sound: {(_chatSettings.EnableSoundNotifications ? "Enabled" : "Disabled")}\n" +
                $"✓ Minimize to Tray: {(_chatSettings.MinimizeToTray ? "Enabled" : "Disabled")}\n\n" +
                $"Blocked Users: {_chatSettings.BlockedUsers.Count}\n" +
                $"Muted Users: {_chatSettings.MutedUsers.Count}",
                "Chat Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region System Tray Integration

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && _chatSettings.MinimizeToTray && _notifyIcon != null)
            {
                MinimizeToTray();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Stop timer
            _refreshTimer?.Stop();

            // Dispose system tray icon
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
        }

        private void MinimizeToTray()
        {
            if (_notifyIcon == null) return;

            _isMinimizedToTray = true;
            this.Hide();
            _notifyIcon.Visible = true;
            _notifyIcon.ShowBalloonTip(2000, "DJ Booking Chat", "Chat minimized to tray. Double-click to restore.", System.Windows.Forms.ToolTipIcon.Info);
        }

        private void RestoreFromTray()
        {
            if (_notifyIcon == null) return;

            _isMinimizedToTray = false;
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            _notifyIcon.Visible = false;
        }

        #endregion

        #region Notifications

        private void ShowNotification(string title, string message)
        {
            if (!_chatSettings.EnableNotifications) return;

            try
            {
                if (_notifyIcon != null && _isMinimizedToTray)
                {
                    // Show balloon tip from tray
                    _notifyIcon.ShowBalloonTip(5000, title, message.Length > 100 ? message.Substring(0, 100) + "..." : message,
                        System.Windows.Forms.ToolTipIcon.Info);
                }
                else
                {
                    // Show toast notification (Windows 10+)
                    // Note: Full implementation would use Windows.UI.Notifications
                    // For now, just use a simple message box if window is not focused
                    if (!this.IsActive)
                    {
                        this.Flash();
                    }
                }

                // Play sound if enabled
                if (_chatSettings.EnableSoundNotifications)
                {
                    System.Media.SystemSounds.Exclamation.Play();
                }
            }
            catch { }
        }

        #endregion

        #region Helper Methods

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToBottom();
        }

        #endregion
    }

    // Extension method to flash window
    public static class WindowExtensions
    {
        public static void Flash(this Window window)
        {
            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }
            window.Activate();
        }
    }
}
