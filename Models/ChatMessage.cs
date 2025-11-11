using System;
using System.Collections.Generic;

namespace DJBookingSystem.Models
{
    public class ChatMessage
    {
        public string? Id { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsErrorMessage { get; set; } = false;
        public string? ErrorCode { get; set; }
        public MessageType Type { get; set; } = MessageType.Normal;

        // Channel and targeting
        public ChatChannel Channel { get; set; } = ChatChannel.World;
        public string? RecipientUsername { get; set; } // For private 1-on-1 messages
        public List<string> GroupMembers { get; set; } = new List<string>(); // For group chats
        public string? GroupName { get; set; } // For group chat identification

        // Message status
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }

    public enum MessageType
    {
        Normal,
        Error,
        System,
        Announcement
    }

    public enum ChatChannel
    {
        World,          // Public to all users
        ToAdmin,        // DJs/Venues seeking admin help (visible to sender + all admins)
        AdminOnly,      // Admin-only channel (SysAdmin + Managers)
        Private,        // 1-on-1 private message
        Group           // Group chat
    }

    public class UserChatSettings
    {
        public string? Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<string> BlockedUsers { get; set; } = new List<string>(); // Users this user has blocked
        public List<string> MutedUsers { get; set; } = new List<string>();   // Users this user has muted
        public bool EnableNotifications { get; set; } = true;
        public bool EnableSoundNotifications { get; set; } = true;
        public bool MinimizeToTray { get; set; } = true;
    }

    public class ChatNotification
    {
        public string MessageId { get; set; } = string.Empty;
        public string SenderUsername { get; set; } = string.Empty;
        public string MessagePreview { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public ChatChannel Channel { get; set; } = ChatChannel.World;
        public bool IsRead { get; set; } = false;
    }
}
