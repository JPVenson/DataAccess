using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Globalization;
using JPB.DataAccess.Contacts;
using System.Collections.Generic;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;

namespace JPB.DataAccess.EntityCreator.MsSql
{
    public class DbTypeToCsType : IValueConverter
    {
        static DbTypeToCsType()
        {
            var all = typeof(SqlGeography).Assembly.GetTypes();

            foreach (var item in all)
            {
                if(item.Name == "SqlGeography")
                {
                    Console.WriteLine();
                }

                var attributes = item.GetCustomAttributes(true);

                if(attributes.Any(f => f is SqlUserDefinedTypeAttribute))
                {
                    _userDefinedTypes.Add(item.Name.Replace("Sql", ""), item);
                }
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetClrType(value as string);
        }

        private static Dictionary<string, Type> _userDefinedTypes = new Dictionary<string, Type>();


        public static Type GetClrType(string sqlType)
        {
            SqlDbType result;
            var resultState = Enum.TryParse(sqlType, true, out result);
            if(!resultState)
            {
                return _userDefinedTypes.First(s => s.Key.ToLower() == sqlType).Value;
            }           

            return GetClrType(result);
        }

        public static Type GetClrType(SqlDbType sqlType)
        {
            switch (sqlType)
            {
                case SqlDbType.BigInt:
                    return typeof(long);

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    return typeof(byte[]);

                case SqlDbType.Bit:
                    return typeof(bool);

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    return typeof(string);

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Date:
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                    return typeof(DateTime?);

                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return typeof(decimal);

                case SqlDbType.Float:
                    return typeof(double);

                case SqlDbType.Int:
                    return typeof(int);

                case SqlDbType.Real:
                    return typeof(float);

                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid);

                case SqlDbType.SmallInt:
                    return typeof(short);

                case SqlDbType.TinyInt:
                    return typeof(byte);

                case SqlDbType.Variant:
                case SqlDbType.Udt:
                    return typeof(object);

                case SqlDbType.Structured:
                    return typeof(DataTable);

                case SqlDbType.DateTimeOffset:
                    return typeof(DateTimeOffset?);

                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}