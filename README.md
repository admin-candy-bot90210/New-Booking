# DJ Booking Management System

A comprehensive Windows desktop application for managing DJ bookings and venue registrations with a complete user authentication and permission system.

## Features

### Core Functionality
- **DJ Booking System** - DJs can book time slots at registered venues
- **Venue Management** - Venue owners can register their spaces and manage availability
- **User Authentication** - Secure login system with SHA256 password hashing
- **Permission System** - Granular role-based access control (SysAdmin, Manager, User)
- **Firebase Integration** - Cloud-based data storage using Firebase Realtime Database

### Key Features
- Each DJ slot is 1 hour long
- All DJ services are FREE (no payment processing)
- Venues can be toggled open/closed
- Only open venues appear in booking dropdown
- Complete user management (create, edit, delete accounts)
- 12 granular permissions across Bookings, Venues, and Admin functions

## Technology Stack

- **.NET 8.0** - Framework
- **WPF (Windows Presentation Foundation)** - UI Framework
- **Firebase Realtime Database** - Cloud database
- **C#** - Programming language
- **SHA256** - Password hashing algorithm

## Getting Started

### Prerequisites

- Windows 10/11
- .NET 8.0 Runtime or SDK
- Firebase Realtime Database account

### Installation

1. **Download** the latest release
2. **Extract** the files to your desired location
3. **Run** `DJBookingSystem.exe`

### First Launch

1. **Enter Firebase URL** - Provide your Firebase Realtime Database URL (format: `https://your-project.firebaseio.com/`)
2. **Login** - Use the default credentials:
   - Username: `admin`
   - Password: `admin123`
3. **Change Password** - IMPORTANT: Change the default admin password immediately!

## User Roles

### SysAdmin
- Full system access
- Manage all users
- Customize app settings
- Cannot be deleted if they're the last SysAdmin

### Manager
- Create, edit, delete bookings
- Register and manage venues
- Cannot manage users or customize app

### User (Default)
- View and create bookings
- Register venues
- Limited edit/delete permissions

## Permissions Matrix

| Permission | SysAdmin | Manager | User |
|-----------|----------|---------|------|
| View Bookings | ✅ | ✅ | ✅ |
| Create Bookings | ✅ | ✅ | ✅ |
| Edit Bookings | ✅ | ✅ | ✅ |
| Delete Bookings | ✅ | ✅ | ❌ |
| View Venues | ✅ | ✅ | ✅ |
| Register Venues | ✅ | ✅ | ✅ |
| Edit Venues | ✅ | ✅ | ❌ |
| Delete Venues | ✅ | ❌ | ❌ |
| Toggle Venue Status | ✅ | ✅ | ❌ |
| Manage Users | ✅ | ❌ | ❌ |
| Customize App | ✅ | ❌ | ❌ |

## Building from Source

### Requirements

- .NET 8.0 SDK
- Windows (or Linux with `<EnableWindowsTargeting>true</EnableWindowsTargeting>`)

### Build Steps

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run

# Publish release build
dotnet publish -c Release -r win-x64 --self-contained
```

## Firebase Database Structure

```json
{
  "bookings": {
    "booking_id": {
      "djName": "DJ Name",
      "streamingLink": "https://...",
      "venue": "Room Name",
      "bookingDate": "2025-11-11T19:00:00",
      "createdAt": "2025-11-11T10:00:00"
    }
  },
  "venues": {
    "venue_id": {
      "roomName": "The Blue Room",
      "roomDescription": "...",
      "openingHours": "Fri-Sat: 8PM-2AM",
      "isOpen": true,
      "createdAt": "2025-11-11T10:00:00"
    }
  },
  "users": {
    "user_id": {
      "username": "admin",
      "passwordHash": "240be5...",
      "fullName": "System Administrator",
      "email": "admin@djbooking.com",
      "role": "SysAdmin",
      "permissions": { ... },
      "isActive": true,
      "createdAt": "2025-11-11T10:00:00",
      "lastLogin": "2025-11-11T15:30:00"
    }
  },
  "settings": {
    "app_settings": {
      "appTitle": "DJ Booking Management System",
      "theme": { ... },
      "features": { ... }
    }
  }
}
```

## Documentation

- **[SYSADMIN_README.md](SYSADMIN_README.md)** - Comprehensive user guide for administrators
- **[SYSADMIN_IMPLEMENTATION_GUIDE.md](SYSADMIN_IMPLEMENTATION_GUIDE.md)** - Technical implementation details

## Security

- Passwords stored as SHA256 hashes (never plain text)
- Session management with login tracking
- Permission-based UI enforcement
- Default admin auto-creation on first launch

## Important Notes

- **Default Credentials**: Username: `admin`, Password: `admin123` - CHANGE IMMEDIATELY!
- **Last SysAdmin Protection**: Cannot delete the last SysAdmin account
- **No Auto-Logout**: Close app to end session (desktop app behavior)
- **Firebase Rules**: Configure Firebase security rules for production use

## Future Enhancements

- Live theme customization UI
- Feature toggle interface
- Password reset email system
- Audit logging
- Two-factor authentication
- LDAP/SSO integration

## License

This project is provided as-is for DJ booking management purposes.

## Support

For detailed setup and usage instructions, see the documentation files included in this repository.
