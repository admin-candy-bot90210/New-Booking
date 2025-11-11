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
        // Primary colors - Matrix/Terminal style (Black background with fluorescent green)
        public string HeaderBackgroundColor { get; set; } = "#000000"; // Pure black
        public string HeaderTextColor { get; set; } = "#00FF00"; // Fluorescent green
        public string AccentColor { get; set; } = "#00FF00"; // Fluorescent green
        public string SuccessColor { get; set; } = "#39FF14"; // Neon green
        public string DangerColor { get; set; } = "#FF0000"; // Bright red for contrast
        public string BackgroundColor { get; set; } = "#0A0A0A"; // Near black (slight gray to reduce eye strain)

        // Text
        public string PrimaryTextColor { get; set; } = "#00FF00"; // Fluorescent green
        public string SecondaryTextColor { get; set; } = "#00CC00"; // Slightly darker green

        // Fonts
        public int HeaderFontSize { get; set; } = 24;
        public int NormalFontSize { get; set; } = 14;
        public string FontFamily { get; set; } = "Consolas"; // Monospace font for terminal aesthetic
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
