using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class MainWindow : Window
    {
        private FirebaseService? _firebaseService;
        private User _currentUser;
        private AppSettings _appSettings;
        private List<Booking> _allBookings = new List<Booking>();
        private List<Venue> _allVenues = new List<Venue>();

        public MainWindow(FirebaseService firebaseService, User currentUser, AppSettings appSettings)
        {
            InitializeComponent();
            _firebaseService = firebaseService;
            _currentUser = currentUser;
            _appSettings = appSettings;

            ApplySettings();
            ApplyPermissions();
            InitializeTimeControls();
        }

        private void ApplySettings()
        {
            // Apply app title
            this.Title = _appSettings.AppTitle;

            // Show current user info if admin tab exists
            try
            {
                if (CurrentUserInfoTextBlock != null)
                {
                    CurrentUserInfoTextBlock.Text = $"Logged in as: {_currentUser.FullName} ({_currentUser.Username})\n" +
                                                   $"Role: {_currentUser.Role}\n" +
                                                   $"Last Login: {_currentUser.LastLogin:MM/dd/yyyy HH:mm}";
                }
            }
            catch { }
        }

        private void ApplyPermissions()
        {
            var perms = _currentUser.Permissions;

            // Show/hide tabs based on permissions
            if (!perms.CanViewBookings)
                DJBookingsTab.Visibility = Visibility.Collapsed;

            if (!perms.CanCreateBookings)
                NewDJBookingTab.Visibility = Visibility.Collapsed;

            if (!perms.CanViewVenues)
                ManageVenuesTab.Visibility = Visibility.Collapsed;

            // Show admin tab only for users with manage users permission
            if (perms.CanManageUsers || perms.CanCustomizeApp)
                AdminTab.Visibility = Visibility.Visible;
            else
                AdminTab.Visibility = Visibility.Collapsed;

            // Disable specific buttons based on permissions
            try
            {
                if (!perms.CanEditBookings)
                {
                    EditBookingButton.IsEnabled = false;
                }

                if (!perms.CanDeleteBookings)
                {
                    DeleteBookingButton.IsEnabled = false;
                }

                if (!perms.CanRegisterVenues)
                {
                    RegisterVenueButton.Visibility = Visibility.Collapsed;
                }

                if (!perms.CanEditVenues)
                {
                    //ViewVenueDetailsButton.IsEnabled = false;
                }

                if (!perms.CanDeleteVenues)
                {
                    DeleteVenueButton.IsEnabled = false;
                }

                if (!perms.CanToggleVenueStatus)
                {
                    ToggleVenueButton.IsEnabled = false;
                }
            }
            catch { }
        }

        private void InitializeTimeControls()
        {
            // Populate hour dropdown (0-23)
            for (int i = 0; i < 24; i++)
            {
                HourComboBox.Items.Add(i.ToString("D2"));
            }

            // Each DJ slot is 1 hour - minute is always :00
            MinuteComboBox.Items.Add("00");
        }


        // Add new booking
        private async void AddBooking_Click(object sender, RoutedEventArgs e)
        {
            if (_firebaseService == null)
            {
                MessageBox.Show("Please connect to Firebase first in the Settings tab.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(DJNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(StreamingLinkTextBox.Text) ||
                    VenueComboBox.SelectedItem == null ||
                    BookingDatePicker.SelectedDate == null ||
                    HourComboBox.SelectedItem == null ||
                    MinuteComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Build the booking date with time
                int hour = int.Parse(HourComboBox.SelectedItem.ToString() ?? "0");
                int minute = int.Parse(MinuteComboBox.SelectedItem.ToString() ?? "0");
                DateTime bookingDateTime = BookingDatePicker.SelectedDate.Value.Date
                    .AddHours(hour)
                    .AddMinutes(minute);

                var booking = new Booking
                {
                    DJName = DJNameTextBox.Text.Trim(),
                    StreamingLink = StreamingLinkTextBox.Text.Trim(),
                    Venue = ((Venue)VenueComboBox.SelectedItem).RoomName,
                    BookingDate = bookingDateTime,
                    CreatedAt = DateTime.Now
                };

                string bookingId = await _firebaseService.AddBookingAsync(booking);

                MessageBox.Show($"DJ Booking added successfully! ID: {bookingId}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear form
                ClearBookingForm();

                // Refresh bookings list
                await LoadBookingsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Clear booking form
        private void ClearBookingForm()
        {
            DJNameTextBox.Clear();
            StreamingLinkTextBox.Clear();
            VenueComboBox.SelectedIndex = -1;
            BookingDatePicker.SelectedDate = null;
            HourComboBox.SelectedIndex = 18; // Default to 6 PM
            MinuteComboBox.SelectedIndex = 0; // Default to :00
        }

        // Refresh bookings list
        private async void RefreshBookings_Click(object sender, RoutedEventArgs e)
        {
            await LoadBookingsAsync();
        }

        // Load bookings from Firebase
        private async System.Threading.Tasks.Task LoadBookingsAsync()
        {
            if (_firebaseService == null)
            {
                MessageBox.Show("Please connect to Firebase first in the Settings tab.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _allBookings = await _firebaseService.GetAllBookingsAsync();
                ApplyVenueFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load bookings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Apply venue filter
        private void VenueFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyVenueFilter();
        }

        // Apply filter to bookings list
        private void ApplyVenueFilter()
        {
            if (VenueFilterComboBox.SelectedItem == null)
            {
                BookingsDataGrid.ItemsSource = _allBookings;
                return;
            }

            string selectedVenue = VenueFilterComboBox.SelectedItem.ToString() ?? "All";

            if (selectedVenue == "All Venues")
            {
                BookingsDataGrid.ItemsSource = _allBookings;
            }
            else
            {
                BookingsDataGrid.ItemsSource = _allBookings.Where(b => b.Venue == selectedVenue).ToList();
            }
        }

        // View booking details
        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is Booking booking)
            {
                string details = $"DJ Booking Details:\n\n" +
                               $"DJ Name: {booking.DJName}\n" +
                               $"Streaming Link: {booking.StreamingLink}\n" +
                               $"Venue: {booking.Venue}\n" +
                               $"Booking Date: {booking.BookingDate:MM/dd/yyyy HH:mm}\n" +
                               $"Created: {booking.CreatedAt:MM/dd/yyyy HH:mm}";

                MessageBox.Show(details, "Booking Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a booking to view.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Edit booking
        private async void EditBooking_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is Booking booking)
            {
                var editWindow = new EditBookingWindow(booking, _allVenues.Where(v => v.IsOpen).ToList());
                if (editWindow.ShowDialog() == true && _firebaseService != null)
                {
                    try
                    {
                        await _firebaseService.UpdateBookingAsync(booking.Id ?? "", editWindow.UpdatedBooking);
                        MessageBox.Show("Booking updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadBookingsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a booking to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Delete booking
        private async void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is Booking booking)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the booking for {booking.DJName}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes && _firebaseService != null)
                {
                    try
                    {
                        await _firebaseService.DeleteBookingAsync(booking.Id ?? "");
                        MessageBox.Show("Booking deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadBookingsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete booking: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a booking to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // VENUE MANAGEMENT

        // Load venues from Firebase
        private async System.Threading.Tasks.Task LoadVenuesAsync()
        {
            if (_firebaseService == null) return;

            try
            {
                _allVenues = await _firebaseService.GetAllVenuesAsync();
                VenuesDataGrid.ItemsSource = _allVenues;

                // Update venue combo boxes
                UpdateVenueComboBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load venues: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateVenueComboBoxes()
        {
            var openVenues = _allVenues.Where(v => v.IsOpen).ToList();

            // Update booking form venue dropdown
            VenueComboBox.ItemsSource = openVenues;
            VenueComboBox.DisplayMemberPath = "RoomName";

            // Update filter dropdown
            var filterItems = new List<string> { "All Venues" };
            filterItems.AddRange(_allVenues.Select(v => v.RoomName));
            VenueFilterComboBox.ItemsSource = filterItems;
            VenueFilterComboBox.SelectedIndex = 0;
        }

        // Refresh venues
        private async void RefreshVenues_Click(object sender, RoutedEventArgs e)
        {
            await LoadVenuesAsync();
        }

        // Register new venue (with full form)
        private async void RegisterVenue_Click(object sender, RoutedEventArgs e)
        {
            if (_firebaseService == null)
            {
                MessageBox.Show("Please connect to Firebase first in the Settings tab.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var registrationWindow = new VenueRegistrationWindow();
            if (registrationWindow.ShowDialog() == true && registrationWindow.RegisteredVenue != null)
            {
                try
                {
                    string venueId = await _firebaseService.AddVenueAsync(registrationWindow.RegisteredVenue);
                    MessageBox.Show($"Room '{registrationWindow.RegisteredVenue.RoomName}' registered successfully!\n\nThank you for offering your venue for free DJ services!\n\nRemember: Each DJ slot is 1 hour long.",
                        "Registration Successful",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await LoadVenuesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to register venue: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // View venue details
        private void ViewVenueDetails_Click(object sender, RoutedEventArgs e)
        {
            if (VenuesDataGrid.SelectedItem is Venue venue)
            {
                ShowVenueDetails(venue);
            }
            else
            {
                MessageBox.Show("Please select a venue to view.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Double-click to view venue details
        private void VenueDetails_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (VenuesDataGrid.SelectedItem is Venue venue)
            {
                ShowVenueDetails(venue);
            }
        }

        // Show venue details dialog
        private void ShowVenueDetails(Venue venue)
        {
            string details = $"Room/Venue Details:\n\n" +
                           $"Room Name: {venue.RoomName}\n" +
                           $"Description: {venue.RoomDescription}\n" +
                           $"Opening Hours: {venue.OpeningHours}\n\n" +
                           $"Status: {(venue.IsOpen ? "Open for Bookings" : "Closed")}\n" +
                           $"Registered: {venue.CreatedAt:MM/dd/yyyy HH:mm}\n\n" +
                           $"Note: Each DJ slot is 1 hour long";

            MessageBox.Show(details, "Room/Venue Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Toggle venue open/closed
        private async void ToggleVenue_Click(object sender, RoutedEventArgs e)
        {
            if (VenuesDataGrid.SelectedItem is Venue venue && _firebaseService != null)
            {
                try
                {
                    venue.IsOpen = !venue.IsOpen;
                    await _firebaseService.UpdateVenueAsync(venue.Id ?? "", venue);
                    MessageBox.Show($"Room '{venue.RoomName}' is now {(venue.IsOpen ? "Open" : "Closed")}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadVenuesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update venue: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a venue to toggle.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Delete venue
        private async void DeleteVenue_Click(object sender, RoutedEventArgs e)
        {
            if (VenuesDataGrid.SelectedItem is Venue venue && _firebaseService != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete room '{venue.RoomName}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _firebaseService.DeleteVenueAsync(venue.Id ?? "");
                        MessageBox.Show("Venue deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadVenuesAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete venue: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a venue to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ADMIN PANEL HANDLERS

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            if (_firebaseService != null)
            {
                var userManagementWindow = new UserManagementWindow(_firebaseService);
                userManagementWindow.ShowDialog();
            }
        }

        private void CustomizeApp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("App customization feature coming soon!\n\nThis will allow you to:\n- Change theme colors\n- Toggle features on/off\n- Customize text and labels",
                "Customize App", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
