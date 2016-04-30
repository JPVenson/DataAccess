using System;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Poco;

namespace JPB.DataAccess.EntityCreator.Core.Models
{
	[Serializable]
	public class StoredPrcInfoModel : IStoredPrcInfoModel
	{
		public StoredProcedureInformation Parameter { get; set; }
		public bool Exclude { get; set; }
		public string NewTableName { get; set; }

		public StoredPrcInfoModel(StoredProcedureInformation parameter)
		{
			Parameter = parameter;
		}

		public string GetClassName()
		{
			return string.IsNullOrEmpty(NewTableName) ? Parameter.TableName : NewTableName;
		}
	}
}