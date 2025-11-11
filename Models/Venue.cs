using System;

namespace DJBookingSystem.Models
{
    public class Venue
    {
        public string? Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string RoomDescription { get; set; } = string.Empty;
        public string OpeningHours { get; set; } = string.Empty; // e.g., "Mon-Fri: 6PM-2AM, Sat-Sun: 8PM-4AM"
        public bool IsOpen { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string DiscordWebhookUrl { get; set; } = string.Empty;
        public string OwnerUsername { get; set; } = string.Empty;
    }
}
