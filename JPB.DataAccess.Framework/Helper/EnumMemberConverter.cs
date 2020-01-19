#region

using System;
using System.Globalization;
using JPB.DataAccess.Contacts;

#endregion

namespace JPB.DataAccess.Helper
{
	/// <summary>
	///		Standard number to Enum converter for Enum fields
	/// </summary>
	/// <seealso cref="IValueConverter" />
	public class EnumMemberConverter : IValueConverter
	{
		/// <summary>
		///     Converts a value from a DB to a C# object
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns>
		///     C# object that is of type of property
		/// </returns>
		/// <exception cref="InvalidCastException">No enum member Provided</exception>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType.IsEnum)
			{
				return Enum.Parse(targetType, value.ToString(), true);
			}
			throw new InvalidCastException("No enum member Provided");
		}

		/// <summary>
		///     Converts a value from a C# object to the proper DB eqivaluent
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(int))
			{
				return (int)value;
			}
			if (targetType == typeof(string))
			{
				return (string) value;
			}
			return value;
		}
	}
	
	/// <summary>
	///		Standard number to Enum converter for Enum fields
	/// </summary>
	/// <seealso cref="IValueConverter" />
	public class SQLDbTypeEnumMemberConverter : IValueConverter
	{
		public object Fallback { get; set; }

		/// <summary>
		///     Converts a value from a DB to a C# object
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns>
		///     C# object that is of type of property
		/// </returns>
		/// <exception cref="InvalidCastException">No enum member Provided</exception>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType.IsEnum)
			{
				try
				{
					var convert = Enum.Parse(targetType, value.ToString(), true);
					return convert;
				}
				catch (Exception e)
				{
					return Fallback;
				}
			}
			throw new InvalidCastException("No enum member Provided");
		}

		/// <summary>
		///     Converts a value from a C# object to the proper DB eqivaluent
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(int))
			{
				return (int)value;
			}
			if (targetType == typeof(string))
			{
				return (string) value;
			}
			return value;
		}
	}
}