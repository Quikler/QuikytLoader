using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace QuikytLoader.AvaloniaUI.Converters;

public class IncrementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int intValue) return value;
        return ++intValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
