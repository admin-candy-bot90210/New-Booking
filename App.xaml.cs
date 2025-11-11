using System;
using System.Windows;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Step 1: Get Firebase URL
                var firebaseUrlWindow = new FirebaseUrlWindow();
                if (firebaseUrlWindow.ShowDialog() != true)
                {
                    Shutdown();
                    return;
                }

                var firebaseService = new FirebaseService(firebaseUrlWindow.FirebaseUrl);

                // Step 2: Initialize default admin if needed
                await firebaseService.InitializeDefaultAdminAsync();

                // Step 3: Show login window
                var loginWindow = new LoginWindow(firebaseService);
                if (loginWindow.ShowDialog() != true || loginWindow.LoggedInUser == null)
                {
                    Shutdown();
                    return;
                }

                // Step 4: Load app settings
                var appSettings = await firebaseService.GetAppSettingsAsync();

                // Step 5: Show main window
                var mainWindow = new MainWindow(firebaseService, loginWindow.LoggedInUser, appSettings);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application startup failed: {ex.Message}", "Startup Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
