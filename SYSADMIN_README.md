# DJ Booking System - SysAdmin Features

## Complete Authentication & User Management System

Your DJ Booking System now includes a comprehensive SysAdmin system with full user management, permission controls, and extensible customization capabilities.

---

## 🚀 Getting Started

### First Launch

1. **Firebase Configuration**
   - Enter your Firebase Realtime Database URL
   - Format: `https://your-project.firebaseio.com/`

2. **Automatic Admin Creation**
   - System creates default admin if no users exist
   - **Default Credentials:**
     - Username: `admin`
     - Password: `admin123`

3. **Login**
   - Use the default credentials to access the system
   - **IMPORTANT:** Change the admin password immediately after first login!

---

## 👥 User Management

### User Roles

**SysAdmin**
- Full system access
- Manage all users
- Customize app settings
- Cannot be deleted if they're the last SysAdmin

**Manager**
- Create, edit, delete bookings
- Register and manage venues
- Cannot manage users or customize app

**User** (Default)
- View and create bookings
- Register venues
- Limited edit/delete permissions

### Managing Users

Access via **Admin Panel** tab (visible only to authorized users):

1. **Add New User**
   - Username (unique)
   - Full name
   - Email address
   - Password
   - Role assignment
   - Auto-assigned default permissions based on role

2. **Edit User**
   - Change user information
   - Update password (optional)
   - Change role
   - Keep existing permissions

3. **Edit Permissions**
   - Granular permission control per user
   - **Booking Permissions:**
     - View, Create, Edit, Delete bookings
   - **Venue Permissions:**
     - View, Register, Edit, Delete, Toggle open/closed
   - **Admin Permissions:**
     - Manage users, Customize app, Access settings

4. **Toggle Active/Inactive**
   - Deactivate user without deletion
   - Inactive users cannot login

5. **Delete User**
   - Permanently remove user
   - Cannot delete last SysAdmin

---

## 🔒 Permission System

### How Permissions Work

- **Tab Visibility:** Tabs hide if user lacks view permission
- **Button States:** Buttons disable if user lacks specific permission
- **Runtime Checks:** All operations verify permissions before execution

### Permission Matrix

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

---

## 🎨 App Customization (Framework Ready)

The app includes a customization framework ready for implementation:

### Theme Settings (Models Created)
- Header colors
- Accent colors
- Success/Danger colors
- Background colors
- Font sizes and families

### Feature Toggles (Models Created)
- Enable/disable booking edit/delete
- Show/hide venue details
- Booking approval workflow
- Multiple bookings same time slot
- Max advance booking days
- Date format preferences

### Implementation Status
- ✅ Models defined (`AppSettings.cs`)
- ✅ Firebase service methods created
- ✅ Settings load on startup
- ⏳ UI customization window (planned)
- ⏳ Live theme application (planned)

---

## 🔐 Security Features

### Password Security
- **SHA256 Hashing:** All passwords stored as secure hashes
- **No Plain Text:** Passwords never stored in plain text
- **Change Anytime:** Users can update passwords via Edit User

### Session Management
- Login required on app startup
- Session persists during app runtime
- Re-login required after app restart
- No automatic session timeout (desktop app)

### Permission Enforcement
- Server-side ready (Firebase rules can enforce)
- Client-side UI enforcement prevents errors
- Double-check before destructive operations

---

## 📊 Firebase Database Structure

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
      "permissions": {
        "canViewBookings": true,
        "canCreateBookings": true,
        ...
      },
      "isActive": true,
      "createdAt": "2025-11-11T10:00:00",
      "lastLogin": "2025-11-11T15:30:00"
    }
  },
  "settings": {
    "app_settings": {
      "appTitle": "DJ Booking Management System",
      "theme": { ... },
      "features": { ... },
      "updatedAt": "2025-11-11T10:00:00"
    }
  }
}
```

---

## 🔧 Common Administrative Tasks

### Change Default Admin Password
1. Login as `admin`
2. Go to **Admin Panel** tab
3. Click **Manage Users**
4. Select admin user
5. Click **Edit User**
6. Enter new password
7. Save

### Create Manager Account
1. **Admin Panel** → **Manage Users** → **Add User**
2. Fill in details
3. Select **Manager** role
4. Save (auto-assigns Manager permissions)

### Restrict User Permissions
1. **Admin Panel** → **Manage Users**
2. Select user
3. Click **Edit Permissions**
4. Uncheck specific permissions
5. Save

### Deactivate User Temporarily
1. **Admin Panel** → **Manage Users**
2. Select user
3. Click **Toggle Active/Inactive**
4. User cannot login while inactive

### Add New SysAdmin
1. Create user with Manager/User role
2. Edit user → Change role to **SysAdmin**
3. Edit permissions → Grant all permissions

---

## 📝 Application Flow

```
1. App Startup
   ↓
2. Firebase URL Entry
   ↓
3. Initialize Default Admin (if needed)
   ↓
4. Login Window
   ↓
5. Authentication Check
   ↓
6. Load App Settings
   ↓
7. Main Window (with permissions applied)
   ↓
8. Admin Panel (if authorized)
```

---

## 🎯 Key Features Implemented

✅ **Authentication System**
- Login window with validation
- Default admin auto-creation
- Password hashing (SHA256)
- Last login tracking

✅ **User Management**
- CRUD operations for users
- Three role types with defaults
- User active/inactive toggle
- Prevention of last SysAdmin deletion

✅ **Permission System**
- 12 granular permissions
- Role-based defaults
- Per-user customization
- UI enforcement (hide/disable)

✅ **Admin Panel**
- User management interface
- Current user information display
- Customization framework ready

✅ **Firebase Integration**
- Separate collections for users/settings
- Async operations throughout
- Error handling

✅ **Security**
- Password hashing
- Session management
- Permission checks

---

## 🚨 Important Notes

1. **Default Admin:** Always change the default password!
2. **Last SysAdmin:** Cannot delete the last SysAdmin account
3. **Firebase Rules:** Configure Firebase security rules for production
4. **Backup:** Regular database backups recommended
5. **Password Recovery:** No built-in recovery - admin must reset
6. **Session:** No auto-logout - close app to end session

---

## 🔄 Future Enhancements (Framework Ready)

- Live theme customization UI
- Feature toggle interface
- Password reset email system
- Audit log (track all user actions)
- Session timeout configuration
- Two-factor authentication
- LDAP/SSO integration
- Export user list to CSV

---

## 💡 Tips

- Create a backup SysAdmin account before making changes
- Use Manager role for venue/booking administrators
- Use User role for DJs and venue owners
- Regularly review user permissions
- Keep the default admin for emergency access
- Test permission changes with test accounts first

---

## 📞 Support & Documentation

For full implementation details, see:
- `SYSADMIN_IMPLEMENTATION_GUIDE.md` - Technical implementation details
- `README.md` - General application documentation

All DJ services remain FREE - the authentication system is purely for management and organization!
