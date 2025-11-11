using System.Windows;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    public partial class EditPermissionsWindow : Window
    {
        private User _user;
        public UserPermissions? UpdatedPermissions { get; private set; }

        public EditPermissionsWindow(User user, bool stayOnTop = false)
        {
            InitializeComponent();
            _user = user;
            UserInfoTextBlock.Text = $"User: {user.Username} ({user.FullName})";

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;

            LoadPermissions();
        }

        private void LoadPermissions()
        {
            var perms = _user.Permissions ?? new UserPermissions();

            // Booking permissions
            CanViewBookingsCheckBox.IsChecked = perms.CanViewBookings;
            CanCreateBookingsCheckBox.IsChecked = perms.CanCreateBookings;
            CanEditBookingsCheckBox.IsChecked = perms.CanEditBookings;
            CanDeleteBookingsCheckBox.IsChecked = perms.CanDeleteBookings;

            // Venue permissions
            CanViewVenuesCheckBox.IsChecked = perms.CanViewVenues;
            CanRegisterVenuesCheckBox.IsChecked = perms.CanRegisterVenues;
            CanEditVenuesCheckBox.IsChecked = perms.CanEditVenues;
            CanDeleteVenuesCheckBox.IsChecked = perms.CanDeleteVenues;
            CanToggleVenueStatusCheckBox.IsChecked = perms.CanToggleVenueStatus;

            // Admin permissions
            CanManageUsersCheckBox.IsChecked = perms.CanManageUsers;
            CanCustomizeAppCheckBox.IsChecked = perms.CanCustomizeApp;
            CanAccessSettingsCheckBox.IsChecked = perms.CanAccessSettings;

            // RadioBOSS permissions
            CanViewRadioBossCheckBox.IsChecked = perms.CanViewRadioBoss;
            CanControlRadioBossCheckBox.IsChecked = perms.CanControlRadioBoss;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            UpdatedPermissions = new UserPermissions
            {
                // Booking permissions
                CanViewBookings = CanViewBookingsCheckBox.IsChecked ?? false,
                CanCreateBookings = CanCreateBookingsCheckBox.IsChecked ?? false,
                CanEditBookings = CanEditBookingsCheckBox.IsChecked ?? false,
                CanDeleteBookings = CanDeleteBookingsCheckBox.IsChecked ?? false,

                // Venue permissions
                CanViewVenues = CanViewVenuesCheckBox.IsChecked ?? false,
                CanRegisterVenues = CanRegisterVenuesCheckBox.IsChecked ?? false,
                CanEditVenues = CanEditVenuesCheckBox.IsChecked ?? false,
                CanDeleteVenues = CanDeleteVenuesCheckBox.IsChecked ?? false,
                CanToggleVenueStatus = CanToggleVenueStatusCheckBox.IsChecked ?? false,

                // Admin permissions
                CanManageUsers = CanManageUsersCheckBox.IsChecked ?? false,
                CanCustomizeApp = CanCustomizeAppCheckBox.IsChecked ?? false,
                CanAccessSettings = CanAccessSettingsCheckBox.IsChecked ?? false,

                // RadioBOSS permissions
                CanViewRadioBoss = CanViewRadioBossCheckBox.IsChecked ?? false,
                CanControlRadioBoss = CanControlRadioBossCheckBox.IsChecked ?? false
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
