using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class UserSettingsWindow : Window
    {
        private User _user;
        private FirebaseService _firebaseService;
        public bool SettingsChanged { get; private set; } = false;
        private bool _isLoadingSettings = false;

        public UserSettingsWindow(User user, FirebaseService firebaseService)
        {
            InitializeComponent();
            _user = user;
            _firebaseService = firebaseService;

            UserInfoTextBlock.Text = $"Settings for: {user.FullName} ({user.Username})";
            LoadSettings();
        }

        private void LoadSettings()
        {
            _isLoadingSettings = true;

            var prefs = _user.AppPreferences ?? new UserAppPreferences();

            // Load theme selection
            switch (prefs.ThemeName)
            {
                case "Default":
                    ThemeComboBox.SelectedIndex = 0;
                    break;
                case "Night":
                    ThemeComboBox.SelectedIndex = 1;
                    break;
                case "DarkGreen":
                    ThemeComboBox.SelectedIndex = 2;
                    break;
                case "Sunset":
                    ThemeComboBox.SelectedIndex = 3;
                    break;
                case "Ocean":
                    ThemeComboBox.SelectedIndex = 4;
                    break;
                case "Purple":
                    ThemeComboBox.SelectedIndex = 5;
                    break;
                case "MidnightBlue":
                    ThemeComboBox.SelectedIndex = 6;
                    break;
                case "Custom":
                    ThemeComboBox.SelectedIndex = 7;
                    break;
                default:
                    ThemeComboBox.SelectedIndex = 0;
                    break;
            }

            // Load all custom colors
            CustomBackgroundTextBox.Text = prefs.CustomBackgroundColor;
            CustomTextTextBox.Text = prefs.CustomTextColor;
            CustomHeaderTextBox.Text = prefs.CustomHeaderColor;
            CustomMenuTextBox.Text = prefs.CustomMenuColor;
            CustomButtonTextBox.Text = prefs.CustomButtonColor;
            CustomButtonTextTextBox.Text = prefs.CustomButtonTextColor;
            CustomBorderTextBox.Text = prefs.CustomBorderColor;
            CustomAccentTextBox.Text = prefs.CustomAccentColor;
            CustomSuccessTextBox.Text = prefs.CustomSuccessColor;
            CustomErrorTextBox.Text = prefs.CustomErrorColor;

            // Load login settings
            RememberMeCheckBox.IsChecked = prefs.RememberMe;
            AutoLoginCheckBox.IsChecked = prefs.AutoLogin;

            // Load window settings
            StayOnTopCheckBox.IsChecked = prefs.StayOnTop;

            _isLoadingSettings = false;

            // Update preview
            UpdatePreview();
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingSettings) return;

            if (ThemeComboBox.SelectedItem is ComboBoxItem selected)
            {
                string theme = selected.Tag.ToString() ?? "Default";

                // Show/hide custom theme panel
                CustomThemePanel.Visibility = theme == "Custom" ? Visibility.Visible : Visibility.Collapsed;

                UpdatePreview();
            }
        }

        private void CustomColor_Changed(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingSettings) return;
            UpdatePreview();
        }

        private void CopyTheme_Click(object sender, RoutedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is ComboBoxItem selected)
            {
                string themeName = selected.Tag.ToString() ?? "Default";

                if (themeName == "Custom")
                {
                    MessageBox.Show("You're already using a custom theme!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Get colors from the selected theme
                if (ThemeManager.AvailableThemes.ContainsKey(themeName))
                {
                    var theme = ThemeManager.AvailableThemes[themeName];

                    // Copy to custom fields
                    CustomBackgroundTextBox.Text = theme.Background;
                    CustomTextTextBox.Text = theme.Text;
                    CustomHeaderTextBox.Text = theme.Header;
                    CustomMenuTextBox.Text = theme.Menu;
                    CustomButtonTextBox.Text = theme.Button;
                    CustomButtonTextTextBox.Text = theme.ButtonText;
                    CustomBorderTextBox.Text = theme.Border;
                    CustomAccentTextBox.Text = theme.Accent;
                    CustomSuccessTextBox.Text = theme.Success;
                    CustomErrorTextBox.Text = theme.Error;

                    // Switch to Custom theme
                    ThemeComboBox.SelectedIndex = 7; // Custom

                    MessageBox.Show($"'{theme.Name}' colors copied to Custom theme!\n\nYou can now customize individual colors.",
                        "Theme Copied", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void UpdatePreview()
        {
            try
            {
                ThemeColors colors;

                // Get current theme colors
                if (ThemeComboBox.SelectedItem is ComboBoxItem selected)
                {
                    string themeName = selected.Tag.ToString() ?? "Default";

                    if (themeName == "Custom")
                    {
                        // Use custom colors from textboxes
                        colors = new ThemeColors
                        {
                            Background = CustomBackgroundTextBox.Text,
                            Text = CustomTextTextBox.Text,
                            Header = CustomHeaderTextBox.Text,
                            Menu = CustomMenuTextBox.Text,
                            Button = CustomButtonTextBox.Text,
                            ButtonText = CustomButtonTextTextBox.Text,
                            Border = CustomBorderTextBox.Text,
                            Accent = CustomAccentTextBox.Text,
                            Success = CustomSuccessTextBox.Text,
                            Error = CustomErrorTextBox.Text
                        };
                    }
                    else if (ThemeManager.AvailableThemes.ContainsKey(themeName))
                    {
                        colors = ThemeManager.AvailableThemes[themeName];
                    }
                    else
                    {
                        colors = ThemeManager.AvailableThemes["Default"];
                    }
                }
                else
                {
                    colors = ThemeManager.AvailableThemes["Default"];
                }

                // Apply to preview
                PreviewBorder.Background = ParseColor(colors.Background);
                PreviewBackground.Foreground = ParseColor(colors.Text);
                PreviewText.Foreground = ParseColor(colors.Text);
                PreviewHeader.Background = ParseColor(colors.Header);
                PreviewButton.Background = ParseColor(colors.Button);
                PreviewButton.Foreground = ParseColor(colors.ButtonText);
                PreviewBorderElement.BorderBrush = ParseColor(colors.Border);
                PreviewAccent.Foreground = ParseColor(colors.Accent);
                PreviewSuccess.Foreground = ParseColor(colors.Success);
                PreviewError.Foreground = ParseColor(colors.Error);
            }
            catch
            {
                // Silently fail on invalid colors during typing
            }
        }

        private SolidColorBrush ParseColor(string hexColor)
        {
            try
            {
                return (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor);
            }
            catch
            {
                return Brushes.Black;
            }
        }

        private void StayOnTop_Changed(object sender, RoutedEventArgs e)
        {
            // Apply immediately to this window
            this.Topmost = StayOnTopCheckBox.IsChecked ?? false;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get selected theme
                string themeName = "Default";
                if (ThemeComboBox.SelectedItem is ComboBoxItem selected)
                {
                    themeName = selected.Tag.ToString() ?? "Default";
                }

                // Update user preferences with ALL fields
                _user.AppPreferences = new UserAppPreferences
                {
                    ThemeName = themeName,
                    CustomBackgroundColor = CustomBackgroundTextBox.Text.Trim(),
                    CustomTextColor = CustomTextTextBox.Text.Trim(),
                    CustomHeaderColor = CustomHeaderTextBox.Text.Trim(),
                    CustomMenuColor = CustomMenuTextBox.Text.Trim(),
                    CustomButtonColor = CustomButtonTextBox.Text.Trim(),
                    CustomButtonTextColor = CustomButtonTextTextBox.Text.Trim(),
                    CustomBorderColor = CustomBorderTextBox.Text.Trim(),
                    CustomAccentColor = CustomAccentTextBox.Text.Trim(),
                    CustomSuccessColor = CustomSuccessTextBox.Text.Trim(),
                    CustomErrorColor = CustomErrorTextBox.Text.Trim(),
                    RememberMe = RememberMeCheckBox.IsChecked ?? false,
                    AutoLogin = AutoLoginCheckBox.IsChecked ?? false,
                    StayOnTop = StayOnTopCheckBox.IsChecked ?? false
                };

                // Safety check for user ID
                if (!string.IsNullOrEmpty(_user.Id))
                {
                    await _firebaseService.UpdateUserAsync(_user.Id, _user);
                }

                MessageBox.Show("Settings saved successfully!\n\nRestart the application for theme changes to fully take effect.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                SettingsChanged = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Reset all settings to default values?\n\nThis will:\n- Set theme to Default (Day Mode)\n- Clear all custom colors\n- Reset login and window preferences",
                "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _user.AppPreferences = new UserAppPreferences(); // Creates default preferences
                LoadSettings();
                MessageBox.Show("Settings reset to defaults!\n\nClick 'Save & Apply Theme' to keep these changes.",
                    "Reset Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
