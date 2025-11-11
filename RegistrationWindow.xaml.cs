using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

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

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
                {
                    MessageBox.Show("Please enter your full name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                {
                    MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !EmailTextBox.Text.Contains("@"))
                {
                    MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                if (AgreeTermsCheckBox.IsChecked != true)
                {
                    MessageBox.Show("Please agree to the Terms of Service and Privacy Policy.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    FullName = FullNameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Role = UserRole.User, // Default role
                    IsDJ = isDJ,
                    IsVenueOwner = isVenueOwner,
                    Permissions = GetDefaultPermissionsForAccountType(isDJ, isVenueOwner),
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                // Save to Firebase
                string userId = await _firebaseService.AddUserAsync(newUser);
                newUser.Id = userId;

                RegisteredUser = newUser;

                MessageBox.Show($"Registration successful!\n\nWelcome, {newUser.FullName}!\n\nYou can now log in with your credentials.",
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
            return new UserPermissions
            {
                // Booking permissions - DJs can view and create
                CanViewBookings = true,
                CanCreateBookings = isDJ, // Only DJs can create bookings
                CanEditBookings = isDJ,
                CanDeleteBookings = false,

                // Venue permissions - Venue owners can register and manage
                CanViewVenues = true,
                CanRegisterVenues = isVenueOwner, // Only venue owners can register venues
                CanEditVenues = isVenueOwner,
                CanDeleteVenues = false,
                CanToggleVenueStatus = isVenueOwner,

                // Admin permissions - none for regular users
                CanManageUsers = false,
                CanCustomizeApp = false,
                CanAccessSettings = true,

                // RadioBOSS permissions - none for regular users
                CanViewRadioBoss = false,
                CanControlRadioBoss = false
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
