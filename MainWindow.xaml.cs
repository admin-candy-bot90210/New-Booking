using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;
using DJBookingSystem.ViewModels;

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
            ApplyUserPreferences();
            InitializeTimeControls();
            AutoFillDJInfo();
        }

        private void ApplyUserPreferences()
        {
            // Apply StayOnTop preference
            if (_currentUser.AppPreferences != null)
            {
                this.Topmost = _currentUser.AppPreferences.StayOnTop;
                StayOnTopMenuItem.IsChecked = _currentUser.AppPreferences.StayOnTop;
            }
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

            // Show admin tab only for users with manage users, customize, or radioboss permission
            if (perms.CanManageUsers || perms.CanCustomizeApp || perms.CanViewRadioBoss)
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

                // RadioBOSS permissions
                if (!perms.CanViewRadioBoss)
                {
                    RadioBossSectionHeader.Visibility = Visibility.Collapsed;
                    RadioBossControlButton.Visibility = Visibility.Collapsed;
                    RadioBossWebButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // User can view, check if they can control
                    if (!perms.CanControlRadioBoss)
                    {
                        // Show buttons but disable the control panel (view-only through web interface)
                        RadioBossControlButton.IsEnabled = false;
                        RadioBossControlButton.ToolTip = "You do not have permission to control RadioBOSS";
                    }
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

        private void AutoFillDJInfo()
        {
            // Auto-fill DJ information from user profile if they are a DJ
            if (_currentUser.IsDJ)
            {
                try
                {
                    // Pre-fill DJ Name with username
                    if (DJNameTextBox != null)
                    {
                        DJNameTextBox.Text = _currentUser.Username;
                    }

                    // Pre-fill Streaming Link from profile if available
                    if (StreamingLinkTextBox != null && !string.IsNullOrEmpty(_currentUser.StreamingLink))
                    {
                        StreamingLinkTextBox.Text = _currentUser.StreamingLink;
                    }
                }
                catch
                {
                    // Silently fail if controls don't exist yet
                }
            }
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

                // VALIDATION: Check if booking is in the past
                if (bookingDateTime < DateTime.Now)
                {
                    MessageBox.Show("Cannot book dates/times in the past!\n\nPlease select a future date and time.",
                        "Invalid Date", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedVenue = (Venue)VenueComboBox.SelectedItem;
                string djName = DJNameTextBox.Text.Trim();

                // VALIDATION: Check if venue is open on the selected day
                DayOfWeek selectedDay = bookingDateTime.DayOfWeek;
                if (selectedVenue.OpeningSchedule != null && !selectedVenue.OpeningSchedule.IsDayOpen(selectedDay))
                {
                    string dayName = selectedDay.ToString();
                    MessageBox.Show($"VENUE CLOSED!\n\n" +
                        $"The venue '{selectedVenue.RoomName}' is not open on {dayName}s.\n\n" +
                        $"Please select a different day when the venue is open.",
                        "Venue Not Open", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // CONFLICT DETECTION: Get all existing bookings
                var allBookings = await _firebaseService.GetAllBookingsAsync();

                // Check #1: Venue Conflict - Is this venue already booked at this time?
                var venueConflict = allBookings.FirstOrDefault(b =>
                    b.Venue == selectedVenue.RoomName &&
                    b.BookingDate == bookingDateTime);

                if (venueConflict != null)
                {
                    MessageBox.Show($"VENUE CONFLICT!\n\n" +
                        $"The venue '{selectedVenue.RoomName}' is already booked at this time by DJ {venueConflict.DJName}.\n\n" +
                        $"Please choose a different time or venue.",
                        "Booking Conflict", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Check #2: DJ Conflict - Is this DJ already booked elsewhere at this time?
                var djConflict = allBookings.FirstOrDefault(b =>
                    b.DJName.Equals(djName, StringComparison.OrdinalIgnoreCase) &&
                    b.BookingDate == bookingDateTime);

                if (djConflict != null)
                {
                    MessageBox.Show($"DJ CONFLICT!\n\n" +
                        $"DJ {djName} is already booked at '{djConflict.Venue}' at this time.\n\n" +
                        $"You cannot DJ in two venues at the same time!\n\n" +
                        $"Please choose a different time.",
                        "DJ Already Booked", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var booking = new Booking
                {
                    DJName = DJNameTextBox.Text.Trim(),
                    StreamingLink = StreamingLinkTextBox.Text.Trim(),
                    Venue = selectedVenue.RoomName,
                    BookingDate = bookingDateTime,
                    CreatedAt = DateTime.Now
                };

                string bookingId = await _firebaseService.AddBookingAsync(booking);

                // Send Discord notification if webhook is configured
                if (!string.IsNullOrEmpty(selectedVenue.DiscordWebhookUrl))
                {
                    // Calculate available slots for the day
                    var allBookings = await _firebaseService.GetAllBookingsAsync();
                    var dayBookings = allBookings.Count(b =>
                        b.Venue == selectedVenue.RoomName &&
                        b.BookingDate.Date == bookingDateTime.Date);
                    int availableSlots = Math.Max(0, 24 - dayBookings); // Assuming 24 one-hour slots per day

                    _ = DiscordService.SendBookingNotificationAsync(
                        selectedVenue.DiscordWebhookUrl,
                        booking.DJName,
                        selectedVenue.RoomName,
                        bookingDateTime,
                        availableSlots);
                }

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

            // Re-fill DJ info from profile for next booking
            AutoFillDJInfo();
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
            // DATE RANGE FILTER: Only show bookings within 1 week from today
            DateTime now = DateTime.Now;
            DateTime oneWeekFromNow = now.AddDays(7);

            // Filter: Remove past bookings and bookings beyond 1 week
            var dateFiltered = _allBookings.Where(b =>
                b.BookingDate >= now &&
                b.BookingDate <= oneWeekFromNow).ToList();

            if (VenueFilterComboBox.SelectedItem == null)
            {
                var viewModels = dateFiltered.Select(b => BookingViewModel.FromBooking(b, _currentUser, _allVenues)).ToList();
                BookingsDataGrid.ItemsSource = viewModels;
                return;
            }

            string selectedVenue = VenueFilterComboBox.SelectedItem.ToString() ?? "All";

            if (selectedVenue == "All Venues")
            {
                var viewModels = dateFiltered.Select(b => BookingViewModel.FromBooking(b, _currentUser, _allVenues)).ToList();
                BookingsDataGrid.ItemsSource = viewModels;
            }
            else
            {
                // Apply both date filter AND venue filter
                var filtered = dateFiltered.Where(b => b.Venue == selectedVenue).ToList();
                var viewModels = filtered.Select(b => BookingViewModel.FromBooking(b, _currentUser, _allVenues)).ToList();
                BookingsDataGrid.ItemsSource = viewModels;
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

        // Copy streaming link from booking list
        private void CopyStreamingLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string streamingLink)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(streamingLink))
                    {
                        MessageBox.Show("No streaming link available.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Clipboard.SetText(streamingLink);
                    MessageBox.Show("Streaming link copied to clipboard!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to copy link: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Edit booking
        private async void EditBooking_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is Booking booking)
            {
                bool stayOnTop = _currentUser.AppPreferences?.StayOnTop ?? false;
                var editWindow = new EditBookingWindow(booking, _allVenues.Where(v => v.IsOpen).ToList(), stayOnTop);
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

            bool stayOnTop = _currentUser.AppPreferences?.StayOnTop ?? false;
            var registrationWindow = new VenueRegistrationWindow(_currentUser.Username, stayOnTop);
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

        // View Daily Schedule for venue
        private void ViewDailySchedule_Click(object sender, RoutedEventArgs e)
        {
            if (VenuesDataGrid.SelectedItem is Venue venue && _firebaseService != null)
            {
                try
                {
                    bool stayOnTop = _currentUser.AppPreferences?.StayOnTop ?? false;
                    var scheduleWindow = new VenueDailyScheduleWindow(_firebaseService, venue, stayOnTop);
                    scheduleWindow.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open daily schedule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Log error to SysAdmin
                    _ = _firebaseService.LogErrorToChatAsync(ex.Message, "SCHEDULE001", _currentUser.Username);
                }
            }
            else
            {
                MessageBox.Show("Please select a venue to view the daily schedule.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                bool stayOnTop = _currentUser.AppPreferences?.StayOnTop ?? false;
                var userManagementWindow = new UserManagementWindow(_firebaseService, stayOnTop);
                userManagementWindow.ShowDialog();
            }
        }

        private void CustomizeApp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("App customization feature coming soon!\n\nThis will allow you to:\n- Change theme colors\n- Toggle features on/off\n- Customize text and labels",
                "Customize App", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // RADIOBOSS CONTROL HANDLERS

        private void OpenRadioBossControl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool stayOnTop = _currentUser.AppPreferences?.StayOnTop ?? false;
                var radioBossControl = new RadioBossControlWindow(stayOnTop);
                radioBossControl.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open RadioBOSS control panel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenRadioBossWeb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool stayOnTop = _currentUser.AppPreferences?.StayOnTop ?? false;
                var radioBossBrowser = new RadioBossBrowserWindow(stayOnTop);
                radioBossBrowser.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open RadioBOSS web interface: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // MENU HANDLERS

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void MySettings_Click(object sender, RoutedEventArgs e)
        {
            if (_firebaseService != null)
            {
                var settingsWindow = new UserSettingsWindow(_currentUser, _firebaseService);
                if (settingsWindow.ShowDialog() == true && settingsWindow.SettingsChanged)
                {
                    // Reload user preferences
                    var updatedUser = await _firebaseService.GetUserByUsernameAsync(_currentUser.Username);
                    if (updatedUser != null)
                    {
                        _currentUser = updatedUser;
                        ApplyUserPreferences();
                    }
                }
            }
        }

        private void StayOnTop_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
        }

        private void StayOnTop_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
        }

        private void Chat_Click(object sender, RoutedEventArgs e)
        {
            if (_firebaseService != null)
            {
                try
                {
                    bool stayOnTop = _currentUser.AppPreferences?.StayOnTop ?? false;
                    var chatWindow = new ChatWindow(_firebaseService, _currentUser, stayOnTop);
                    chatWindow.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open chat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Log error to Firebase for SysAdmin
                    _ = _firebaseService.LogErrorToChatAsync(ex.Message, "CHAT001", _currentUser.Username);
                }
            }
        }

        private void RadioPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (_firebaseService != null)
            {
                try
                {
                    bool stayOnTop = _currentUser.AppPreferences?.StayOnTop ?? false;
                    var radioPlayer = new RadioPlayerWindow(_firebaseService, _currentUser, stayOnTop);
                    radioPlayer.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open radio player: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Log error to Firebase for SysAdmin
                    _ = _firebaseService.LogErrorToChatAsync(ex.Message, "RADIO003", _currentUser.Username);
                }
            }
        }

        private void HelpGuide_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool stayOnTop = _currentUser.AppPreferences?.StayOnTop ?? false;
                var helpWindow = new HelpGuideWindow(stayOnTop);
                helpWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open help guide: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Log error to Firebase for SysAdmin
                if (_firebaseService != null)
                {
                    _ = _firebaseService.LogErrorToChatAsync(ex.Message, "HELP001", _currentUser.Username);
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "DJ Booking Management System\n\n" +
                "Version 2.0\n\n" +
                "Features:\n" +
                "• DJ Booking Management\n" +
                "• Venue Registration & Management\n" +
                "• User Authentication & Permissions\n" +
                "• RadioBOSS Cloud Integration\n" +
                "• Chat & Communication\n" +
                "• Customizable Themes\n" +
                "• Firebase Realtime Database\n" +
                "• Error Logging to SysAdmin\n\n" +
                "All DJ services are FREE!\n\n" +
                "🤖 Built with Claude Code",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
