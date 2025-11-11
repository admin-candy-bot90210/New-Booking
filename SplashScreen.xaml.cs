using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace DJBookingSystem
{
    public partial class SplashScreen : Window
    {
        private Random _random = new Random();
        private double _currentProgress = 0;

        public SplashScreen()
        {
            InitializeComponent();
            Loaded += SplashScreen_Loaded;
        }

        private async void SplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            await SimulateLoading();
        }

        private async Task SimulateLoading()
        {
            // Simulate random loading increments
            while (_currentProgress < 100)
            {
                // Random increment between 2 and 15
                double increment = _random.Next(2, 16);

                _currentProgress += increment;

                if (_currentProgress > 100)
                    _currentProgress = 100;

                // Update UI
                await Dispatcher.InvokeAsync(() =>
                {
                    // Update percentage text
                    PercentageTextBlock.Text = $"{(int)_currentProgress}%";

                    // Calculate progress bar width
                    double maxWidth = this.Width - 100 - 4; // Container width minus margins and borders
                    double newWidth = (maxWidth * _currentProgress) / 100;
                    ProgressBarFill.Width = newWidth;
                });

                // Random delay between 50ms and 200ms
                int delay = _random.Next(50, 201);
                await Task.Delay(delay);
            }

            // Show "LET'S PARTY!" message
            await Dispatcher.InvokeAsync(() =>
            {
                LoadingTextBlock.Text = "LET'S PARTY!";
                LoadingTextBlock.Foreground = System.Windows.Media.Brushes.Gold;
            });

            // Wait 1 second before fading
            await Task.Delay(1000);

            // Fade out animation
            await Dispatcher.InvokeAsync(() =>
            {
                var fadeOut = (Storyboard)FindResource("FadeOutAnimation");
                fadeOut.Completed += (s, args) =>
                {
                    DialogResult = true;
                    Close();
                };
                fadeOut.Begin(this);
            });
        }
    }
}
