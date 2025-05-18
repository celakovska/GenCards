using System.Globalization;
using Microsoft.Maui.Graphics.Platform;

namespace StudyApp.Data;

public class NullToColumnConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value as string) ? 0 : 1;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value as string); // Returns true if value is not null/empty, otherwise false
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullToGridLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value as string) ? new GridLength(0) : GridLength.Auto;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullToMarginConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // If image exists, apply a left margin of 10; otherwise, no margin
        return string.IsNullOrEmpty(value as string) ? new Thickness(0) : new Thickness(10, 0, 0, 0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public static class UtilityFunctions
{
    public static bool FindImageOrientation(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            return false; // Default to landscape

        try
        {
            using var stream = File.OpenRead(imagePath);
            var image = PlatformImage.FromStream(stream);

            return image.Height >= image.Width; // Portrait if height > width
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking image orientation: {ex.Message}");
            return false;
        }
    }
}

