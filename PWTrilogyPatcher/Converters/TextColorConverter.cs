using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Globalization;

namespace PWTrilogyPatcher.Converters;

public class TextColorConverter : IValueConverter
{
    public static readonly TextColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool finished)
        {
            return finished ? Brushes.Black : new ImmutableSolidColorBrush(Color.FromRgb(41, 136, 189));
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
