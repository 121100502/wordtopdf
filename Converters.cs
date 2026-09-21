using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WordToPdfApp.Models;

namespace WordToPdfApp;

#region 布尔取反转换器

public class BoolInverseConverter : IValueConverter
{
    public static readonly BoolInverseConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return value;
    }
}

#endregion

#region 布尔转可见性

public class BoolToVisibilityConverter : IValueConverter
{
    public static readonly BoolToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return b ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility v) return v == Visibility.Visible;
        return false;
    }
}

#endregion

#region 布尔取反转可见性

public class BoolInverseVisibilityConverter : IValueConverter
{
    public static readonly BoolInverseVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility v) return v != Visibility.Visible;
        return true;
    }
}

#endregion

#region 整数转可见性（0 = 可见，>0 = 折叠，用于空状态）

public class IntToVisibilityConverter : IValueConverter
{
    public static readonly IntToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i) return i == 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

#endregion

#region 状态转可见性转换器

public class StatusToVisibilityConverter : IValueConverter
{
    public static readonly StatusToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ConvertStatus status && parameter is string param)
        {
            if (Enum.TryParse<ConvertStatus>(param, out var target))
            {
                return status == target ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

#endregion
