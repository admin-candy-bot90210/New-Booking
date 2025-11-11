using System;
using System.Collections.Generic;

namespace DJBookingSystem.Models
{
    public class Venue
    {
        public string? Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string RoomDescription { get; set; } = string.Empty;
        public string OpeningHours { get; set; } = string.Empty; // Legacy: e.g., "Mon-Fri: 6PM-2AM, Sat-Sun: 8PM-4AM"
        public bool IsOpen { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string DiscordWebhookUrl { get; set; } = string.Empty;
        public string OwnerUsername { get; set; } = string.Empty;

        // Day-specific opening hours (new structured format)
        public VenueOpeningHours OpeningSchedule { get; set; } = new VenueOpeningHours();
    }

    public class VenueOpeningHours
    {
        public DaySchedule Monday { get; set; } = new DaySchedule();
        public DaySchedule Tuesday { get; set; } = new DaySchedule();
        public DaySchedule Wednesday { get; set; } = new DaySchedule();
        public DaySchedule Thursday { get; set; } = new DaySchedule();
        public DaySchedule Friday { get; set; } = new DaySchedule();
        public DaySchedule Saturday { get; set; } = new DaySchedule();
        public DaySchedule Sunday { get; set; } = new DaySchedule();

        // Helper method to get schedule for a specific day
        public DaySchedule GetScheduleForDay(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => Monday,
                DayOfWeek.Tuesday => Tuesday,
                DayOfWeek.Wednesday => Wednesday,
                DayOfWeek.Thursday => Thursday,
                DayOfWeek.Friday => Friday,
                DayOfWeek.Saturday => Saturday,
                DayOfWeek.Sunday => Sunday,
                _ => new DaySchedule()
            };
        }

        // Helper method to check if a day is open
        public bool IsDayOpen(DayOfWeek day)
        {
            return GetScheduleForDay(day).IsOpen;
        }
    }

    public class DaySchedule
    {
        public bool IsOpen { get; set; } = false;
        public string OpenTime { get; set; } = "20:00"; // Default 8PM (24-hour format)
        public string CloseTime { get; set; } = "02:00"; // Default 2AM next day (24-hour format)
    }
}
