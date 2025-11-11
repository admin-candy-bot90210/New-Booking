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

        // Sender identity flags for color coding
        public bool SenderIsDJ { get; set; } = false;
        public bool SenderIsVenueOwner { get; set; } = false;

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

    public class UserReport
    {
        public string? Id { get; set; }
        public string ReportedUsername { get; set; } = string.Empty;
        public string ReporterUsername { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; } = DateTime.Now;
        public bool IsResolved { get; set; } = false;
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? AdminNotes { get; set; }
    }

    public enum ReportReason
    {
        Harassment,
        Spam,
        InappropriateContent,
        Impersonation,
        Other
    }

    public class ModerationAction
    {
        public string? Id { get; set; }
        public string TargetUsername { get; set; } = string.Empty;
        public string ModeratorUsername { get; set; } = string.Empty;
        public ModerationActionType ActionType { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; } // For temporary bans/mutes
        public bool IsActive { get; set; } = true;
        public DateTime? RevokedAt { get; set; }
        public string? RevokedBy { get; set; }
    }

    public enum ModerationActionType
    {
        Ban,
        Unban,
        Mute,
        Unmute,
        Warning,
        ReportResolved
    }

    public class SupportMessage
    {
        public string? Id { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public bool IsResolved { get; set; } = false;
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? AdminResponse { get; set; }
    }
}
