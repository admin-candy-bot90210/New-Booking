using System.Windows;

namespace DJBookingSystem
{
    public partial class HelpGuideWindow : Window
    {
        public HelpGuideWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
