using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;
using DJBookingSystem.Utilities;

namespace DJBookingSystem
{
    public partial class LoginWindow : Window
    {
        private FirebaseService? _firebaseService;
        public User? LoggedInUser { get; private set; }

        public LoginWindow(FirebaseService? firebaseService)
        {
            InitializeComponent();
            _firebaseService = firebaseService;

            // Load saved login info
            var loginInfo = LocalStorage.GetLoginInfo();
            if (loginInfo != null && loginInfo.RememberMe)
            {
                UsernameTextBox.Text = loginInfo.Username;
                RememberMeCheckBox.IsChecked = true;
            }

            // Trigger fade-in animation when window loads
            Loaded += (s, e) =>
            {
                var fadeIn = (System.Windows.Media.Animation.Storyboard)FindResource("FadeInAnimation");
                fadeIn.Begin(this);
            };
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageTextBlock.Visibility = Visibility.Collapsed;

            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            if (_firebaseService == null)
            {
                ShowError("Not connected to Firebase. Please configure Firebase URL in settings.");
                return;
            }

            try
            {
                // Get current IP address
                string currentIP = await IPHelper.GetPublicIPAddressAsync();

                // Check if this IP is banned
                var bannedUser = await _firebaseService.GetBannedUserByIPAsync(currentIP);
                if (bannedUser != null)
                {
                    string banMessage = $"This IP address has been banned.";
                    if (bannedUser.BanExpiry.HasValue)
                    {
                        banMessage += $"\nBan expires: {bannedUser.BanExpiry.Value:MMM dd, yyyy HH:mm}";
                    }
                    if (!string.IsNullOrEmpty(bannedUser.BanReason))
                    {
                        banMessage += $"\n\nReason: {bannedUser.BanReason}";
                    }
                    banMessage += "\n\nIf you believe this is a mistake, please contact an administrator.";
                    ShowError(banMessage);
                    return;
                }

                // Get user from Firebase
                var user = await _firebaseService.GetUserByUsernameAsync(username);

                if (user == null)
                {
                    ShowError("Invalid username or password.");
                    return;
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    ShowError("This account has been deactivated. Please contact your administrator.");
                    return;
                }

                // Check if user is banned
                if (user.IsBanned)
                {
                    string banMessage = "Your account has been banned.";
                    if (user.BanExpiry.HasValue)
                    {
                        if (user.BanExpiry.Value > DateTime.Now)
                        {
                            banMessage = $"Your account is temporarily banned until {user.BanExpiry.Value:MMM dd, yyyy HH:mm}.";
                        }
                        else
                        {
                            // Ban expired, automatically unban
                            user.IsBanned = false;
                            user.BannedBy = null;
                            user.BannedAt = null;
                            user.BanReason = null;
                            user.BanExpiry = null;
                            if (!string.IsNullOrEmpty(user.Id))
                            {
                                await _firebaseService.UpdateUserAsync(user.Id, user);
                            }
                        }
                    }

                    if (user.IsBanned)
                    {
                        if (!string.IsNullOrEmpty(user.BanReason))
                        {
                            banMessage += $"\n\nReason: {user.BanReason}";
                        }
                        banMessage += "\n\nPlease contact an administrator if you believe this is a mistake.";
                        ShowError(banMessage);
                        return;
                    }
                }

                // Check if mute expired and clear it
                if (user.IsGloballyMuted && user.MuteExpiry.HasValue && user.MuteExpiry.Value <= DateTime.Now)
                {
                    user.IsGloballyMuted = false;
                    user.MutedBy = null;
                    user.MutedAt = null;
                    user.MuteExpiry = null;
                    if (!string.IsNullOrEmpty(user.Id))
                    {
                        await _firebaseService.UpdateUserAsync(user.Id, user);
                    }
                }

                // Verify password
                string passwordHash = HashPassword(password);
                if (user.PasswordHash != passwordHash)
                {
                    ShowError("Invalid username or password.");
                    return;
                }

                // Update last login and IP tracking
                user.LastLogin = DateTime.Now;
                user.CurrentIP = currentIP;

                // Add to IP history if not already present
                if (!user.IPHistory.Contains(currentIP))
                {
                    user.IPHistory.Add(currentIP);
                }

                // Safety check: Only update if user has valid ID
                if (!string.IsNullOrEmpty(user.Id))
                {
                    await _firebaseService.UpdateUserAsync(user.Id, user);
                }

                // Save login info if Remember Me is checked
                bool rememberMe = RememberMeCheckBox.IsChecked ?? false;
                bool autoLogin = user.AppPreferences?.AutoLogin ?? false;
                LocalStorage.SaveLoginInfo(username, rememberMe, autoLogin);

                // Login successful
                LoggedInUser = user;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Login failed: {ex.Message}");
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var registrationWindow = new RegistrationWindow(_firebaseService);
                if (registrationWindow.ShowDialog() == true && registrationWindow.RegisteredUser != null)
                {
                    // Auto-fill username after successful registration
                    UsernameTextBox.Text = registrationWindow.RegisteredUser.Username;
                    PasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to open registration: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            ErrorMessageTextBlock.Text = message;
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
