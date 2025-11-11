# Enhanced Chat System - Implementation Plan

## Overview
Comprehensive multi-channel chat system with role-based UI, user management, notifications, and system tray integration for the 3DXChat DJ Booking System.

## Features Implemented

### 1. Multi-Channel Chat Architecture
- **World Chat**: Public channel visible to all users
- **To Admin**: Help channel where DJs/Venues can request admin assistance (visible to sender + all admins)
- **Admin Only**: Private admin channel (SysAdmins + Managers only)
- **Private (1-on-1)**: Direct messaging between two users
- **Group Chat**: Multi-user private conversations (future enhancement)

### 2. Role-Based Visual System
**Role Icons:**
- 👑 SysAdmin (Crown)
- ⭐ Manager (Star)
- 👤 User (Person)

**Role Colors:**
- SysAdmin: Red (#E74C3C)
- Manager: Blue (#3498DB)
- User: Green (#27AE60)

### 3. User Management Features
- **Mute User**: Hide messages from specific users (client-side filter)
- **Block User**: Completely block communication with specific users (stored in Firebase)
- **Manage Blocked Users**: View and unblock previously blocked users

### 4. Message Notification System
- Desktop notifications for new messages
- Unread message badges on channels
- Sound notifications (optional)
- Notification preview with message snippet

### 5. System Tray Integration
- Minimize chat to system tray
- Tray icon with notification badge
- Show notification balloons from tray
- Double-click tray icon to restore window
- Right-click context menu (Restore, Exit)

## New Models

### ChatMessage (Extended)
```csharp
- Channel: ChatChannel enum
- RecipientUsername: string (for private messages)
- GroupMembers: List<string> (for group chats)
- GroupName: string (for group identification)
- IsRead: bool
- ReadAt: DateTime?
```

### UserChatSettings
```csharp
- Username: string
- BlockedUsers: List<string>
- MutedUsers: List<string>
- EnableNotifications: bool
- EnableSoundNotifications: bool
- MinimizeToTray: bool
```

### ChatNotification
```csharp
- MessageId: string
- SenderUsername: string
- MessagePreview: string
- Timestamp: DateTime
- Channel: ChatChannel
- IsRead: bool
```

## UI Layout

### Three-Column Design:
1. **Left Panel (200px)**: Channel selector + Private conversations list
2. **Center Panel**: Active chat area with messages + input
3. **Right Panel (220px)**: Online users list with action buttons

### Key UI Elements:
- Channel buttons with unread badges
- Role-colored usernames with icons
- Role badge pills
- Online user list with status indicators
- Quick action buttons (PM, Mute, Block)

## Firebase Service Extensions

New methods needed:
- `GetChatMessagesByChannel(ChatChannel channel)`
- `GetPrivateMessages(string user1, string user2)`
- `GetUserChatSettings(string username)`
- `UpdateUserChatSettings(UserChatSettings settings)`
- `BlockUser(string username, string blockedUsername)`
- `UnblockUser(string username, string unblockedUsername)`
- `MuteUser(string username, string mutedUsername)` - Client-side only
- `MarkMessageAsRead(string messageId)`

## System Tray Implementation

Using `System.Windows.Forms.NotifyIcon`:
- Create NotifyIcon instance
- Set tray icon (custom or application icon)
- Handle StateChanged event
- Show balloon notifications
- Context menu with Restore/Exit

## File Structure

**Models:**
- `Models/ChatMessage.cs` ✅ (Extended)
- `Models/UserChatSettings.cs` ✅ (New class in ChatMessage.cs)

**UI:**
- `ChatWindow.xaml` ✅ (Completely redesigned)
- `ChatWindow.xaml.cs` (Needs complete rewrite)
- `ChatSettingsWindow.xaml` (New - for notification/tray settings)
- `ChatSettingsWindow.xaml.cs` (New)
- `PrivateChatWindow.xaml` (Optional - or use main window with channel switching)

**Services:**
- `Services/FirebaseService.cs` (Extend with chat methods)
- `Services/NotificationService.cs` (New - handle notifications)

## Implementation Priority

### Phase 1: Core Multi-Channel Chat ✅
- Channel switching UI ✅
- Filter messages by channel
- Display channel-specific messages
- Send messages to specific channels

### Phase 2: Role Icons & Colors ✅
- Role icons in message display ✅
- Role-colored usernames ✅
- Role badge pills ✅

### Phase 3: Private Messaging
- Private conversation list
- 1-on-1 message sending
- Private message filtering
- Unread private message badges

### Phase 4: User Management
- Mute functionality (client-side filter)
- Block functionality (Firebase storage)
- Manage blocked users dialog
- User action buttons

### Phase 5: Notifications
- Desktop notification service
- Sound notifications
- Unread message tracking
- Notification settings UI

### Phase 6: System Tray
- NotifyIcon implementation
- Minimize to tray
- Tray notification balloons
- Context menu
- Restore from tray

## Dependencies Required

Add to `.csproj`:
```xml
<ItemGroup>
  <Reference Include="System.Windows.Forms" />
  <Reference Include="System.Drawing" />
</ItemGroup>
```

## Testing Checklist

- [ ] World chat messaging works
- [ ] To Admin channel (users can send, admins can see)
- [ ] Admin Only channel (only admins can see)
- [ ] Private messages work 1-on-1
- [ ] Role icons display correctly
- [ ] Role colors display correctly
- [ ] Mute user hides messages
- [ ] Block user prevents communication
- [ ] Desktop notifications appear
- [ ] Sound notifications play
- [ ] System tray icon appears
- [ ] Minimize to tray works
- [ ] Restore from tray works
- [ ] Unread badges update correctly

## Notes

- Compatibility: Requires .NET 8.0 + WPF + Windows Forms for NotifyIcon
- Firebase: All messages stored with channel info for filtering
- Performance: Large message history may need pagination
- Security: Block/mute lists stored per-user in Firebase
- UX: Minimize to tray by default to keep chat accessible but not intrusive
