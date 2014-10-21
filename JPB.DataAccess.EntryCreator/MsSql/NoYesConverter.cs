using System;
using System.Globalization;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntryCreator.MsSql
{
    class NoYesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sValue = value as string;
            return sValue.ToLower() == "yes";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}