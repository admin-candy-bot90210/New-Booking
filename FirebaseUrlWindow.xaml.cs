using System.Windows;

namespace DJBookingSystem
{
    public partial class FirebaseUrlWindow : Window
    {
        public string FirebaseUrl { get; private set; } = string.Empty;

        public FirebaseUrlWindow()
        {
            InitializeComponent();
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
