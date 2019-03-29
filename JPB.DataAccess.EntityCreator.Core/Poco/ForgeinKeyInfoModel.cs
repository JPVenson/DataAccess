using System;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.EntityCreator.Core.Poco
{
	[Serializable]
	public class ForgeinKeyInfoModel : IForgeinKeyInfoModel
	{
		public ForgeinKeyInfoModel()
		{

		}

		[SelectFactoryMethod]
		public static IQueryFactoryResult Callup(string tableName, string database)
		{
			return new QueryFactoryResult("SELECT ccu.column_name AS SourceColumn, " +
			                              "kcu.table_name AS TargetTable, " +
			                              "kcu.column_name AS TargetColumn " +
			                              "FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu " +
										  "INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc " +
										  "ON ccu.CONSTRAINT_NAME = rc.CONSTRAINT_NAME " +
										  "INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu " +
										  "ON kcu.CONSTRAINT_NAME = rc.UNIQUE_CONSTRAINT_NAME " +
										  "WHERE ccu.TABLE_NAME = @tableName AND ccu.TABLE_CATALOG = @database " +
										  "ORDER BY ccu.table_name",
				new QueryParameter("database", database),
				new QueryParameter("tableName", tableName));
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