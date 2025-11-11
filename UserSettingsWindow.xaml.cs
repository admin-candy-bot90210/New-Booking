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
            var prefs = _user.AppPreferences ?? new UserAppPreferences();

            // Load theme
            switch (prefs.ThemeName)
            {
                case "Default":
                    ThemeComboBox.SelectedIndex = 0;
                    break;
                case "DarkGreen":
                    ThemeComboBox.SelectedIndex = 1;
                    break;
                case "Custom":
                    ThemeComboBox.SelectedIndex = 2;
                    CustomBackgroundTextBox.Text = prefs.CustomBackgroundColor;
                    CustomTextTextBox.Text = prefs.CustomTextColor;
                    CustomAccentTextBox.Text = prefs.CustomAccentColor;
                    break;
                default:
                    ThemeComboBox.SelectedIndex = 0;
                    break;
            }

            // Load login settings
            RememberMeCheckBox.IsChecked = prefs.RememberMe;
            AutoLoginCheckBox.IsChecked = prefs.AutoLogin;

            // Load window settings
            StayOnTopCheckBox.IsChecked = prefs.StayOnTop;
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is ComboBoxItem selected)
            {
                string theme = selected.Tag.ToString() ?? "Default";
                CustomThemePanel.Visibility = theme == "Custom" ? Visibility.Visible : Visibility.Collapsed;

                // Show preview for DarkGreen
                if (theme == "DarkGreen")
                {
                    ShowPreview("#000000", "#00FF00", "#00FF00");
                }
            }
        }

        private void PreviewTheme_Click(object sender, RoutedEventArgs e)
        {
            ShowPreview(
                CustomBackgroundTextBox.Text,
                CustomTextTextBox.Text,
                CustomAccentTextBox.Text
            );
        }

        private void ShowPreview(string bgColor, string textColor, string accentColor)
        {
            try
            {
                PreviewPanel.Visibility = Visibility.Visible;
                PreviewBorder.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(bgColor);
                PreviewBorder.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(accentColor);
                PreviewText.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom(textColor);

                var button = (Button)PreviewBorder.Child;
                if (button != null)
                {
                    var stackPanel = button.Parent as StackPanel;
                    if (stackPanel != null)
                    {
                        var sampleButton = stackPanel.Children[1] as Button;
                        if (sampleButton != null)
                        {
                            sampleButton.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(accentColor);
                            sampleButton.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom(bgColor);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid color format: {ex.Message}", "Preview Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                // Update user preferences
                _user.AppPreferences = new UserAppPreferences
                {
                    ThemeName = themeName,
                    CustomBackgroundColor = CustomBackgroundTextBox.Text,
                    CustomTextColor = CustomTextTextBox.Text,
                    CustomAccentColor = CustomAccentTextBox.Text,
                    RememberMe = RememberMeCheckBox.IsChecked ?? false,
                    AutoLogin = AutoLoginCheckBox.IsChecked ?? false,
                    StayOnTop = StayOnTopCheckBox.IsChecked ?? false
                };

                // Save to Firebase
                await _firebaseService.UpdateUserAsync(_user.Id ?? "", _user);

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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
