#region

using System;
using System.Data;
using System.Globalization;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

#endregion

namespace JPB.DataAccess.EntityCreator.DatabaseStructure.MsSql.Models
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
			ColumnName = dataRecord.GetString(dataRecord.GetOrdinal("COLUMN_NAME"));
			PositionFromTop = dataRecord.GetInt32(dataRecord.GetOrdinal("ORDINAL_POSITION"));
			var IS_NULLABLE = dataRecord.GetString(dataRecord.GetOrdinal("IS_NULLABLE"));
			Nullable = IS_NULLABLE == "1" 
			           || IS_NULLABLE.Equals("yes", StringComparison.InvariantCultureIgnoreCase)
			           || IS_NULLABLE.Equals("true", StringComparison.InvariantCultureIgnoreCase);
			var ordinal = dataRecord.GetOrdinal("CHARACTER_MAXIMUM_LENGTH");
			if(dataRecord.IsDBNull(ordinal))
			{
				MaxLength = dataRecord.GetInt32(ordinal);
			}

			var targetType = dataRecord.GetValue(dataRecord.GetOrdinal("DATA_TYPE"));
			SqlType = (SqlDbType)new SQLDbTypeEnumMemberConverter() { Fallback = SqlDbType.Udt }
				.Convert(targetType, typeof(SqlDbType), null, CultureInfo.CurrentCulture);
			var targetType2 = dataRecord.GetValue(dataRecord.GetOrdinal("DATA_TYPE2"));
			TargetType = (Type)new DbTypeToCsType().Convert(targetType2, typeof(Type), null, CultureInfo.CurrentCulture);
		}

		[ForModel("DATA_TYPE")]
		public SqlDbType SqlType { get; set; }

		[ForModel("COLUMN_NAME")]
		public string ColumnName { get; set; }

		[ForModel("ORDINAL_POSITION")]
		public int PositionFromTop { get; set; }

		[ForModel("IS_NULLABLE")]
		[ValueConverter(typeof(NoYesConverter))]
		public bool Nullable { get; set; }

		[ForModel("DATA_TYPE2")]
		public Type TargetType { get; set; }

		[ForModel("CHARACTER_MAXIMUM_LENGTH")]
		public int? MaxLength { get; set; }

		[SelectFactoryMethod]
		public static IQueryFactoryResult SelectColumns(string tableName, string database)
		{
			return
					new QueryFactoryResult(
					"SELECT *, DATA_TYPE AS DATA_TYPE2 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName AND TABLE_CATALOG = @database",
					new QueryParameter("@tableName", tableName), new QueryParameter("@database", database));
		}
	}
}