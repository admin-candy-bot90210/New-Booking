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
        public EditUserWindow()
        {
            InitializeComponent();
            IsNewUser = true;
            TitleTextBlock.Text = "Add New User";
            PasswordHintTextBlock.Visibility = Visibility.Collapsed;
        }

        // Constructor for editing existing user
        public EditUserWindow(User user)
        {
            InitializeComponent();
            IsNewUser = false;
            _existingUser = user;
            TitleTextBlock.Text = "Edit User";
            PasswordLabel.Text = "New Password: (optional)";
            PasswordHintTextBlock.Visibility = Visibility.Visible;

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
                        CanControlRadioBoss = true
                    };

                case UserRole.Manager:
                    return new UserPermissions
                    {
                        CanViewBookings = true,
                        CanCreateBookings = true,
                        CanEditBookings = true,
                        CanDeleteBookings = true,
                        CanViewVenues = true,
                        CanRegisterVenues = true,
                        CanEditVenues = true,
                        CanDeleteVenues = false,
                        CanToggleVenueStatus = true,
                        CanManageUsers = false,
                        CanCustomizeApp = false,
                        CanAccessSettings = true,
                        CanViewRadioBoss = false,
                        CanControlRadioBoss = false
                    };

                case UserRole.User:
                default:
                    return new UserPermissions
                    {
                        CanViewBookings = true,
                        CanCreateBookings = true,
                        CanEditBookings = true,
                        CanDeleteBookings = false,
                        CanViewVenues = true,
                        CanRegisterVenues = true,
                        CanEditVenues = false,
                        CanDeleteVenues = false,
                        CanToggleVenueStatus = false,
                        CanManageUsers = false,
                        CanCustomizeApp = false,
                        CanAccessSettings = true,
                        CanViewRadioBoss = false,
                        CanControlRadioBoss = false
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
