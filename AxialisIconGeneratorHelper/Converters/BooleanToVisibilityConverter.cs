#region Usings

using System;
using System.Globalization;
using System.Windows;
using AxialisIconGeneratorHelper.Converters.Base;

#endregion

namespace AxialisIconGeneratorHelper.Converters
{
    public class BooleanToVisibilityConverter : BaseValueConverter<BooleanToVisibilityConverter>
    {
        #region Public Methods

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool valueAsBool)) return value;
            if (parameter is null) return valueAsBool ? Visibility.Visible : Visibility.Collapsed;
            return valueAsBool ? Visibility.Collapsed : Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        #endregion
    }
}