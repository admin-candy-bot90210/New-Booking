using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class ModerationWindow : Window
    {
        private FirebaseService _firebaseService;
        private User _currentUser;
        private List<UserReport> _allReports = new List<UserReport>();

        public ModerationWindow(FirebaseService firebaseService, User currentUser)
        {
            InitializeComponent();
            _firebaseService = firebaseService;
            _currentUser = currentUser;

            // Load initial data
            LoadReports();
            LoadBannedUsers();
            LoadMutedUsers();
        }

        #region User Reports

        private async void LoadReports()
        {
            try
            {
                _allReports = await _firebaseService.GetAllUserReportsAsync();
                FilterAndDisplayReports();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load reports: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterAndDisplayReports()
        {
            var filtered = _allReports;

            if (ShowResolvedCheckBox.IsChecked == false)
            {
                filtered = filtered.Where(r => !r.IsResolved).ToList();
            }

            ReportsDataGrid.ItemsSource = filtered;
        }

        private void RefreshReports_Click(object sender, RoutedEventArgs e)
        {
            LoadReports();
        }

        private void ShowResolved_Changed(object sender, RoutedEventArgs e)
        {
            FilterAndDisplayReports();
        }

        private void ViewReport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is UserReport report)
            {
                var dialog = new Window
                {
                    Title = $"Report Details - {report.ReportedUsername}",
                    Width = 500,
                    Height = 450,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var panel = new StackPanel { Margin = new Thickness(15) };

                // Report Details
                panel.Children.Add(new TextBlock
                {
                    Text = "Report Details",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 15)
                });

                panel.Children.Add(CreateInfoRow("Reported User:", report.ReportedUsername));
                panel.Children.Add(CreateInfoRow("Reported By:", report.ReporterUsername));
                panel.Children.Add(CreateInfoRow("Reason:", report.Reason));
                panel.Children.Add(CreateInfoRow("Date:", report.ReportedAt.ToString("MMM dd, yyyy HH:mm")));

                panel.Children.Add(new TextBlock
                {
                    Text = "Details:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                });

                panel.Children.Add(new TextBox
                {
                    Text = report.Details,
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    Height = 80,
                    Margin = new Thickness(0, 0, 0, 15),
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                });

                if (report.IsResolved)
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = "Resolution",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 10, 0, 10),
                        Foreground = System.Windows.Media.Brushes.Green
                    });

                    panel.Children.Add(CreateInfoRow("Resolved By:", report.ResolvedBy ?? "N/A"));
                    panel.Children.Add(CreateInfoRow("Resolved At:", report.ResolvedAt?.ToString("MMM dd, yyyy HH:mm") ?? "N/A"));

                    panel.Children.Add(new TextBlock
                    {
                        Text = "Admin Notes:",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 10, 0, 5)
                    });

                    panel.Children.Add(new TextBox
                    {
                        Text = report.AdminNotes ?? "No notes",
                        TextWrapping = TextWrapping.Wrap,
                        IsReadOnly = true,
                        Height = 60,
                        Margin = new Thickness(0, 0, 0, 15),
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    });
                }

                var closeButton = new Button
                {
                    Content = "Close",
                    Padding = new Thickness(15, 8, 15, 8),
                    Background = System.Windows.Media.Brushes.Gray,
                    Foreground = System.Windows.Media.Brushes.White,
                    BorderThickness = new Thickness(0),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                closeButton.Click += (s, args) => dialog.Close();
                panel.Children.Add(closeButton);

                dialog.Content = panel;
                dialog.ShowDialog();
            }
        }

        private StackPanel CreateInfoRow(string label, string value)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };

            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.Bold,
                Width = 120
            });

            panel.Children.Add(new TextBlock
            {
                Text = value,
                TextWrapping = TextWrapping.Wrap
            });

            return panel;
        }

        private async void ResolveReport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is UserReport report)
            {
                var dialog = new Window
                {
                    Title = "Resolve Report",
                    Width = 450,
                    Height = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var panel = new StackPanel { Margin = new Thickness(15) };

                panel.Children.Add(new TextBlock
                {
                    Text = $"Resolving report for: {report.ReportedUsername}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 15)
                });

                panel.Children.Add(new TextBlock
                {
                    Text = "Admin Notes (optional):",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                });

                var notesTextBox = new TextBox
                {
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                    Height = 100,
                    Margin = new Thickness(0, 0, 0, 15),
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                panel.Children.Add(notesTextBox);

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                var resolveButton = new Button
                {
                    Content = "✓ Mark Resolved",
                    Padding = new Thickness(15, 8, 15, 8),
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = System.Windows.Media.Brushes.Green,
                    Foreground = System.Windows.Media.Brushes.White,
                    BorderThickness = new Thickness(0)
                };
                resolveButton.Click += async (s, args) =>
                {
                    try
                    {
                        report.IsResolved = true;
                        report.ResolvedBy = _currentUser.Username;
                        report.ResolvedAt = DateTime.Now;
                        report.AdminNotes = notesTextBox.Text.Trim();

                        await _firebaseService.UpdateUserReportAsync(report.Id!, report);
                        dialog.Close();
                        MessageBox.Show("Report has been marked as resolved.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadReports();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to resolve report: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                var cancelButton = new Button
                {
                    Content = "Cancel",
                    Padding = new Thickness(15, 8, 15, 8),
                    Background = System.Windows.Media.Brushes.Gray,
                    Foreground = System.Windows.Media.Brushes.White,
                    BorderThickness = new Thickness(0)
                };
                cancelButton.Click += (s, args) => dialog.Close();

                buttonPanel.Children.Add(resolveButton);
                buttonPanel.Children.Add(cancelButton);
                panel.Children.Add(buttonPanel);

                dialog.Content = panel;
                dialog.ShowDialog();
            }
        }

        #endregion

        #region Banned Users

        private async void LoadBannedUsers()
        {
            try
            {
                var bannedUsers = await _firebaseService.GetBannedUsersAsync();
                BannedUsersDataGrid.ItemsSource = bannedUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load banned users: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshBannedUsers_Click(object sender, RoutedEventArgs e)
        {
            LoadBannedUsers();
        }

        private async void UnbanUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string username)
            {
                var result = MessageBox.Show($"Unban {username}?\n\nThe user will be able to access the system again.",
                    "Confirm Unban", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _firebaseService.UnbanUserAsync(username, _currentUser.Username);
                        MessageBox.Show($"{username} has been unbanned.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadBannedUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to unban user: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        #endregion

        #region Muted Users

        private async void LoadMutedUsers()
        {
            try
            {
                var mutedUsers = await _firebaseService.GetMutedUsersAsync();
                MutedUsersDataGrid.ItemsSource = mutedUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load muted users: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshMutedUsers_Click(object sender, RoutedEventArgs e)
        {
            LoadMutedUsers();
        }

        private async void UnmuteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string username)
            {
                var result = MessageBox.Show($"Unmute {username}?\n\nThe user will be able to send messages again.",
                    "Confirm Unmute", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _firebaseService.UnmuteUserAsync(username, _currentUser.Username);
                        MessageBox.Show($"{username} has been unmuted.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadMutedUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to unmute user: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        #endregion
    }
}
