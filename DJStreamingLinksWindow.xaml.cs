using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class DJStreamingLinksWindow : Window
    {
        private FirebaseService _firebaseService;
        private List<User> _allDJs = new List<User>();
        private User _currentUser;

        public DJStreamingLinksWindow(FirebaseService firebaseService, User currentUser)
        {
            InitializeComponent();
            _firebaseService = firebaseService;
            _currentUser = currentUser;

            // Admin-only check
            if (_currentUser.Role != UserRole.SysAdmin && _currentUser.Role != UserRole.Manager)
            {
                MessageBox.Show("Access Denied: Only administrators can view DJ streaming links directory.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }

            LoadDJLinks();
        }

        private async void LoadDJLinks()
        {
            try
            {
                // Get all users
                var allUsers = await _firebaseService.GetAllUsersAsync();

                // Filter to only DJs with streaming links
                _allDJs = allUsers
                    .Where(u => u.IsDJ && !string.IsNullOrWhiteSpace(u.StreamingLink))
                    .OrderBy(u => u.Username)
                    .ToList();

                DJLinksDataGrid.ItemsSource = _allDJs;
                TotalDJsTextBlock.Text = _allDJs.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load DJ links: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDJLinks();
        }

        private void CopyLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string link)
            {
                try
                {
                    Clipboard.SetText(link);
                    MessageBox.Show("Streaming link copied to clipboard!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to copy link: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string link)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = link,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open link: {ex.Message}\n\nLink: {link}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
