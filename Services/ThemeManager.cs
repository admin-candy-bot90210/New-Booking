using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    public static class ThemeManager
    {
        public static Dictionary<string, ThemeColors> AvailableThemes = new Dictionary<string, ThemeColors>
        {
            {
                "Default", new ThemeColors
                {
                    Background = "#ECF0F1",
                    Text = "#000000",
                    Accent = "#3498DB",
                    Header = "#2C3E50",
                    Success = "#27AE60",
                    Danger = "#E74C3C"
                }
            },
            {
                "DarkGreen", new ThemeColors
                {
                    Background = "#000000",
                    Text = "#00FF00",
                    Accent = "#00FF00",
                    Header = "#001100",
                    Success = "#00FF00",
                    Danger = "#FF0000"
                }
            }
        };

        public static void ApplyTheme(Window window, UserAppPreferences prefs)
        {
            ThemeColors colors;

            if (prefs.ThemeName == "Custom")
            {
                colors = new ThemeColors
                {
                    Background = prefs.CustomBackgroundColor,
                    Text = prefs.CustomTextColor,
                    Accent = prefs.CustomAccentColor,
                    Header = DarkenColor(prefs.CustomBackgroundColor, 0.2),
                    Success = prefs.CustomAccentColor,
                    Danger = "#FF0000"
                };
            }
            else if (AvailableThemes.ContainsKey(prefs.ThemeName))
            {
                colors = AvailableThemes[prefs.ThemeName];
            }
            else
            {
                colors = AvailableThemes["Default"];
            }

            ApplyThemeColors(window, colors);

            // Apply StayOnTop preference
            window.Topmost = prefs.StayOnTop;
        }

        private static void ApplyThemeColors(Window window, ThemeColors colors)
        {
            // This is a simplified version - in a real app you'd use ResourceDictionaries
            // For now, we set the window background
            try
            {
                window.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(colors.Background);
            }
            catch { }
        }

        private static string DarkenColor(string hexColor, double factor)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                byte r = (byte)(color.R * (1 - factor));
                byte g = (byte)(color.G * (1 - factor));
                byte b = (byte)(color.B * (1 - factor));
                return $"#{r:X2}{g:X2}{b:X2}";
            }
            catch
            {
                return hexColor;
            }
        }
    }

    public class ThemeColors
    {
        public string Background { get; set; } = "#FFFFFF";
        public string Text { get; set; } = "#000000";
        public string Accent { get; set; } = "#3498DB";
        public string Header { get; set; } = "#2C3E50";
        public string Success { get; set; } = "#27AE60";
        public string Danger { get; set; } = "#E74C3C";
    }
}
