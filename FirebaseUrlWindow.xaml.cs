using System.Windows;

namespace DJBookingSystem
{
    public partial class FirebaseUrlWindow : Window
    {
        public string FirebaseUrl { get; private set; } = string.Empty;

        public FirebaseUrlWindow(bool stayOnTop = false)
        {
            InitializeComponent();
            // Set default Firebase URL
            FirebaseUrlTextBox.Text = "https://new-booking-system-46908-default-rtdb.firebaseio.com/";

            // Apply Stay on Top preference (not really needed for initial window, but for consistency)
            this.Topmost = stayOnTop;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string url = FirebaseUrlTextBox.Text.Trim();

            if (string.IsNullOrEmpty(url) || !url.StartsWith("https://"))
            {
                MessageBox.Show("Please enter a valid Firebase URL (must start with https://)",
                    "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FirebaseUrl = url;
            DialogResult = true;
            Close();
        }
    }
}
