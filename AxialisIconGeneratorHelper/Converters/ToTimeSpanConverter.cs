#region Usings

using System;
using System.Globalization;
using System.Windows;
using AxialisIconGeneratorHelper.Converters.Base;

#endregion

namespace AxialisIconGeneratorHelper.Converters
{
    public class ToTimeSpanConverter : BaseValueConverter<ToTimeSpanConverter>
    {
        #region Public Methods

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan duration) return duration.TotalMilliseconds;
            return DependencyProperty.UnsetValue;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalMilliseconds) return TimeSpan.FromMilliseconds(totalMilliseconds);
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}