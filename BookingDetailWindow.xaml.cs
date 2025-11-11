using System;
using System.Diagnostics;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class BookingDetailWindow : Window
    {
        private FirebaseService _firebaseService;
        private User _currentUser;
        private Booking _booking;
        private bool _canViewStreamingLink = false;

        public BookingDetailWindow(FirebaseService firebaseService, User currentUser, Booking booking)
        {
            InitializeComponent();
            _firebaseService = firebaseService;
            _currentUser = currentUser;
            _booking = booking;

            LoadBookingDetails();
        }

        private async void LoadBookingDetails()
        {
            // Set basic information (visible to everyone)
            DJNameTextBlock.Text = _booking.DJName;
            VenueTextBlock.Text = _booking.Venue;
            DateTextBlock.Text = _booking.BookingDate.ToString("MMMM dd, yyyy 'at' hh:mm tt");
            CreatedTextBlock.Text = $"Booking created on {_booking.CreatedAt:MMM dd, yyyy}";

            // Determine if user can view streaming link
            bool isAdmin = _currentUser.Role == UserRole.SysAdmin || _currentUser.Role == UserRole.Manager;
            bool isVenueOwner = _currentUser.IsVenueOwner;

            // Check if current user owns this venue
            bool ownsThisVenue = false;
            if (isVenueOwner && !isAdmin)
            {
                try
                {
                    var venues = await _firebaseService.GetAllVenuesAsync();
                    ownsThisVenue = venues.Exists(v => v.RoomName == _booking.Venue);

                    // Additional check: We could store venue ownership in the future
                    // For now, venue owners see their registered venues
                }
                catch
                {
                    ownsThisVenue = false;
                }
            }

            // Determine access level
            if (isAdmin)
            {
                _canViewStreamingLink = true;
                StreamingLinkPanel.Visibility = Visibility.Visible;
                StreamingLinkTextBox.Text = _booking.StreamingLink;
                AccessInfoTextBlock.Text = "ℹ️ Admin Access: You can view all booking details including streaming links.";
                BookingSubtitleTextBlock.Text = "Full administrative access";
            }
            else if (isVenueOwner && ownsThisVenue)
            {
                _canViewStreamingLink = true;
                StreamingLinkPanel.Visibility = Visibility.Visible;
                StreamingLinkTextBox.Text = _booking.StreamingLink;
                AccessInfoTextBlock.Text = "ℹ️ Venue Owner Access: You can view full details for bookings at your venue including DJ streaming links.";
                BookingSubtitleTextBlock.Text = $"Venue Owner: {_booking.Venue}";
            }
            else
            {
                _canViewStreamingLink = false;
                StreamingLinkPanel.Visibility = Visibility.Collapsed;
                AccessInfoTextBlock.Text = "ℹ️ Limited Access: Streaming links are only visible to venue owners (for their venues) and administrators.";
                BookingSubtitleTextBlock.Text = "Public booking information";
            }
        }

        private void CopyStreamingLink_Click(object sender, RoutedEventArgs e)
        {
            if (!_canViewStreamingLink)
            {
                MessageBox.Show("You don't have permission to access the streaming link.", "Access Denied",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Clipboard.SetText(_booking.StreamingLink);
                MessageBox.Show("Streaming link copied to clipboard!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy link: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenStreamingLink_Click(object sender, RoutedEventArgs e)
        {
            if (!_canViewStreamingLink)
            {
                MessageBox.Show("You don't have permission to access the streaming link.", "Access Denied",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _booking.StreamingLink,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open link: {ex.Message}\n\nLink: {_booking.StreamingLink}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
