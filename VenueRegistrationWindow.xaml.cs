using System;
using System.Windows;
using DJBookingSystem.Models;

namespace DJBookingSystem
{
    public partial class VenueRegistrationWindow : Window
    {
        public Venue? RegisteredVenue { get; private set; }
        private string _ownerUsername;

        public VenueRegistrationWindow(string ownerUsername, bool stayOnTop = false)
        {
            InitializeComponent();
            _ownerUsername = ownerUsername;

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;
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

            // Validate that at least one day is selected
            if (MondayCheckBox.IsChecked != true &&
                TuesdayCheckBox.IsChecked != true &&
                WednesdayCheckBox.IsChecked != true &&
                ThursdayCheckBox.IsChecked != true &&
                FridayCheckBox.IsChecked != true &&
                SaturdayCheckBox.IsChecked != true &&
                SundayCheckBox.IsChecked != true)
            {
                MessageBox.Show("Please select at least one day when the venue is open.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AcceptTermsCheckBox.IsChecked != true)
            {
                MessageBox.Show("Please confirm the terms by checking the box.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate Discord webhook if provided
            string discordWebhook = DiscordWebhookTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(discordWebhook))
            {
                if (!discordWebhook.StartsWith("https://discord.com/api/webhooks/") &&
                    !discordWebhook.StartsWith("https://discordapp.com/api/webhooks/"))
                {
                    MessageBox.Show("Invalid Discord webhook URL. It should start with https://discord.com/api/webhooks/",
                        "Invalid Webhook", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DiscordWebhookTextBox.Focus();
                    return;
                }
            }

            // Build opening schedule from checkboxes
            var openingSchedule = new VenueOpeningHours
            {
                Monday = new DaySchedule { IsOpen = MondayCheckBox.IsChecked == true },
                Tuesday = new DaySchedule { IsOpen = TuesdayCheckBox.IsChecked == true },
                Wednesday = new DaySchedule { IsOpen = WednesdayCheckBox.IsChecked == true },
                Thursday = new DaySchedule { IsOpen = ThursdayCheckBox.IsChecked == true },
                Friday = new DaySchedule { IsOpen = FridayCheckBox.IsChecked == true },
                Saturday = new DaySchedule { IsOpen = SaturdayCheckBox.IsChecked == true },
                Sunday = new DaySchedule { IsOpen = SundayCheckBox.IsChecked == true }
            };

            // Build legacy opening hours string for display
            var openDays = new System.Collections.Generic.List<string>();
            if (MondayCheckBox.IsChecked == true) openDays.Add("Mon");
            if (TuesdayCheckBox.IsChecked == true) openDays.Add("Tue");
            if (WednesdayCheckBox.IsChecked == true) openDays.Add("Wed");
            if (ThursdayCheckBox.IsChecked == true) openDays.Add("Thu");
            if (FridayCheckBox.IsChecked == true) openDays.Add("Fri");
            if (SaturdayCheckBox.IsChecked == true) openDays.Add("Sat");
            if (SundayCheckBox.IsChecked == true) openDays.Add("Sun");

            string openingHoursText = $"Open: {string.Join(", ", openDays)}";

            // Create the venue object
            RegisteredVenue = new Venue
            {
                RoomName = RoomNameTextBox.Text.Trim(),
                RoomDescription = RoomDescriptionTextBox.Text.Trim(),
                OpeningHours = openingHoursText,
                OpeningSchedule = openingSchedule,
                IsOpen = true,
                CreatedAt = DateTime.Now,
                DiscordWebhookUrl = discordWebhook,
                OwnerUsername = _ownerUsername
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
