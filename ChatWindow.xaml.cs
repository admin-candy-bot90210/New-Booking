using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class ChatWindow : Window
    {
        private FirebaseService _firebaseService;
        private User _currentUser;
        private List<ChatMessage> _allMessages = new List<ChatMessage>();
        private bool _showErrorsOnly = false;

        public ChatWindow(FirebaseService firebaseService, User currentUser, bool stayOnTop = false)
        {
            InitializeComponent();
            _firebaseService = firebaseService;
            _currentUser = currentUser;

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;

            // Show stats panel only for SysAdmin
            if (_currentUser.Role == UserRole.SysAdmin)
            {
                StatsPanel.Visibility = Visibility.Visible;
                HeaderTextBlock.Text = "SysAdmin Chat Panel";
                SubHeaderTextBlock.Text = "All messages and error notifications";
            }

            LoadMessages();
        }

        private async void LoadMessages()
        {
            try
            {
                _allMessages = await _firebaseService.GetAllChatMessagesAsync();
                DisplayMessages();
                UpdateStatistics();
                ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load chat messages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayMessages()
        {
            var messagesToDisplay = _showErrorsOnly
                ? _allMessages.Where(m => m.IsErrorMessage).ToList()
                : _allMessages;

            ChatMessagesControl.ItemsSource = messagesToDisplay;
        }

        private void UpdateStatistics()
        {
            if (_currentUser.Role == UserRole.SysAdmin)
            {
                TotalMessagesText.Text = $"Total Messages: {_allMessages.Count}";
                ErrorCountText.Text = $"Errors: {_allMessages.Count(m => m.IsErrorMessage)}";

                var today = DateTime.Today;
                var todayMessages = _allMessages.Count(m => m.Timestamp.Date == today);
                TodayMessagesText.Text = $"Today: {todayMessages}";
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private async System.Threading.Tasks.Task SendMessage()
        {
            string messageText = MessageTextBox.Text.Trim();

            if (string.IsNullOrEmpty(messageText))
            {
                MessageBox.Show("Please enter a message.", "Empty Message", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var message = new ChatMessage
                {
                    SenderUsername = _currentUser.Username,
                    SenderRole = _currentUser.Role.ToString(),
                    Message = messageText,
                    Timestamp = DateTime.Now,
                    IsErrorMessage = false,
                    Type = MessageType.Normal
                };

                await _firebaseService.AddChatMessageAsync(message);

                MessageTextBox.Clear();
                LoadMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMessages();
        }

        private void FilterErrors_Click(object sender, RoutedEventArgs e)
        {
            _showErrorsOnly = !_showErrorsOnly;

            if (_showErrorsOnly)
            {
                FilterErrorsButton.Content = "Show All Messages";
                FilterErrorsButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));
            }
            else
            {
                FilterErrorsButton.Content = "Show Errors Only";
                FilterErrorsButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));
            }

            DisplayMessages();
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToBottom();
        }
    }
}
