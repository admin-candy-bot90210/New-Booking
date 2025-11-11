using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;
using DJBookingSystem.Utilities;

namespace DJBookingSystem
{
    public partial class RegistrationWindow : Window
    {
        private readonly FirebaseService _firebaseService;
        public User? RegisteredUser { get; private set; }

        public RegistrationWindow(FirebaseService firebaseService)
        {
            InitializeComponent();
            _firebaseService = firebaseService;
        }

        private void AccountTypeChanged(object sender, RoutedEventArgs e)
        {
            // Hide/show descriptions based on selection
            bool isDJChecked = IsDJCheckBox.IsChecked ?? false;
            bool isVenueOwnerChecked = IsVenueOwnerCheckBox.IsChecked ?? false;

            // Hide DJ description when checked
            DJDescription.Visibility = isDJChecked ? Visibility.Collapsed : Visibility.Visible;

            // Hide Venue Owner description when checked
            VenueOwnerDescription.Visibility = isVenueOwnerChecked ? Visibility.Collapsed : Visibility.Visible;

            // Show DJ-specific fields only if DJ is checked
            DJFieldsPanel.Visibility = isDJChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OpenPostImages_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open PostImages.org in default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://postimages.org/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open PostImages.org: {ex.Message}\n\nPlease manually visit: https://postimages.org/",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                {
                    MessageBox.Show("Please enter a username (DJ name).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(PasswordBox.Password) || PasswordBox.Password.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool isDJ = IsDJCheckBox.IsChecked ?? false;
                bool isVenueOwner = IsVenueOwnerCheckBox.IsChecked ?? false;

                if (!isDJ && !isVenueOwner)
                {
                    MessageBox.Show("Please select at least one account type (DJ or Venue Owner).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // DJ-specific validation
                if (isDJ)
                {
                    if (string.IsNullOrWhiteSpace(StreamingLinkTextBox.Text))
                    {
                        MessageBox.Show("Please enter your streaming link URL.\n\nThis is required for DJ accounts.",
                            "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Basic URL validation for streaming link
                    if (!StreamingLinkTextBox.Text.Trim().StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                        !StreamingLinkTextBox.Text.Trim().StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Streaming link must be a valid URL starting with http:// or https://",
                            "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Optional: Validate DJ Logo URL if provided
                    if (!string.IsNullOrWhiteSpace(DJLogoUrlTextBox.Text))
                    {
                        if (!DJLogoUrlTextBox.Text.Trim().StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                            !DJLogoUrlTextBox.Text.Trim().StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("DJ Logo URL must be a valid URL starting with http:// or https://",
                                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }

                if (AgreeTermsCheckBox.IsChecked != true)
                {
                    MessageBox.Show("Please agree to the Terms of Service and Privacy Policy.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get current IP address
                string currentIP = await IPHelper.GetPublicIPAddressAsync();

                // Check if this IP is banned
                var bannedUser = await _firebaseService.GetBannedUserByIPAsync(currentIP);
                if (bannedUser != null)
                {
                    string banMessage = "This IP address has been banned and cannot create new accounts.";
                    if (bannedUser.BanExpiry.HasValue)
                    {
                        banMessage += $"\nBan expires: {bannedUser.BanExpiry.Value:MMM dd, yyyy HH:mm}";
                    }
                    banMessage += "\n\nIf you believe this is a mistake, please contact an administrator.";
                    MessageBox.Show(banMessage, "IP Banned", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Check if username already exists
                var existingUser = await _firebaseService.GetUserByUsernameAsync(UsernameTextBox.Text.Trim());
                if (existingUser != null)
                {
                    MessageBox.Show("Username already exists. Please choose a different username.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create new user
                var newUser = new User
                {
                    Username = UsernameTextBox.Text.Trim(),
                    PasswordHash = HashPassword(PasswordBox.Password),
                    FullName = UsernameTextBox.Text.Trim(), // Use username as display name
                    Email = "", // Not collected
                    Role = UserRole.User, // Default role
                    IsDJ = isDJ,
                    IsVenueOwner = isVenueOwner,
                    StreamingLink = isDJ ? StreamingLinkTextBox.Text.Trim() : "",
                    DJLogoUrl = isDJ ? DJLogoUrlTextBox.Text.Trim() : "",
                    Permissions = GetDefaultPermissionsForAccountType(isDJ, isVenueOwner),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    RegisteredIP = currentIP,
                    CurrentIP = currentIP,
                    IPHistory = new System.Collections.Generic.List<string> { currentIP }
                };

                // Save to Firebase
                string userId = await _firebaseService.AddUserAsync(newUser);
                newUser.Id = userId;

                RegisteredUser = newUser;

                // Auto-save login info to local storage for easier access
                LocalStorage.SaveLoginInfo(newUser.Username, rememberMe: true, autoLogin: false);

                string accountTypes = isDJ && isVenueOwner ? "DJ and Venue Owner" :
                                     isDJ ? "DJ" : "Venue Owner";

                MessageBox.Show($"Registration successful!\n\n" +
                    $"Welcome, {newUser.Username}!\n" +
                    $"Account Type: {accountTypes}\n\n" +
                    $"You can now log in with your credentials.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private UserPermissions GetDefaultPermissionsForAccountType(bool isDJ, bool isVenueOwner)
        {
            // Default permissions for regular users (DJ/Venue Owner)
            return new UserPermissions
            {
                // Booking permissions
                CanViewBookings = true,
                CanCreateBookings = true, // Both DJs and Venue Owners can create bookings
                CanEditBookings = true,
                CanDeleteBookings = true, // DJs can delete their own bookings, Venue Owners can delete bookings at their venues

                // Venue permissions
                CanViewVenues = true,
                CanRegisterVenues = isVenueOwner, // Only venue owners can register venues
                CanEditVenues = isVenueOwner, // Only venue owners can edit their own venues
                CanDeleteVenues = isVenueOwner, // Only venue owners can delete their own venues
                CanToggleVenueStatus = isVenueOwner, // Only venue owners can toggle their venue status

                // Admin permissions - none for regular users
                CanManageUsers = false,
                CanCustomizeApp = false,
                CanAccessSettings = true,

                // RadioBOSS permissions - can view to listen, but not control
                CanViewRadioBoss = true, // Everyone can view/listen to RadioBoss
                CanControlRadioBoss = false, // Regular users cannot control RadioBoss

                // Moderation permissions - none for regular users
                CanBanUsers = false,
                CanMuteUsers = false,
                CanViewReports = false,
                CanResolveReports = false
            };
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
