using System;
using System.Windows;
using DJBookingSystem.Services;
using Newtonsoft.Json.Linq;

namespace DJBookingSystem
{
    public partial class RadioBossControlWindow : Window
    {
        private readonly RadioBossService _radioBossService;
        private readonly bool _stayOnTop = false;

        public RadioBossControlWindow(bool stayOnTop = false)
        {
            InitializeComponent();
            _radioBossService = new RadioBossService();
            _stayOnTop = stayOnTop;

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;

            // Auto-load status on startup
            Loaded += async (s, e) => await RefreshStatusAsync();
        }

        private async void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            await RefreshStatusAsync();
        }

        private async System.Threading.Tasks.Task RefreshStatusAsync()
        {
            try
            {
                StatusTextBlock.Text = "Checking...";
                ListenersTextBlock.Text = "Loading...";

                bool isOnline = await _radioBossService.IsOnlineAsync();
                StatusTextBlock.Text = isOnline ? "🟢 Online" : "🔴 Offline";

                string listeners = await _radioBossService.GetListenerCountAsync();
                ListenersTextBlock.Text = listeners;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Error";
                MessageBox.Show($"Failed to get status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshTrack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NowPlayingTextBlock.Text = "Loading...";
                string trackInfo = await _radioBossService.GetFormattedTrackInfoAsync();
                NowPlayingTextBlock.Text = trackInfo;
            }
            catch (Exception ex)
            {
                NowPlayingTextBlock.Text = "Error loading track info";
                MessageBox.Show($"Failed to get track info: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadPlaylist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlaylistTextBlock.Text = "Loading playlist...";
                var playlist = await _radioBossService.GetPlaylistAsync();

                if (playlist != null)
                {
                    // Format playlist for display
                    string formattedPlaylist = FormatJsonForDisplay(playlist);
                    PlaylistTextBlock.Text = formattedPlaylist;
                }
                else
                {
                    PlaylistTextBlock.Text = "No playlist data available";
                }
            }
            catch (Exception ex)
            {
                PlaylistTextBlock.Text = "Error loading playlist";
                MessageBox.Show($"Failed to load playlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadEvents_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EventsTextBlock.Text = "Loading events...";
                var events = await _radioBossService.GetUpcomingEventsAsync();

                if (events != null)
                {
                    // Format events for display
                    string formattedEvents = FormatJsonForDisplay(events);
                    EventsTextBlock.Text = formattedEvents;
                }
                else
                {
                    EventsTextBlock.Text = "No upcoming events";
                }
            }
            catch (Exception ex)
            {
                EventsTextBlock.Text = "Error loading events";
                MessageBox.Show($"Failed to load events: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SetArtwork_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string artworkUrl = ArtworkUrlTextBox.Text.Trim();

                if (string.IsNullOrEmpty(artworkUrl))
                {
                    MessageBox.Show("Please enter an artwork URL.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!Uri.IsWellFormedUriString(artworkUrl, UriKind.Absolute))
                {
                    MessageBox.Show("Please enter a valid URL.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool success = await _radioBossService.SetArtworkAsync(artworkUrl);

                if (success)
                {
                    MessageBox.Show("Artwork updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ArtworkUrlTextBox.Clear();
                }
                else
                {
                    MessageBox.Show("Failed to update artwork. Please check the URL and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set artwork: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenWebInterface_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var browserWindow = new RadioBossBrowserWindow(_stayOnTop);
                browserWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open web interface: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FormatJsonForDisplay(JObject json)
        {
            try
            {
                // Pretty-print JSON with indentation
                return json.ToString(Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {
                return json.ToString();
            }
        }
    }
}
