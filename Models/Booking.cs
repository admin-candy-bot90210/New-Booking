using System;

namespace DJBookingSystem.Models
{
    public class Booking
    {
        public string? Id { get; set; }
        public string DJName { get; set; } = string.Empty;
        public string StreamingLink { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
