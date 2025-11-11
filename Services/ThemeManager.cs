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
                    Name = "Default (Day Mode)",
                    Background = "#ECF0F1",
                    Text = "#2C3E50",
                    Header = "#2C3E50",
                    Menu = "#34495E",
                    Button = "#3498DB",
                    ButtonText = "#FFFFFF",
                    Border = "#BDC3C7",
                    Accent = "#3498DB",
                    Success = "#27AE60",
                    Error = "#E74C3C"
                }
            },
            {
                "Night", new ThemeColors
                {
                    Name = "Night Mode (Dark)",
                    Background = "#1E1E1E",
                    Text = "#E0E0E0",
                    Header = "#252525",
                    Menu = "#2D2D30",
                    Button = "#007ACC",
                    ButtonText = "#FFFFFF",
                    Border = "#3F3F46",
                    Accent = "#007ACC",
                    Success = "#4EC9B0",
                    Error = "#F48771"
                }
            },
            {
                "DarkGreen", new ThemeColors
                {
                    Name = "Dark Green (Matrix)",
                    Background = "#000000",
                    Text = "#00FF00",
                    Header = "#001100",
                    Menu = "#002200",
                    Button = "#00AA00",
                    ButtonText = "#00FF00",
                    Border = "#00FF00",
                    Accent = "#00FF00",
                    Success = "#00FF00",
                    Error = "#FF0000"
                }
            },
            {
                "Sunset", new ThemeColors
                {
                    Name = "Sunset (Warm)",
                    Background = "#FFF5E1",
                    Text = "#5D4037",
                    Header = "#FF6F00",
                    Menu = "#FF8F00",
                    Button = "#FF6F00",
                    ButtonText = "#FFFFFF",
                    Border = "#FFB74D",
                    Accent = "#FF6F00",
                    Success = "#8BC34A",
                    Error = "#F44336"
                }
            },
            {
                "Ocean", new ThemeColors
                {
                    Name = "Ocean (Cool Blue)",
                    Background = "#E3F2FD",
                    Text = "#01579B",
                    Header = "#0277BD",
                    Menu = "#0288D1",
                    Button = "#0277BD",
                    ButtonText = "#FFFFFF",
                    Border = "#81D4FA",
                    Accent = "#0277BD",
                    Success = "#4CAF50",
                    Error = "#F44336"
                }
            },
            {
                "Purple", new ThemeColors
                {
                    Name = "Purple Haze",
                    Background = "#F3E5F5",
                    Text = "#4A148C",
                    Header = "#6A1B9A",
                    Menu = "#7B1FA2",
                    Button = "#8E24AA",
                    ButtonText = "#FFFFFF",
                    Border = "#BA68C8",
                    Accent = "#8E24AA",
                    Success = "#66BB6A",
                    Error = "#EF5350"
                }
            },
            {
                "MidnightBlue", new ThemeColors
                {
                    Name = "Midnight Blue",
                    Background = "#0D1B2A",
                    Text = "#E0E1DD",
                    Header = "#1B263B",
                    Menu = "#415A77",
                    Button = "#778DA9",
                    ButtonText = "#FFFFFF",
                    Border = "#778DA9",
                    Accent = "#778DA9",
                    Success = "#52B788",
                    Error = "#D62828"
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
                    Name = "Custom",
                    Background = prefs.CustomBackgroundColor,
                    Text = prefs.CustomTextColor,
                    Header = prefs.CustomHeaderColor,
                    Menu = prefs.CustomMenuColor,
                    Button = prefs.CustomButtonColor,
                    ButtonText = prefs.CustomButtonTextColor,
                    Border = prefs.CustomBorderColor,
                    Accent = prefs.CustomAccentColor,
                    Success = prefs.CustomSuccessColor,
                    Error = prefs.CustomErrorColor
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
            try
            {
                // Apply colors to window resources so they cascade to all controls
                window.Resources["ThemeBackgroundBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Background));
                window.Resources["ThemeTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Text));
                window.Resources["ThemeHeaderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Header));
                window.Resources["ThemeMenuBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Menu));
                window.Resources["ThemeButtonBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Button));
                window.Resources["ThemeButtonTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.ButtonText));
                window.Resources["ThemeBorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Border));
                window.Resources["ThemeAccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Accent));
                window.Resources["ThemeSuccessBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Success));
                window.Resources["ThemeErrorBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Error));

                // Apply window background
                window.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Background));
            }
            catch { }
        }

        public static ThemeColors GetThemeColors(UserAppPreferences prefs)
        {
            if (prefs.ThemeName == "Custom")
            {
                return new ThemeColors
                {
                    Name = "Custom",
                    Background = prefs.CustomBackgroundColor,
                    Text = prefs.CustomTextColor,
                    Header = prefs.CustomHeaderColor,
                    Menu = prefs.CustomMenuColor,
                    Button = prefs.CustomButtonColor,
                    ButtonText = prefs.CustomButtonTextColor,
                    Border = prefs.CustomBorderColor,
                    Accent = prefs.CustomAccentColor,
                    Success = prefs.CustomSuccessColor,
                    Error = prefs.CustomErrorColor
                };
            }
            else if (AvailableThemes.ContainsKey(prefs.ThemeName))
            {
                return AvailableThemes[prefs.ThemeName];
            }
            else
            {
                return AvailableThemes["Default"];
            }
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
        public string Name { get; set; } = "Default";
        public string Background { get; set; } = "#ECF0F1";
        public string Text { get; set; } = "#2C3E50";
        public string Header { get; set; } = "#2C3E50";
        public string Menu { get; set; } = "#34495E";
        public string Button { get; set; } = "#3498DB";
        public string ButtonText { get; set; } = "#FFFFFF";
        public string Border { get; set; } = "#BDC3C7";
        public string Accent { get; set; } = "#3498DB";
        public string Success { get; set; } = "#27AE60";
        public string Error { get; set; } = "#E74C3C";
    }
}
