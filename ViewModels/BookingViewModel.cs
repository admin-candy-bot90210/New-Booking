using System;
using DJBookingSystem.Models;

namespace DJBookingSystem.ViewModels
{
    public class BookingViewModel
    {
        public string? Id { get; set; }
        public string DJName { get; set; } = string.Empty;
        public string StreamingLink { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public BookingStatus Status { get; set; }
        public int DurationHours { get; set; }

        // For copy button functionality
        public string ActualStreamingLink { get; set; } = string.Empty;
        public bool CanCopyStreamingLink { get; set; } = false;

        public static BookingViewModel FromBooking(Booking booking, User currentUser, System.Collections.Generic.List<Venue> allVenues)
        {
            var venue = allVenues.Find(v => v.RoomName == booking.Venue);
            bool canSeeStreamingLink = false;

            // Check if user can see streaming link
            if (currentUser.Role == UserRole.SysAdmin || currentUser.Role == UserRole.Manager)
            {
                canSeeStreamingLink = true;
            }
            else if (currentUser.IsVenueOwner && venue != null && venue.OwnerUsername == currentUser.Username)
            {
                canSeeStreamingLink = true;
            }

            return new BookingViewModel
            {
                Id = booking.Id,
                DJName = booking.DJName,
                StreamingLink = canSeeStreamingLink ? booking.StreamingLink : "[Hidden - Venue Owner/Admin Only]",
                Venue = booking.Venue,
                BookingDate = booking.BookingDate,
                CreatedAt = booking.CreatedAt,
                Status = booking.Status,
                DurationHours = booking.DurationHours,
                ActualStreamingLink = booking.StreamingLink,
                CanCopyStreamingLink = canSeeStreamingLink
            };
        }
    }
}
