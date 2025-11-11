using System;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    public partial class EditUserWindow : Window
    {
        private User? _existingUser;
        public User? UpdatedUser { get; private set; }
        public bool IsNewUser { get; private set; }

        // Constructor for new user
        public EditUserWindow(bool stayOnTop = false)
        {
            InitializeComponent();
            IsNewUser = true;
            TitleTextBlock.Text = "Add New User";
            PasswordHintTextBlock.Visibility = Visibility.Collapsed;

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;
        }

        // Constructor for editing existing user
        public EditUserWindow(User user, bool stayOnTop = false)
        {
            InitializeComponent();
            IsNewUser = false;
            _existingUser = user;
            TitleTextBlock.Text = "Edit User";
            PasswordLabel.Text = "New Password: (optional)";
            PasswordHintTextBlock.Visibility = Visibility.Visible;

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;

            LoadUserData();
        }

        private void LoadUserData()
        {
            if (_existingUser == null) return;

            UsernameTextBox.Text = _existingUser.Username;
            FullNameTextBox.Text = _existingUser.FullName;
            EmailTextBox.Text = _existingUser.Email;
            IsActiveCheckBox.IsChecked = _existingUser.IsActive;

            // Set role
            foreach (ComboBoxItem item in RoleComboBox.Items)
            {
                if (item.Tag.ToString() == _existingUser.Role.ToString())
                {
                    RoleComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Please enter the full name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Please enter an email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Password validation for new users
            if (IsNewUser && string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Please enter a password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Email validation
            if (!EmailTextBox.Text.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get selected role
            var selectedRole = UserRole.User;
            if (RoleComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                Enum.TryParse(selectedItem.Tag.ToString(), out selectedRole);
            }

            // Create or update user object
            if (IsNewUser)
            {
                UpdatedUser = new User
                {
                    Username = UsernameTextBox.Text.Trim(),
                    PasswordHash = LoginWindow.HashPassword(PasswordBox.Password),
                    FullName = FullNameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Role = selectedRole,
                    Permissions = GetDefaultPermissionsForRole(selectedRole),
                    IsActive = IsActiveCheckBox.IsChecked ?? true,
                    CreatedAt = DateTime.Now
                };
            }
            else
            {
                UpdatedUser = new User
                {
                    Id = _existingUser?.Id,
                    Username = UsernameTextBox.Text.Trim(),
                    PasswordHash = string.IsNullOrWhiteSpace(PasswordBox.Password)
                        ? _existingUser?.PasswordHash ?? ""
                        : LoginWindow.HashPassword(PasswordBox.Password),
                    FullName = FullNameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Role = selectedRole,
                    Permissions = _existingUser?.Permissions ?? GetDefaultPermissionsForRole(selectedRole),
                    IsActive = IsActiveCheckBox.IsChecked ?? true,
                    CreatedAt = _existingUser?.CreatedAt ?? DateTime.Now,
                    LastLogin = _existingUser?.LastLogin ?? DateTime.MinValue
                };
            }

            DialogResult = true;
            Close();
        }

        private UserPermissions GetDefaultPermissionsForRole(UserRole role)
        {
            switch (role)
            {
                case UserRole.SysAdmin:
                    // SysAdmin has ALL permissions
                    return new UserPermissions
                    {
                        CanViewBookings = true,
                        CanCreateBookings = true,
                        CanEditBookings = true,
                        CanDeleteBookings = true,
                        CanViewVenues = true,
                        CanRegisterVenues = true,
                        CanEditVenues = true,
                        CanDeleteVenues = true,
                        CanToggleVenueStatus = true,
                        CanManageUsers = true,
                        CanCustomizeApp = true,
                        CanAccessSettings = true,
                        CanViewRadioBoss = true,
                        CanControlRadioBoss = true,
                        CanBanUsers = true,
                        CanMuteUsers = true,
                        CanViewReports = true,
                        CanResolveReports = true
                    };

                case UserRole.Manager:
                    // Manager has ALL permissions EXCEPT CanCustomizeApp
                    return new UserPermissions
                    {
                        CanViewBookings = true,
                        CanCreateBookings = true,
                        CanEditBookings = true,
                        CanDeleteBookings = true,
                        CanViewVenues = true,
                        CanRegisterVenues = true,
                        CanEditVenues = true,
                        CanDeleteVenues = true,
                        CanToggleVenueStatus = true,
                        CanManageUsers = true,
                        CanCustomizeApp = false, // Only difference from SysAdmin
                        CanAccessSettings = true,
                        CanViewRadioBoss = true,
                        CanControlRadioBoss = true,
                        CanBanUsers = true,
                        CanMuteUsers = true,
                        CanViewReports = true,
                        CanResolveReports = true
                    };

                case UserRole.User:
                default:
                    // Regular users (DJ/Venue Owner) - limited permissions
                    return new UserPermissions
                    {
                        CanViewBookings = true,
                        CanCreateBookings = true,
                        CanEditBookings = true,
                        CanDeleteBookings = true, // Can delete their own bookings/bookings at their venues
                        CanViewVenues = true,
                        CanRegisterVenues = false, // Set based on IsVenueOwner in registration
                        CanEditVenues = false, // Set based on IsVenueOwner in registration
                        CanDeleteVenues = false, // Set based on IsVenueOwner in registration
                        CanToggleVenueStatus = false, // Set based on IsVenueOwner in registration
                        CanManageUsers = false,
                        CanCustomizeApp = false,
                        CanAccessSettings = true,
                        CanViewRadioBoss = true, // Can view/listen to RadioBoss
                        CanControlRadioBoss = false,
                        CanBanUsers = false,
                        CanMuteUsers = false,
                        CanViewReports = false,
                        CanResolveReports = false
                    };
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
