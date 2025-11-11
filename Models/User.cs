using System;

namespace DJBookingSystem.Models
{
    public class User
    {
        public string? Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public UserPermissions Permissions { get; set; } = new UserPermissions();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastLogin { get; set; }

        // Account Types (can be multiple)
        public bool IsDJ { get; set; } = false;
        public bool IsVenueOwner { get; set; } = false;

        // DJ-specific information (stored on profile for auto-fill during booking)
        public string StreamingLink { get; set; } = string.Empty;
        public string DJLogoUrl { get; set; } = string.Empty;

        // User App Preferences
        public UserAppPreferences AppPreferences { get; set; } = new UserAppPreferences();

        // Moderation fields
        public bool IsBanned { get; set; } = false;
        public string? BannedBy { get; set; } // Username of moderator who banned
        public DateTime? BannedAt { get; set; }
        public string? BanReason { get; set; }
        public DateTime? BanExpiry { get; set; } // Null = permanent ban

        public bool IsGloballyMuted { get; set; } = false; // Moderator mute (can't send messages)
        public string? MutedBy { get; set; } // Username of moderator who muted
        public DateTime? MutedAt { get; set; }
        public DateTime? MuteExpiry { get; set; } // Null = permanent mute

        // IP tracking for ban enforcement
        public string? RegisteredIP { get; set; } // IP used during registration
        public string? CurrentIP { get; set; } // Last login IP
        public List<string> IPHistory { get; set; } = new List<string>(); // Track all IPs used
        public string? BannedIP { get; set; } // IP that was banned (if applicable)
    }

    public class UserAppPreferences
    {
        // Theme preferences
        public string ThemeName { get; set; } = "Default"; // "Default", "Night", "DarkGreen", "Sunset", "Ocean", "Custom"

        // Comprehensive custom theme colors
        public string CustomBackgroundColor { get; set; } = "#ECF0F1";
        public string CustomTextColor { get; set; } = "#2C3E50";
        public string CustomHeaderColor { get; set; } = "#2C3E50";
        public string CustomMenuColor { get; set; } = "#34495E";
        public string CustomButtonColor { get; set; } = "#3498DB";
        public string CustomButtonTextColor { get; set; } = "#FFFFFF";
        public string CustomBorderColor { get; set; } = "#BDC3C7";
        public string CustomAccentColor { get; set; } = "#3498DB";
        public string CustomSuccessColor { get; set; } = "#27AE60";
        public string CustomErrorColor { get; set; } = "#E74C3C";

        // Login preferences
        public bool RememberMe { get; set} = false;
        public bool AutoLogin { get; set; } = false;

        // Window preferences
        public bool StayOnTop { get; set; } = false;
    }

    public enum UserRole
    {
        SysAdmin,
        Manager,
        User
    }

    public class UserPermissions
    {
        // Booking permissions
        public bool CanViewBookings { get; set; } = true;
        public bool CanCreateBookings { get; set; } = true;
        public bool CanEditBookings { get; set; } = true;
        public bool CanDeleteBookings { get; set; } = false;

        // Venue permissions
        public bool CanViewVenues { get; set; } = true;
        public bool CanRegisterVenues { get; set; } = true;
        public bool CanEditVenues { get; set; } = false;
        public bool CanDeleteVenues { get; set; } = false;
        public bool CanToggleVenueStatus { get; set; } = false;

        // Admin permissions
        public bool CanManageUsers { get; set; } = false;
        public bool CanCustomizeApp { get; set; } = false;
        public bool CanAccessSettings { get; set; } = true;

        // Moderation permissions
        public bool CanBanUsers { get; set; } = false;
        public bool CanMuteUsers { get; set; } = false;
        public bool CanViewReports { get; set; } = false;
        public bool CanResolveReports { get; set; } = false;

        // RadioBOSS permissions
        public bool CanViewRadioBoss { get; set; } = false;
        public bool CanControlRadioBoss { get; set; } = false;
    }
}
