using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NAudio.Wave;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class RadioPlayerWindow : Window
    {
        private FirebaseService _firebaseService;
        private User _currentUser;
        private IWavePlayer? _wavePlayer;
        private MediaFoundationReader? _mediaReader;
        private List<RadioStation> _savedStations = new List<RadioStation>();

        public RadioPlayerWindow(FirebaseService firebaseService, User currentUser)
        {
            InitializeComponent();
            _firebaseService = firebaseService;
            _currentUser = currentUser;

            LoadSavedStations();
        }

        private async void LoadSavedStations()
        {
            try
            {
                _savedStations = await _firebaseService.GetAllRadioStationsAsync();
                StationsListBox.ItemsSource = _savedStations;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Failed to load saved stations: {ex.Message}";
            }
        }

        private void LoadStream_Click(object sender, RoutedEventArgs e)
        {
            string streamUrl = StreamUrlTextBox.Text.Trim();

            if (string.IsNullOrEmpty(streamUrl))
            {
                MessageBox.Show("Please enter a stream URL.", "No URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!streamUrl.StartsWith("http://") && !streamUrl.StartsWith("https://"))
            {
                MessageBox.Show("Please enter a valid URL starting with http:// or https://", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Stop any existing stream
                StopStream();

                // Load and play the new stream
                StatusText.Text = "Loading stream...";

                _mediaReader = new MediaFoundationReader(streamUrl);
                _wavePlayer = new WaveOutEvent();
                _wavePlayer.Init(_mediaReader);
                _wavePlayer.Volume = (float)(VolumeSlider.Value / 100.0);
                _wavePlayer.Play();

                PlayButton.IsEnabled = false;
                PauseButton.IsEnabled = true;
                StopButton.IsEnabled = true;

                string stationName = StationNameTextBox.Text.Trim();
                CurrentStationText.Text = !string.IsNullOrEmpty(stationName) ? $"Playing: {stationName}" : "Playing stream";
                StatusText.Text = $"Now playing: {streamUrl}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load stream: {ex.Message}\n\nMake sure the URL is a valid audio stream.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Failed to load stream";

                // Log error
                _ = _firebaseService.LogErrorToChatAsync(ex.Message, "RADIO001", _currentUser.Username);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (_wavePlayer != null && _mediaReader != null)
            {
                _wavePlayer.Play();
                PlayButton.IsEnabled = false;
                PauseButton.IsEnabled = true;
                StopButton.IsEnabled = true;
                StatusText.Text = "Playing...";
            }
            else
            {
                MessageBox.Show("Please load a stream first.", "No Stream", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (_wavePlayer != null)
            {
                _wavePlayer.Pause();
                PlayButton.IsEnabled = true;
                PauseButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                StatusText.Text = "Paused";
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopStream();
        }

        private void StopStream()
        {
            if (_wavePlayer != null)
            {
                _wavePlayer.Stop();
                _wavePlayer.Dispose();
                _wavePlayer = null;
            }

            if (_mediaReader != null)
            {
                _mediaReader.Dispose();
                _mediaReader = null;
            }

            PlayButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            CurrentStationText.Text = "No station playing";
            StatusText.Text = "Stopped";
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeText != null)
            {
                VolumeText.Text = $"{(int)VolumeSlider.Value}%";
            }

            if (_wavePlayer != null)
            {
                _wavePlayer.Volume = (float)(VolumeSlider.Value / 100.0);
            }
        }

        private async void SaveStation_Click(object sender, RoutedEventArgs e)
        {
            string streamUrl = StreamUrlTextBox.Text.Trim();
            string stationName = StationNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(streamUrl))
            {
                MessageBox.Show("Please enter a stream URL.", "No URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(stationName))
            {
                MessageBox.Show("Please enter a station name.", "No Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var station = new RadioStation
                {
                    Name = stationName,
                    StreamUrl = streamUrl,
                    Genre = "",
                    AddedBy = _currentUser.Username,
                    AddedDate = DateTime.Now,
                    IsFavorite = false
                };

                await _firebaseService.AddRadioStationAsync(station);
                MessageBox.Show($"Station '{stationName}' saved successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                StationNameTextBox.Clear();
                LoadSavedStations();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save station: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Log error
                await _firebaseService.LogErrorToChatAsync(ex.Message, "RADIO002", _currentUser.Username);
            }
        }

        private void Station_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (StationsListBox.SelectedItem is RadioStation station)
            {
                StreamUrlTextBox.Text = station.StreamUrl;
                StationNameTextBox.Text = station.Name;
                LoadStream_Click(sender, e);
            }
        }

        private async void DeleteStation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string stationId)
            {
                var station = _savedStations.Find(s => s.Id == stationId);
                if (station == null) return;

                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{station.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _firebaseService.DeleteRadioStationAsync(stationId);
                        MessageBox.Show("Station deleted successfully!", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadSavedStations();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete station: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RefreshStations_Click(object sender, RoutedEventArgs e)
        {
            LoadSavedStations();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopStream();
        }
    }
}
