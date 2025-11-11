using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DJBookingSystem.Models;

namespace DJBookingSystem.Converters
{
    /// <summary>
    /// Converts user role and DJ/Owner status to appropriate color
    /// SysAdmin: Rainbow (gradient effect)
    /// Manager: Green
    /// DJ only: Red
    /// Owner only: Blue
    /// Both DJ and Owner: Yellow
    /// </summary>
    public class UserRoleToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
                return new SolidColorBrush(Colors.Gray);

            string role = values[0]?.ToString() ?? "User";
            bool isDJ = values.Length > 1 && values[1] is bool && (bool)values[1];
            bool isOwner = values.Length > 2 && values[2] is bool && (bool)values[2];

            // SysAdmin always gets rainbow/multicolor (we'll use a gradient)
            if (role == "SysAdmin")
            {
                // Create rainbow gradient brush
                var gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new System.Windows.Point(0, 0);
                gradientBrush.EndPoint = new System.Windows.Point(1, 0);
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 0, 0), 0.0));     // Red
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 127, 0), 0.2));   // Orange
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 255, 0), 0.4));   // Yellow
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0, 255, 0), 0.6));     // Green
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0, 0, 255), 0.8));     // Blue
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(139, 0, 255), 1.0));   // Purple
                return gradientBrush;
            }

            // Manager: Green
            if (role == "Manager")
            {
                return new SolidColorBrush(Color.FromRgb(39, 174, 96)); // #27AE60
            }

            // For regular users, check DJ/Owner status
            if (isDJ && isOwner)
            {
                // Both DJ and Owner: Yellow
                return new SolidColorBrush(Color.FromRgb(241, 196, 15)); // #F1C40F
            }
            else if (isDJ)
            {
                // DJ only: Red
                return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // #E74C3C
            }
            else if (isOwner)
            {
                // Owner only: Blue
                return new SolidColorBrush(Color.FromRgb(52, 152, 219)); // #3498DB
            }

            // Default user: Gray
            return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // #95A5A6
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts user role and status to badge background color (solid colors only)
    /// </summary>
    public class UserRoleToBadgeColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
                return new SolidColorBrush(Colors.Gray);

            string role = values[0]?.ToString() ?? "User";
            bool isDJ = values.Length > 1 && values[1] is bool && (bool)values[1];
            bool isOwner = values.Length > 2 && values[2] is bool && (bool)values[2];

            // SysAdmin: Purple (since we can't do gradient in badge)
            if (role == "SysAdmin")
                return new SolidColorBrush(Color.FromRgb(155, 89, 182)); // #9B59B6 Purple

            // Manager: Green
            if (role == "Manager")
                return new SolidColorBrush(Color.FromRgb(39, 174, 96)); // #27AE60

            // Both DJ and Owner: Yellow
            if (isDJ && isOwner)
                return new SolidColorBrush(Color.FromRgb(241, 196, 15)); // #F1C40F

            // DJ only: Red
            if (isDJ)
                return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // #E74C3C

            // Owner only: Blue
            if (isOwner)
                return new SolidColorBrush(Color.FromRgb(52, 152, 219)); // #3498DB

            // Default
            return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // #95A5A6
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts user role and status to display label text
    /// </summary>
    public class UserRoleToLabelConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
                return "User";

            string role = values[0]?.ToString() ?? "User";
            bool isDJ = values.Length > 1 && values[1] is bool && (bool)values[1];
            bool isOwner = values.Length > 2 && values[2] is bool && (bool)values[2];

            if (role == "SysAdmin")
                return "SysAdmin";

            if (role == "Manager")
                return "Manager";

            // Build label based on DJ/Owner status
            if (isDJ && isOwner)
                return "DJ & Owner";
            else if (isDJ)
                return "DJ";
            else if (isOwner)
                return "Owner";

            return "User";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts user role and status to appropriate icon
    /// </summary>
    public class UserRoleToIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
                return "👤";

            string role = values[0]?.ToString() ?? "User";
            bool isDJ = values.Length > 1 && values[1] is bool && (bool)values[1];
            bool isOwner = values.Length > 2 && values[2] is bool && (bool)values[2];

            if (role == "SysAdmin")
                return "👑"; // Crown for SysAdmin

            if (role == "Manager")
                return "⭐"; // Star for Manager

            if (isDJ && isOwner)
                return "🎭"; // Drama masks for both

            if (isDJ)
                return "🎧"; // Headphones for DJ

            if (isOwner)
                return "🏢"; // Building for Owner

            return "👤"; // Person for regular user
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
