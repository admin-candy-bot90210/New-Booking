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
                await _firebaseService.UpdateUserAsync(user.Id ?? "", user);

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
