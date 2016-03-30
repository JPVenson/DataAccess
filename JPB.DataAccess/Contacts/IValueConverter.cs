/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
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