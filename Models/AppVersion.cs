using System;

namespace DJBookingSystem.Models
{
    public class AppVersion
    {
        public string Version { get; set; } = "2.0.0";
        public string ReleaseNotes { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; } = DateTime.Now;
        public string DownloadUrl { get; set; } = string.Empty;
        public bool IsMandatory { get; set; } = false;
        public string MinimumRequiredVersion { get; set; } = "1.0.0";
    }
}
