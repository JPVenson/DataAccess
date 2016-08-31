using JPB.DataAccess.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace JPB.DataAccess.Helper
{
	/// <summary>
	///
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Contacts.IValueConverter" />
	public class EnumMemberConverter : IValueConverter
	{
		/// <summary>
		/// Converts a value from a DB to a C# object
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns>
		/// C# object that is of type of property
		/// </returns>
		/// <exception cref="InvalidCastException">No enum member Provided</exception>
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

		/// <summary>
		/// Converts a value from a C# object to the proper DB eqivaluent
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (int)value;
		}
	}
}
