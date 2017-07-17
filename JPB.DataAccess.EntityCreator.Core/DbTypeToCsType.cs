#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using JPB.DataAccess.Contacts;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;

#endregion

namespace JPB.DataAccess.EntityCreator.Core
{
	public class DbTypeToCsType : IValueConverter
	{
		public static Dictionary<string, Type> UserDefinedTypes { get; } = new Dictionary<string, Type>();
		public static Dictionary<SqlDbType, Type> SqlDefinedTypes { get; } = new Dictionary<SqlDbType, Type>();

		static DbTypeToCsType()
		{
			var all = typeof(SqlGeography).Assembly.GetTypes();

			foreach (var item in all)
			{
				var attributes = item.GetCustomAttributes(true);

				if (attributes.Any(f => f is SqlUserDefinedTypeAttribute))
				{
					UserDefinedTypes.Add(item.Name.Replace("Sql", ""), item);
				}
			}

			SqlDefinedTypes.Add(SqlDbType.BigInt, typeof(long));

			SqlDefinedTypes.Add(SqlDbType.Binary, typeof(byte[]));
			SqlDefinedTypes.Add(SqlDbType.Image, typeof(byte[]));
			SqlDefinedTypes.Add(SqlDbType.Timestamp, typeof(byte[]));
			SqlDefinedTypes.Add(SqlDbType.VarBinary, typeof(byte[]));

			SqlDefinedTypes.Add(SqlDbType.TinyInt, typeof(byte));

			SqlDefinedTypes.Add(SqlDbType.Bit, typeof(bool));

			SqlDefinedTypes.Add(SqlDbType.Char, typeof(string));
			SqlDefinedTypes.Add(SqlDbType.NChar, typeof(string));
			SqlDefinedTypes.Add(SqlDbType.NText, typeof(string));
			SqlDefinedTypes.Add(SqlDbType.NVarChar, typeof(string));
			SqlDefinedTypes.Add(SqlDbType.Text, typeof(string));
			SqlDefinedTypes.Add(SqlDbType.VarChar, typeof(string));
			SqlDefinedTypes.Add(SqlDbType.Xml, typeof(string));

			SqlDefinedTypes.Add(SqlDbType.DateTime, typeof(DateTime));
			SqlDefinedTypes.Add(SqlDbType.SmallDateTime, typeof(DateTime));
			SqlDefinedTypes.Add(SqlDbType.Date, typeof(DateTime));
			SqlDefinedTypes.Add(SqlDbType.Time, typeof(DateTime));
			SqlDefinedTypes.Add(SqlDbType.DateTime2, typeof(DateTime));

			SqlDefinedTypes.Add(SqlDbType.Decimal, typeof(decimal));
			SqlDefinedTypes.Add(SqlDbType.Money, typeof(decimal));
			SqlDefinedTypes.Add(SqlDbType.SmallMoney, typeof(decimal));

			SqlDefinedTypes.Add(SqlDbType.Float, typeof(double));

			SqlDefinedTypes.Add(SqlDbType.Int, typeof(int));

			SqlDefinedTypes.Add(SqlDbType.Real, typeof(float));

			SqlDefinedTypes.Add(SqlDbType.UniqueIdentifier, typeof(Guid));

			SqlDefinedTypes.Add(SqlDbType.SmallInt, typeof(short));

			SqlDefinedTypes.Add(SqlDbType.Variant, typeof(object));
			SqlDefinedTypes.Add(SqlDbType.Udt, typeof(object));

			SqlDefinedTypes.Add(SqlDbType.Structured, typeof(DataTable));
			SqlDefinedTypes.Add(SqlDbType.DateTimeOffset, typeof(DateTimeOffset));
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return GetClrType(value as string);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var t = (Type)value;

			foreach (var sqlDefinedType in SqlDefinedTypes)
			{
				if (sqlDefinedType.Value == t)
					return sqlDefinedType.Key;
			}
			return SqlDbType.Variant;
		}

		public static Type GetClrType(string sqlType)
		{
			SqlDbType result;
			var resultState = Enum.TryParse(sqlType, true, out result);
			if (!resultState)
			{
				return UserDefinedTypes.FirstOrDefault(s => s.Key.ToLower() == sqlType).Value;
			}
			return SqlDefinedTypes[result];
		}
		public static Type GetClrType(SqlDbType sqlType)
		{
			return SqlDefinedTypes[sqlType];
		}
	}
}