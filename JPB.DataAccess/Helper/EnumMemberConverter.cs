using JPB.DataAccess.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace JPB.DataAccess.Helper
{
	public class EnumMemberConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(targetType.IsEnum)
			{
				return Enum.ToObject(targetType, value);
			}
			else
			{
				throw new InvalidCastException("No enum member Provided");
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (int)value;
		}
	}
}
