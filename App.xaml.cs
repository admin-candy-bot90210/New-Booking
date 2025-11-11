using System;
using System.Windows;
using DJBookingSystem.Services;

namespace DJBookingSystem
{
    public partial class App : Application
    {
        private static FirebaseService? _globalFirebaseService;
        private static string _currentUsername = "System";

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Setup global exception handlers
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            try
            {
                // Step 1: Show splash screen
                var splashScreen = new SplashScreen();
                splashScreen.ShowDialog();

                // Step 2: Get Firebase URL
                var firebaseUrlWindow = new FirebaseUrlWindow();
                if (firebaseUrlWindow.ShowDialog() != true)
                {
                    Shutdown();
                    return;
                }

                var firebaseService = new FirebaseService(firebaseUrlWindow.FirebaseUrl);
                _globalFirebaseService = firebaseService;

                // Step 3: Initialize default admin if needed
                await firebaseService.InitializeDefaultAdminAsync();

                // Step 4: Check for auto-login
                User? loggedInUser = null;
                var savedLogin = Services.LocalStorage.GetLoginInfo();

                if (savedLogin != null && savedLogin.AutoLogin && !string.IsNullOrEmpty(savedLogin.Username))
                {
                    // Attempt auto-login
                    var user = await firebaseService.GetUserByUsernameAsync(savedLogin.Username);
                    if (user != null && user.IsActive && user.AppPreferences?.AutoLogin == true)
                    {
                        loggedInUser = user;
                        user.LastLogin = DateTime.Now;

                        // Safety check: Only update if user has valid ID
                        if (!string.IsNullOrEmpty(user.Id))
                        {
                            await firebaseService.UpdateUserAsync(user.Id, user);
                        }
                    }
                }

                // Step 5: Show login window if auto-login failed
                if (loggedInUser == null)
                {
                    var loginWindow = new LoginWindow(firebaseService);
                    if (loginWindow.ShowDialog() != true || loginWindow.LoggedInUser == null)
                    {
                        Shutdown();
                        return;
                    }
                    loggedInUser = loginWindow.LoggedInUser;
                }

                // Set current username for error logging
                _currentUsername = loggedInUser?.Username ?? "System";

                // Step 6: Load app settings
                var appSettings = await firebaseService.GetAppSettingsAsync();

                // Step 7: Check for updates
                await CheckForUpdatesAsync(firebaseService);

                // Step 8: Show main window
                var mainWindow = new MainWindow(firebaseService, loggedInUser, appSettings);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application startup failed: {ex.Message}", "Startup Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogErrorToSysAdmin(ex, "UNHANDLED");
                MessageBox.Show($"A critical error occurred: {ex.Message}\n\nThis error has been logged and sent to the system administrator.",
                    "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogErrorToSysAdmin(e.Exception, "DISPATCHER");
            MessageBox.Show($"An error occurred: {e.Exception.Message}\n\nThis error has been logged and sent to the system administrator.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private static async void LogErrorToSysAdmin(Exception ex, string errorCode)
        {
            if (_globalFirebaseService != null)
            {
                try
                {
                    string errorMessage = $"{ex.GetType().Name}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                    await _globalFirebaseService.LogErrorToChatAsync(errorMessage, errorCode, _currentUsername);
                }
                catch
                {
                    // Silently fail if logging fails
                }
            }
        }

        private static async System.Threading.Tasks.Task CheckForUpdatesAsync(FirebaseService firebaseService)
        {
            try
            {
                bool updateAvailable = await firebaseService.IsUpdateAvailableAsync();
                if (updateAvailable)
                {
                    var latestVersion = await firebaseService.GetLatestVersionAsync();
                    if (latestVersion != null)
                    {
                        string message = $"A new version ({latestVersion.Version}) is available!\n\n" +
                                       $"Release Date: {latestVersion.ReleaseDate:MM/dd/yyyy}\n\n" +
                                       $"Release Notes:\n{latestVersion.ReleaseNotes}\n\n" +
                                       $"Current Version: {firebaseService.GetCurrentVersion()}\n\n";

                        if (latestVersion.IsMandatory)
                        {
                            message += "This is a mandatory update. Please update to continue using the application.";
                            MessageBox.Show(message, "Update Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            message += "Would you like to download the update now?";
                            var result = MessageBox.Show(message, "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);

                            if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(latestVersion.DownloadUrl))
                            {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = latestVersion.DownloadUrl,
                                    UseShellExecute = true
                                });
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silently fail if update check fails
            }
        }
    }
}
