using System;
using System.Globalization;

namespace JPB.DataAccess.Contacts
{
	/// <summary>
	///     Converts values from DB to C# and back
	/// </summary>
	public interface IValueConverter
	{
		/// <summary>
		///     Converts a value from a DB to a C# object
		/// </summary>
		/// <returns>C# object that is of type of property</returns>
		object Convert(object value, Type targetType, object parameter, CultureInfo culture);

		/// <summary>
		///     Converts a value from a C# object to the proper DB eqivaluent
		/// </summary>
		/// <returns></returns>
		object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
	}
}