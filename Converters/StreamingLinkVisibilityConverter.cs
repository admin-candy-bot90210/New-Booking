using System;
using System.Globalization;
using System.Windows.Data;
using DJBookingSystem.Models;

namespace DJBookingSystem.Converters
{
    public class StreamingLinkVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
                return "[Hidden]";

            string streamingLink = values[0] as string ?? "[No Link]";
            string venueName = values[1] as string ?? "";
            User? currentUser = values[2] as User;

            if (currentUser == null)
                return "[Hidden]";

            // Show to SysAdmin and Managers
            if (currentUser.Role == UserRole.SysAdmin || currentUser.Role == UserRole.Manager)
                return streamingLink;

            // Show to venue owners
            // We need to check if the current user owns this venue
            // This would require passing venue data, so for now we'll use a simpler approach
            // The MainWindow will handle this logic instead

            return "[Hidden - Venue Owner Only]";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
