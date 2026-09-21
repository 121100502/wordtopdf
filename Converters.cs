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

#region 百分比转宽度（多值：[0]=总宽度, [1]=百分比）

public class PercentToWidthConverter : IMultiValueConverter
{
    public static readonly PercentToWidthConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is double totalWidth && totalWidth > 0 && values[1] is double percent)
        {
            return Math.Max(0, Math.Min(totalWidth, totalWidth * percent / 100.0));
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
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
