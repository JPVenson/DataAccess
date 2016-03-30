using System;
using System.Globalization;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper
{
	public class EnumMemberConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType.IsEnum)
			{
				return Enum.ToObject(targetType, value);
			}
			throw new InvalidCastException("No enum member Provided");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (int) value;
		}
	}
}