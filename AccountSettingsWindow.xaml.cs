using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class AccountSettingsWindow : Window
    {
        private FirebaseService _firebaseService;
        private User _currentUser;
        public bool AccountDeleted { get; private set; } = false;

        public AccountSettingsWindow(FirebaseService firebaseService, User currentUser)
        {
            InitializeComponent();
            _firebaseService = firebaseService;
            _currentUser = currentUser;

            LoadAccountInfo();
        }

        private void LoadAccountInfo()
        {
            UsernameTextBlock.Text = _currentUser.Username;
            RoleTextBlock.Text = _currentUser.Role.ToString();
            MemberSinceTextBlock.Text = _currentUser.CreatedAt.ToString("MMM dd, yyyy");

            // Determine account type
            List<string> accountTypes = new List<string>();
            if (_currentUser.IsDJ) accountTypes.Add("DJ");
            if (_currentUser.IsVenueOwner) accountTypes.Add("Venue Owner");
            if (!_currentUser.IsDJ && !_currentUser.IsVenueOwner) accountTypes.Add("User");

            AccountTypeTextBlock.Text = string.Join(" & ", accountTypes);

            // Check if user is banned
            if (_currentUser.IsBanned)
            {
                BanWarningTextBlock.Visibility = Visibility.Visible;
                DeleteAccountButton.IsEnabled = false;
                DeleteAccountButton.Opacity = 0.5;
            }
        }

        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            // Confirm deletion with password
            var passwordDialog = new Window
            {
                Title = "Confirm Account Deletion",
                Width = 450,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = System.Windows.Media.Brushes.White
            };

            var panel = new System.Windows.Controls.StackPanel { Margin = new Thickness(20) };

            panel.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "⚠️ Confirm Account Deletion",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new Thickness(0, 0, 0, 15)
            });

            panel.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "This action cannot be undone. All your personal data will be permanently deleted.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = System.Windows.Media.Brushes.Black
            });

            panel.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "Please enter your password to confirm:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = System.Windows.Media.Brushes.Black
            });

            var passwordBox = new System.Windows.Controls.PasswordBox
            {
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 20)
            };
            panel.Children.Add(passwordBox);

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var deleteButton = new System.Windows.Controls.Button
            {
                Content = "Delete My Account",
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 10, 0),
                Background = System.Windows.Media.Brushes.Red,
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.Bold
            };

            deleteButton.Click += async (s, args) =>
            {
                if (string.IsNullOrEmpty(passwordBox.Password))
                {
                    MessageBox.Show("Please enter your password.", "Password Required",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Verify password
                string passwordHash = HashPassword(passwordBox.Password);
                if (passwordHash != _currentUser.PasswordHash)
                {
                    MessageBox.Show("Incorrect password. Account deletion cancelled.", "Incorrect Password",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Final confirmation
                var result = MessageBox.Show(
                    "Are you absolutely sure you want to delete your account?\n\nThis action is PERMANENT and cannot be undone.",
                    "Final Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _firebaseService.DeleteUserAccountAsync(_currentUser.Username);

                        MessageBox.Show(
                            "Your account has been successfully deleted.\n\nThank you for using our service.",
                            "Account Deleted",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        AccountDeleted = true;
                        passwordDialog.Close();
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete account: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };

            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Cancel",
                Padding = new Thickness(15, 8, 15, 8),
                Background = System.Windows.Media.Brushes.Gray,
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0)
            };
            cancelButton.Click += (s, args) => passwordDialog.Close();

            buttonPanel.Children.Add(deleteButton);
            buttonPanel.Children.Add(cancelButton);
            panel.Children.Add(buttonPanel);

            passwordDialog.Content = panel;
            passwordDialog.ShowDialog();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
