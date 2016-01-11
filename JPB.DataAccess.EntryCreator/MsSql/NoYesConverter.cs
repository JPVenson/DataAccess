using System;
using System.Globalization;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.EntityCreator.MsSql
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
			return (bool) value ? "yes" : "no";
		}
	}
}