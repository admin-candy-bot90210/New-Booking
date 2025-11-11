using System;

namespace DJBookingSystem.Models
{
    public class AppSettings
    {
        public string? Id { get; set; }
        public string AppTitle { get; set; } = "DJ Booking Management System";
        public ThemeSettings Theme { get; set; } = new ThemeSettings();
        public FeatureSettings Features { get; set; } = new FeatureSettings();
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class ThemeSettings
    {
        // Primary colors
        public string HeaderBackgroundColor { get; set; } = "#2C3E50";
        public string HeaderTextColor { get; set; } = "#FFFFFF";
        public string AccentColor { get; set; } = "#3498DB";
        public string SuccessColor { get; set; } = "#27AE60";
        public string DangerColor { get; set; } = "#E74C3C";
        public string BackgroundColor { get; set; } = "#ECF0F1";

        // Text
        public string PrimaryTextColor { get; set; } = "#000000";
        public string SecondaryTextColor { get; set; } = "#7F8C8D";

        // Fonts
        public int HeaderFontSize { get; set; } = 24;
        public int NormalFontSize { get; set; } = 14;
        public string FontFamily { get; set; } = "Segoe UI";
    }

    public class FeatureSettings
    {
        // Feature toggles
        public bool EnableVenueRegistration { get; set; } = true;
        public bool EnableBookingEdit { get; set; } = true;
        public bool EnableBookingDelete { get; set; } = true;
        public bool RequireBookingApproval { get; set; } = false;
        public bool ShowVenueDetails { get; set; } = true;
        public bool AllowMultipleBookingsSameTime { get; set; } = false;

        // Time settings
        public int BookingSlotDurationHours { get; set; } = 1;
        public int MaxAdvanceBookingDays { get; set; } = 90;

        // Display settings
        public bool ShowStreamingLink { get; set; } = true;
        public bool ShowBookingCreatedDate { get; set; } = true;
        public string DateFormat { get; set; } = "MM/dd/yyyy";
    }
}
