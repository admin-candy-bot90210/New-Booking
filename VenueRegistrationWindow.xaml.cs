using System;
using System.Windows;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    public partial class VenueRegistrationWindow : Window
    {
        public Venue? RegisteredVenue { get; private set; }

        public VenueRegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegisterVenue_Click(object sender, RoutedEventArgs e)
        {
            // Validate all required fields
            if (string.IsNullOrWhiteSpace(RoomNameTextBox.Text))
            {
                MessageBox.Show("Please enter the room name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                RoomNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(RoomDescriptionTextBox.Text))
            {
                MessageBox.Show("Please enter the room description.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                RoomDescriptionTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(OpeningHoursTextBox.Text))
            {
                MessageBox.Show("Please enter the opening hours.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                OpeningHoursTextBox.Focus();
                return;
            }

            if (AcceptTermsCheckBox.IsChecked != true)
            {
                MessageBox.Show("Please confirm the terms by checking the box.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create the venue object
            RegisteredVenue = new Venue
            {
                RoomName = RoomNameTextBox.Text.Trim(),
                RoomDescription = RoomDescriptionTextBox.Text.Trim(),
                OpeningHours = OpeningHoursTextBox.Text.Trim(),
                IsOpen = true,
                CreatedAt = DateTime.Now
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
