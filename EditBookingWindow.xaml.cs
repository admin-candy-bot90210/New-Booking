using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    public partial class EditBookingWindow : Window
    {
        private Booking _originalBooking;
        private List<Venue> _venues;
        public Booking UpdatedBooking { get; private set; }

        public EditBookingWindow(Booking booking, List<Venue> venues, bool stayOnTop = false)
        {
            InitializeComponent();
            _originalBooking = booking;
            _venues = venues;
            UpdatedBooking = new Booking();

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;

            InitializeTimeControls();
            LoadBookingData();
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

        private void LoadBookingData()
        {
            DJNameTextBox.Text = _originalBooking.DJName;
            StreamingLinkTextBox.Text = _originalBooking.StreamingLink;
            BookingDatePicker.SelectedDate = _originalBooking.BookingDate.Date;

            // Set time controls
            HourComboBox.SelectedItem = _originalBooking.BookingDate.Hour.ToString("D2");
            int minute = _originalBooking.BookingDate.Minute;
            int minuteIndex = minute / 15; // 0, 15, 30, 45
            MinuteComboBox.SelectedIndex = minuteIndex;

            // Set venue combo box
            VenueComboBox.ItemsSource = _venues;
            VenueComboBox.DisplayMemberPath = "RoomName";

            var selectedVenue = _venues.FirstOrDefault(v => v.RoomName == _originalBooking.Venue);
            if (selectedVenue != null)
                VenueComboBox.SelectedItem = selectedVenue;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
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

            // Update booking object
            UpdatedBooking = new Booking
            {
                Id = _originalBooking.Id,
                DJName = DJNameTextBox.Text.Trim(),
                StreamingLink = StreamingLinkTextBox.Text.Trim(),
                Venue = ((Venue)VenueComboBox.SelectedItem).RoomName,
                BookingDate = bookingDateTime,
                CreatedAt = _originalBooking.CreatedAt
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
