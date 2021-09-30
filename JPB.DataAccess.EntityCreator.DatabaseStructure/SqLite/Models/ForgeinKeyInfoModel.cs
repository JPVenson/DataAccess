using System;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure.SqLite.Models
{
	[Serializable]
	public class ForgeinKeyInfoModel : IForgeinKeyInfoModel
	{
		public ForgeinKeyInfoModel()
		{

		}

		[SelectFactoryMethod]
		public static IQueryFactoryResult Callup(string tableName)
		{
			return new QueryFactoryResult("PRAGMA foreign_key_list(" + tableName + ");");
		}

		[ForModel("table")]
		public string TableName { get; set; }

		[ForModel("from")]
		public string SourceColumn { get; set; }

		[ForModel("to")]
		public string TargetColumn { get; set; }

		public override string ToString()
		{
			return string.Format("Column '{0}' references column '{1}' on table '{2}'", SourceColumn, TargetColumn, TableName);
		}
	}
}