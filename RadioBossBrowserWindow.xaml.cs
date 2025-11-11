using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace DJBookingSystem
{
    public partial class RadioBossBrowserWindow : Window
    {
        private const string LoginUrl = "https://c40.radioboss.fm/#main";
        private const string Username = "Remote";
        private const string Password = "Remote";
        private bool _isInitialized = false;

        public RadioBossBrowserWindow(bool stayOnTop = false)
        {
            InitializeComponent();

            // Apply Stay on Top preference
            this.Topmost = stayOnTop;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                // Initialize WebView2
                await RadioBossWebView.EnsureCoreWebView2Async(null);
                _isInitialized = true;

                // Subscribe to navigation events
                RadioBossWebView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                RadioBossWebView.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;

                // Navigate to RadioBOSS Cloud
                RadioBossWebView.CoreWebView2.Navigate(LoginUrl);
                StatusTextBlock.Text = "Loading RadioBOSS Cloud...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize browser: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Error loading browser";
            }
        }

        private async void CoreWebView2_DOMContentLoaded(object? sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            // Wait a moment for the page to fully render
            await Task.Delay(1500);

            // Attempt auto-login by injecting JavaScript
            await AttemptAutoLogin();
        }

        private void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                StatusTextBlock.Text = "Connected to RadioBOSS Cloud";
            }
            else
            {
                StatusTextBlock.Text = "Navigation failed";
            }
        }

        private async Task AttemptAutoLogin()
        {
            try
            {
                // JavaScript to auto-fill and submit login form
                // This will need to be adjusted based on RadioBOSS Cloud's actual login form structure
                string script = $@"
                    (function() {{
                        try {{
                            // Look for username/email input field
                            var usernameField = document.querySelector('input[name=""username""]') ||
                                              document.querySelector('input[name=""email""]') ||
                                              document.querySelector('input[type=""text""]') ||
                                              document.querySelector('input[id*=""user""]') ||
                                              document.querySelector('input[id*=""login""]');

                            // Look for password input field
                            var passwordField = document.querySelector('input[name=""password""]') ||
                                              document.querySelector('input[type=""password""]');

                            // Look for login button
                            var loginButton = document.querySelector('button[type=""submit""]') ||
                                            document.querySelector('input[type=""submit""]') ||
                                            document.querySelector('button:contains(""Login"")') ||
                                            document.querySelector('button:contains(""Sign in"")');

                            if (usernameField && passwordField) {{
                                usernameField.value = '{Username}';
                                passwordField.value = '{Password}';

                                // Trigger input events to ensure form validation
                                usernameField.dispatchEvent(new Event('input', {{ bubbles: true }}));
                                passwordField.dispatchEvent(new Event('input', {{ bubbles: true }}));

                                if (loginButton) {{
                                    // Small delay before clicking login
                                    setTimeout(function() {{
                                        loginButton.click();
                                    }}, 500);
                                    return 'Login attempted';
                                }} else {{
                                    // Try form submission if no button found
                                    var form = document.querySelector('form');
                                    if (form) {{
                                        form.submit();
                                        return 'Form submitted';
                                    }}
                                }}
                            }} else {{
                                return 'Login form not found - may already be logged in';
                            }}
                        }} catch (e) {{
                            return 'Error: ' + e.message;
                        }}
                    }})();
                ";

                string result = await RadioBossWebView.CoreWebView2.ExecuteScriptAsync(script);

                // Update status
                await Dispatcher.InvokeAsync(() =>
                {
                    StatusTextBlock.Text = "Auto-login attempted";
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    StatusTextBlock.Text = $"Auto-login failed: {ex.Message}";
                });
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitialized && RadioBossWebView.CoreWebView2 != null)
            {
                RadioBossWebView.CoreWebView2.Reload();
                StatusTextBlock.Text = "Reloading...";
            }
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitialized && RadioBossWebView.CoreWebView2 != null)
            {
                RadioBossWebView.CoreWebView2.Navigate(LoginUrl);
                StatusTextBlock.Text = "Returning to home...";
            }
        }
    }
}
