using System;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;

namespace JPB.DataAccess.EntityCreator.Core.Poco
{
	[Serializable]
	public class ForgeinKeyInfoModel : IForgeinKeyInfoModel
	{
		public ForgeinKeyInfoModel()
		{

		}

		[SelectFactoryMethod]
		public static void Callup(RootQuery builder, string tableName, string database)
		{
			builder.QueryText("SELECT ccu.column_name AS SourceColumn ,kcu.table_name AS TargetTable ,kcu.column_name AS TargetColumn")
				.QueryText("FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu")
				.QueryText("INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc")
				.QueryText("ON ccu.CONSTRAINT_NAME = rc.CONSTRAINT_NAME")
				.QueryText("INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu")
				.QueryText("ON kcu.CONSTRAINT_NAME = rc.UNIQUE_CONSTRAINT_NAME")
				.QueryD("WHERE ccu.TABLE_NAME = @tableName AND ccu.TABLE_CATALOG = @database", new
				{
					database = database,
					tableName = tableName
				})
				.QueryText("ORDER BY ccu.table_name");
		}

		[ForModel("TargetTable")]
		public string TableName { get; set; }

		[ForModel("SourceColumn")]
		public string SourceColumn { get; set; }

		[ForModel("TargetColumn")]
		public string TargetColumn { get; set; }

		public override string ToString()
		{
			return string.Format("Column '{0}' references column '{1}' on table '{2}'", SourceColumn, TargetColumn, TableName);
		}
	}
}