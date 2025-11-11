using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

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

                // Verify password
                string passwordHash = HashPassword(password);
                if (user.PasswordHash != passwordHash)
                {
                    ShowError("Invalid username or password.");
                    return;
                }

                // Update last login
                user.LastLogin = DateTime.Now;

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
