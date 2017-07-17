#region

using System;
using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

#endregion

namespace JPB.DataAccess.EntityCreator.Core.Poco
{
	[Serializable]
	public class ColumnInfo : IColumnInfo
	{
		public ColumnInfo()
		{
			;
			;
		}

		[ForModel("DATA_TYPE")]
		[ValueConverter(typeof(EnumMemberConverter))]
		public SqlDbType SqlType { get; set; }

		[ForModel("COLUMN_NAME")]
		public string ColumnName { get; set; }

		[ForModel("ORDINAL_POSITION")]
		public int PositionFromTop { get; set; }

		[ForModel("IS_NULLABLE")]
		[ValueConverter(typeof(NoYesConverter))]
		public bool Nullable { get; set; }

		[ForModel("DATA_TYPE2")]
		[ValueConverter(typeof(DbTypeToCsType))]
		public Type TargetType { get; set; }

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