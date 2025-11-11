using System;
using System.IO;
using System.Text.Json;

namespace DJBookingSystem.Services
{
    public static class LocalStorage
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DJBookingSystem"
        );

        private static readonly string SettingsFile = Path.Combine(AppDataFolder, "settings.json");

        public static void SaveLoginInfo(string username, bool rememberMe, bool autoLogin)
        {
            try
            {
                if (!Directory.Exists(AppDataFolder))
                    Directory.CreateDirectory(AppDataFolder);

                var loginInfo = new LoginInfo
                {
                    Username = rememberMe ? username : "",
                    RememberMe = rememberMe,
                    AutoLogin = autoLogin,
                    LastSaved = DateTime.Now
                };

                string json = JsonSerializer.Serialize(loginInfo, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json);
            }
            catch
            {
                // Fail silently
            }
        }

        public static LoginInfo? GetLoginInfo()
        {
            try
            {
                if (!File.Exists(SettingsFile))
                    return null;

                string json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<LoginInfo>(json);
            }
            catch
            {
                return null;
            }
        }

        public static void ClearLoginInfo()
        {
            try
            {
                if (File.Exists(SettingsFile))
                    File.Delete(SettingsFile);
            }
            catch
            {
                // Fail silently
            }
        }
    }

    public class LoginInfo
    {
        public string Username { get; set; } = "";
        public bool RememberMe { get; set; } = false;
        public bool AutoLogin { get; set; } = false;
        public DateTime LastSaved { get; set; }
    }
}
