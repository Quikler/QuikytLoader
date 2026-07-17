using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace QuikytLoader.AvaloniaUI.Converters;

public class EnumToSpacedStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue)
            return value;

        var enumStringValue = enumValue.ToString();
        var result = new System.Text.StringBuilder();
        result.Append(enumStringValue[0]);

        for (int i = 1; i < enumStringValue.Length; i++)
        {
            if (char.IsUpper(enumStringValue[i]))
                result.Append(' ');
            result.Append(enumStringValue[i]);
        }

        return result.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
