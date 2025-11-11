# SysAdmin System Implementation Guide

## Overview
This document outlines the implementation plan for adding a comprehensive SysAdmin system with user management, permissions, and app customization to the DJ Booking System.

## Features Implemented So Far

### ✅ Models Created
1. **User Model** (`Models/User.cs`)
   - Username, PasswordHash, FullName, Email
   - UserRole enum (SysAdmin, Manager, User)
   - UserPermissions class with granular permissions
   - IsActive flag, CreatedAt, LastLogin timestamps

2. **AppSettings Model** (`Models/AppSettings.cs`)
   - ThemeSettings (colors, fonts)
   - FeatureSettings (toggle features, time settings, display options)

3. **Login Window** (`LoginWindow.xaml` & `.xaml.cs`)
   - Username/password authentication
   - SHA256 password hashing
   - Default SysAdmin credentials displayed
   - Error handling

4. **User Management Window** (`UserManagementWindow.xaml`)
   - DataGrid showing all users
   - Add, Edit, Delete user buttons
   - Edit permissions button
   - Toggle active/inactive status

## Required Implementation Steps

### 1. Firebase Service Extensions

Add to `Services/FirebaseService.cs`:

```csharp
private const string UsersNode = "users";
private const string SettingsNode = "settings";

// User Management
public async Task<User?> GetUserByUsernameAsync(string username)
public async Task<List<User>> GetAllUsersAsync()
public async Task<string> AddUserAsync(User user)
public async Task UpdateUserAsync(string id, User user)
public async Task DeleteUserAsync(string id)
public async Task InitializeDefaultAdminAsync() // Creates default admin if none exists

// Settings Management
public async Task<AppSettings> GetAppSettingsAsync()
public async Task UpdateAppSettingsAsync(AppSettings settings)
```

### 2. User Management Windows

#### A. UserManagementWindow.xaml.cs
- Load users from Firebase
- Add User dialog
- Edit User dialog
- Edit Permissions dialog
- Toggle active/inactive
- Delete with confirmation

#### B. EditUserWindow.xaml
- Text fields for Username, FullName, Email
- Password field (optional for edit, required for new)
- Role dropdown
- Save/Cancel buttons

#### C. EditPermissionsWindow.xaml
- CheckBoxes for each permission category:
  - Booking permissions (View, Create, Edit, Delete)
  - Venue permissions (View, Register, Edit, Delete, Toggle Status)
  - Admin permissions (Manage Users, Customize App, Access Settings)
- Save/Cancel buttons

### 3. App Customization Window

#### AppCustomizationWindow.xaml
**Theme Tab:**
- Color pickers for all theme colors
- Font size sliders
- Font family dropdown
- Preview panel showing changes
- Reset to defaults button

**Features Tab:**
- CheckBoxes for feature toggles
- Numeric inputs for time settings
- Date format dropdown
- Apply/Save buttons

### 4. Update App.xaml.cs

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    // Show Firebase connection dialog first
    var firebaseUrlWindow = new FirebaseUrlWindow();
    if (firebaseUrlWindow.ShowDialog() == true)
    {
        var firebaseService = new FirebaseService(firebaseUrlWindow.FirebaseUrl);

        // Initialize default admin if needed
        await firebaseService.InitializeDefaultAdminAsync();

        // Show login window
        var loginWindow = new LoginWindow(firebaseService);
        if (loginWindow.ShowDialog() == true)
        {
            // Load app settings
            var appSettings = await firebaseService.GetAppSettingsAsync();

            // Show main window with user and settings
            var mainWindow = new MainWindow(firebaseService, loginWindow.LoggedInUser, appSettings);
            mainWindow.Show();
        }
        else
        {
            Shutdown();
        }
    }
    else
    {
        Shutdown();
    }
}
```

### 5. Update MainWindow.xaml & .xaml.cs

**Constructor changes:**
```csharp
private User _currentUser;
private AppSettings _appSettings;

public MainWindow(FirebaseService firebaseService, User currentUser, AppSettings appSettings)
{
    InitializeComponent();
    _firebaseService = firebaseService;
    _currentUser = currentUser;
    _appSettings = appSettings;

    ApplyThemeSettings();
    ApplyPermissions();
    InitializeTimeControls();
}
```

**Permission Methods:**
```csharp
private void ApplyPermissions()
{
    // Hide/disable UI elements based on permissions
    if (!_currentUser.Permissions.CanCreateBookings)
        NewBookingTab.Visibility = Visibility.Collapsed;

    if (!_currentUser.Permissions.CanDeleteBookings)
        DeleteBookingButton.IsEnabled = false;

    if (!_currentUser.Permissions.CanRegisterVenues)
        RegisterVenueButton.Visibility = Visibility.Collapsed;

    // Show admin panel only for SysAdmin
    if (_currentUser.Role == UserRole.SysAdmin)
    {
        AdminTab.Visibility = Visibility.Visible;
    }
}
```

**Theme Methods:**
```csharp
private void ApplyThemeSettings()
{
    // Apply colors
    HeaderBorder.Background = new SolidColorBrush(
        (Color)ColorConverter.ConvertFromString(_appSettings.Theme.HeaderBackgroundColor));

    // Apply fonts
    this.FontSize = _appSettings.Theme.NormalFontSize;
    this.FontFamily = new FontFamily(_appSettings.Theme.FontFamily);

    // Update app title
    this.Title = _appSettings.AppTitle;
}
```

### 6. Add Admin Tab to MainWindow.xaml

```xml
<TabItem x:Name="AdminTab" Header="Admin Panel" Visibility="Collapsed">
    <Grid Margin="20">
        <StackPanel>
            <TextBlock Text="SysAdmin Panel"
                      FontSize="20"
                      FontWeight="Bold"
                      Margin="0,0,0,20"/>

            <Button Content="Manage Users"
                   Click="ManageUsers_Click"
                   Padding="15,10"
                   Margin="0,5"
                   HorizontalAlignment="Left"
                   Width="200"/>

            <Button Content="Customize App"
                   Click="CustomizeApp_Click"
                   Padding="15,10"
                   Margin="0,5"
                   HorizontalAlignment="Left"
                   Width="200"/>

            <TextBlock Text="Current User Info"
                      FontSize="16"
                      FontWeight="Bold"
                      Margin="0,30,0,10"/>

            <TextBlock x:Name="CurrentUserInfoTextBlock"/>
        </StackPanel>
    </Grid>
</TabItem>
```

### 7. Default Admin Account

**Username:** `admin`
**Password:** `admin123`
**Password Hash:** `240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9`

The system should create this account automatically on first run if no users exist.

### 8. Firebase Database Structure

```json
{
  "bookings": { ... },
  "venues": { ... },
  "users": {
    "user_id_1": {
      "username": "admin",
      "passwordHash": "...",
      "fullName": "System Administrator",
      "email": "admin@djbooking.com",
      "role": "SysAdmin",
      "permissions": { ... },
      "isActive": true,
      "createdAt": "...",
      "lastLogin": "..."
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

### 9. Security Considerations

1. **Password Storage:** Use SHA256 hashing (already implemented)
2. **Session Management:** Store current user in memory, require re-login on app restart
3. **Permission Checks:** Validate permissions before each action
4. **Audit Trail:** Consider logging user actions (optional enhancement)

### 10. Testing Checklist

- [ ] Default admin account creation
- [ ] Login with valid/invalid credentials
- [ ] Create new users with different roles
- [ ] Edit user information
- [ ] Edit user permissions
- [ ] Toggle user active/inactive status
- [ ] Delete users (except last SysAdmin)
- [ ] Verify permissions hide/show UI elements
- [ ] Verify permissions prevent unauthorized actions
- [ ] Customize theme colors and see changes
- [ ] Toggle features on/off
- [ ] Settings persist across app restarts

## Quick Start Implementation Order

1. **Phase 1: Authentication (1-2 hours)**
   - Update FirebaseService with user methods
   - Update App.xaml.cs startup flow
   - Test login with default admin

2. **Phase 2: User Management (2-3 hours)**
   - Complete UserManagementWindow.xaml.cs
   - Create EditUserWindow
   - Create EditPermissionsWindow
   - Test CRUD operations

3. **Phase 3: Permissions (1-2 hours)**
   - Update MainWindow to accept User parameter
   - Implement ApplyPermissions() method
   - Test with different user roles

4. **Phase 4: Customization (2-3 hours)**
   - Create AppCustomizationWindow
   - Implement theme application
   - Implement feature toggles
   - Test customization persistence

5. **Phase 5: Testing & Polish (1-2 hours)**
   - End-to-end testing
   - Bug fixes
   - Documentation updates

## Estimated Total Implementation Time: 7-12 hours

## Files Needed (Not Yet Created)

- `UserManagementWindow.xaml.cs`
- `EditUserWindow.xaml` + `.xaml.cs`
- `EditPermissionsWindow.xaml` + `.xaml.cs`
- `AppCustomizationWindow.xaml` + `.xaml.cs`
- `FirebaseUrlWindow.xaml` + `.xaml.cs` (for initial setup)
- Updates to existing `App.xaml.cs`
- Updates to existing `MainWindow.xaml` + `.xaml.cs`
- Updates to existing `FirebaseService.cs`

## Current Status

✅ Models defined
✅ Login window created
✅ User management UI skeleton created
⏳ Implementation guide documented
❌ Full integration pending

This system provides enterprise-level user management and customization while maintaining the free, open nature of the DJ booking service itself.
