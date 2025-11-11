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
    }
}
