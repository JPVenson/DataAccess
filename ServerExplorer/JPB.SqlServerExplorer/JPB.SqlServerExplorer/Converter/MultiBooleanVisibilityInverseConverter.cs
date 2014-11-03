using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace JPB.SqlServerExplorer.Converter
{
    public class MultiBooleanVisibilityInverseConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var convertedValues = values.Cast<Boolean?>().Where(s => s.HasValue).Select(s => s.Value).ToArray();
            return convertedValues.All(s => s) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
