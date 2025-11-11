using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DJBookingSystem.Models;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class UserManagementWindow : Window
    {
        private FirebaseService _firebaseService;
        private List<User> _users = new List<User>();
        private bool _stayOnTop = false;

        public UserManagementWindow(FirebaseService firebaseService, bool stayOnTop = false)
        {
            InitializeComponent();
            _firebaseService = firebaseService;
            _stayOnTop = stayOnTop;

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;

            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {
                _users = await _firebaseService.GetAllUsersAsync();
                UsersDataGrid.ItemsSource = _users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditUserWindow(_stayOnTop);
            if (editWindow.ShowDialog() == true && editWindow.UpdatedUser != null)
            {
                try
                {
                    // Check if username already exists
                    var existing = await _firebaseService.GetUserByUsernameAsync(editWindow.UpdatedUser.Username);
                    if (existing != null)
                    {
                        MessageBox.Show("Username already exists. Please choose a different username.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    await _firebaseService.AddUserAsync(editWindow.UpdatedUser);
                    MessageBox.Show("User added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User user)
            {
                // Safety check: Ensure user has valid ID
                if (string.IsNullOrEmpty(user.Id))
                {
                    MessageBox.Show("User ID is missing. Cannot edit this user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var editWindow = new EditUserWindow(user, _stayOnTop);
                if (editWindow.ShowDialog() == true && editWindow.UpdatedUser != null)
                {
                    try
                    {
                        await _firebaseService.UpdateUserAsync(user.Id, editWindow.UpdatedUser);
                        MessageBox.Show("User updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a user to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void EditPermissions_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User user)
            {
                // Safety check: Ensure user has valid ID
                if (string.IsNullOrEmpty(user.Id))
                {
                    MessageBox.Show("User ID is missing. Cannot edit permissions.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var permWindow = new EditPermissionsWindow(user, _stayOnTop);
                if (permWindow.ShowDialog() == true && permWindow.UpdatedPermissions != null)
                {
                    try
                    {
                        user.Permissions = permWindow.UpdatedPermissions;
                        await _firebaseService.UpdateUserAsync(user.Id, user);
                        MessageBox.Show("Permissions updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update permissions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a user to edit permissions.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DetailedPermissions_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User user)
            {
                // Safety check: Ensure user has valid ID
                if (string.IsNullOrEmpty(user.Id))
                {
                    MessageBox.Show("User ID is missing. Cannot edit detailed permissions.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var detailedPermWindow = new DetailedPermissionsWindow(user, _stayOnTop);
                if (detailedPermWindow.ShowDialog() == true && detailedPermWindow.UpdatedPermissions != null)
                {
                    try
                    {
                        user.Permissions = detailedPermWindow.UpdatedPermissions;
                        await _firebaseService.UpdateUserAsync(user.Id, user);
                        MessageBox.Show("Detailed permissions updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update detailed permissions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a user to edit detailed permissions.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void ToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User user)
            {
                // Safety check: Ensure user has valid ID
                if (string.IsNullOrEmpty(user.Id))
                {
                    MessageBox.Show("User ID is missing. Cannot toggle status.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    user.IsActive = !user.IsActive;
                    await _firebaseService.UpdateUserAsync(user.Id, user);
                    MessageBox.Show($"User '{user.Username}' is now {(user.IsActive ? "Active" : "Inactive")}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update user status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a user to toggle status.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User user)
            {
                // Safety check: Ensure user has valid ID
                if (string.IsNullOrEmpty(user.Id))
                {
                    MessageBox.Show("User ID is missing. Cannot delete this user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Check if this is the last SysAdmin
                int sysAdminCount = _users.Count(u => u.Role == UserRole.SysAdmin);
                if (user.Role == UserRole.SysAdmin && sysAdminCount <= 1)
                {
                    MessageBox.Show("Cannot delete the last SysAdmin account.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete user '{user.Username}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _firebaseService.DeleteUserAsync(user.Id);
                        MessageBox.Show("User deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a user to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }
    }
}
