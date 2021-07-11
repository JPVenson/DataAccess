#region

using System;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

#endregion

namespace JPB.DataAccess.EntityCreator.Core.Poco.SqLite
{
	[Serializable]
	public class ColumnInfo : IColumnInfo
	{
		public ColumnInfo()
		{

		}

		[ObjectFactoryMethod]
		public ColumnInfo(IDataRecord dataRecord)
		{
			ColumnName = dataRecord.GetString(dataRecord.GetOrdinal("name"));
			PositionFromTop = (int) dataRecord.GetInt64(dataRecord.GetOrdinal("cid"));
			Nullable = dataRecord.GetInt64(dataRecord.GetOrdinal("notnull")) == 0;
			IsPrimaryKey = dataRecord.GetInt64(dataRecord.GetOrdinal("pk")) == 1;

			var targetType = dataRecord.GetValue(dataRecord.GetOrdinal("type"));

			switch (targetType)
			{
				case "INTEGER":
					SqlType = SqlDbType.Int;
					break;
				case "BLOB":
					SqlType = SqlDbType.VarBinary;
					break;
				case "TEXT":
					SqlType = SqlDbType.VarChar;
					break;
				case "REAL":
					SqlType = SqlDbType.Real;
					break;
			}
			
			TargetType = (Type)new DbTypeToCsType().Convert(SqlType.ToString(), typeof(Type), null, CultureInfo.CurrentCulture);

			//ColumnName = dataRecord.GetString(dataRecord.GetOrdinal("COLUMN_NAME"));
			//PositionFromTop = dataRecord.GetInt32(dataRecord.GetOrdinal("ORDINAL_POSITION"));
			//var IS_NULLABLE = dataRecord.GetString(dataRecord.GetOrdinal("IS_NULLABLE"));
			//Nullable = IS_NULLABLE == "1" 
			//           || IS_NULLABLE.Equals("yes", StringComparison.InvariantCultureIgnoreCase)
			//           || IS_NULLABLE.Equals("true", StringComparison.InvariantCultureIgnoreCase);
			//var ordinal = dataRecord.GetOrdinal("CHARACTER_MAXIMUM_LENGTH");
			//if(dataRecord.IsDBNull(ordinal))
			//{
			//	MaxLength = dataRecord.GetInt32(ordinal);
			//}

			//var targetType = dataRecord.GetValue(dataRecord.GetOrdinal("DATA_TYPE"));
			//SqlType = (SqlDbType)new SQLDbTypeEnumMemberConverter() { Fallback = SqlDbType.Udt }
			//	.Convert(targetType, typeof(SqlDbType), null, CultureInfo.CurrentCulture);
			//var targetType2 = dataRecord.GetValue(dataRecord.GetOrdinal("DATA_TYPE2"));
			//TargetType = (Type)new DbTypeToCsType().Convert(targetType2, typeof(Type), null, CultureInfo.CurrentCulture);
		}

		public SqlDbType SqlType { get; set; }
		public string ColumnName { get; set; }
		public int PositionFromTop { get; set; }
		public bool Nullable { get; set; }
		public Type TargetType { get; set; }
		public int? MaxLength { get; set; }
		public bool IsPrimaryKey { get; set; }

		[SelectFactoryMethod]
		public static IQueryFactoryResult SelectColumns(string tableName)
		{
			return
					new QueryFactoryResult(
					"PRAGMA table_info(" + tableName + ");");
		}
	}
}