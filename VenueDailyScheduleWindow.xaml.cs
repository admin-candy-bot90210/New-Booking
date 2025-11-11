using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class VenueDailyScheduleWindow : Window
    {
        private FirebaseService _firebaseService;
        private Venue _venue;
        private List<Booking> _allBookings = new List<Booking>();
        private DateTime _selectedDate;

        public VenueDailyScheduleWindow(FirebaseService firebaseService, Venue venue, bool stayOnTop = false)
        {
            InitializeComponent();
            _firebaseService = firebaseService;
            _venue = venue;
            _selectedDate = DateTime.Today;

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;

            VenueNameTextBlock.Text = $"Daily Schedule - {_venue.RoomName}";
            ScheduleDatePicker.SelectedDate = _selectedDate;

            LoadSchedule();
        }

        private async void LoadSchedule()
        {
            try
            {
                // Load all bookings for this venue
                var allBookings = await _firebaseService.GetAllBookingsAsync();
                _allBookings = allBookings.Where(b => b.Venue == _venue.RoomName).ToList();

                FilterBySelectedDate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load schedule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterBySelectedDate()
        {
            var dayBookings = _allBookings
                .Where(b => b.BookingDate.Date == _selectedDate.Date)
                .OrderBy(b => b.BookingDate)
                .ToList();

            ScheduleDataGrid.ItemsSource = dayBookings;

            // Update header
            DateTextBlock.Text = $"Bookings for {_selectedDate:dddd, MMMM dd, yyyy}";

            // Update statistics
            TotalBookingsText.Text = $"Total Bookings: {dayBookings.Count}";
            ConfirmedBookingsText.Text = $"Confirmed: {dayBookings.Count(b => b.Status == BookingStatus.Confirmed)}";
            PendingBookingsText.Text = $"Pending: {dayBookings.Count(b => b.Status == BookingStatus.Pending)}";
        }

        private void ScheduleDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ScheduleDatePicker.SelectedDate.HasValue)
            {
                _selectedDate = ScheduleDatePicker.SelectedDate.Value;
                FilterBySelectedDate();
            }
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedDate = DateTime.Today;
            ScheduleDatePicker.SelectedDate = _selectedDate;
            FilterBySelectedDate();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSchedule();
        }

        private async void ClearDay_Click(object sender, RoutedEventArgs e)
        {
            var dayBookings = _allBookings
                .Where(b => b.BookingDate.Date == _selectedDate.Date)
                .ToList();

            if (dayBookings.Count == 0)
            {
                MessageBox.Show($"No bookings found for {_selectedDate:dddd, MMMM dd, yyyy}.", "No Bookings", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to clear all {dayBookings.Count} booking(s) for {_selectedDate:dddd, MMMM dd, yyyy}?\n\n" +
                $"This will delete:\n" +
                $"• {dayBookings.Count(b => b.Status == BookingStatus.Confirmed)} Confirmed booking(s)\n" +
                $"• {dayBookings.Count(b => b.Status == BookingStatus.Pending)} Pending booking(s)\n\n" +
                $"This action cannot be undone!",
                "Confirm Clear Day",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    int deletedCount = 0;
                    foreach (var booking in dayBookings)
                    {
                        if (!string.IsNullOrEmpty(booking.Id))
                        {
                            await _firebaseService.DeleteBookingAsync(booking.Id);
                            deletedCount++;
                        }
                    }

                    MessageBox.Show($"Successfully cleared {deletedCount} booking(s) for {_selectedDate:dddd, MMMM dd, yyyy}.",
                        "Day Cleared", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadSchedule();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to clear day: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Log error to SysAdmin
                    await _firebaseService.LogErrorToChatAsync(ex.Message, "CLEAR001", "VenueSchedule");
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
