using System;

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
    }

    public enum MessageType
    {
        Normal,
        Error,
        System,
        Announcement
    }
}
