using System;
using JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts;

namespace JPB.DataAccess.EntityCreator.Core.Models
{
	[Serializable]
	public class StoredPrcInfoModel : IStoredPrcInfoModel
	{
		public IStoredProcedureInformation Parameter { get; set; }
		public bool Exclude { get; set; }
		public string NewTableName { get; set; }

		public StoredPrcInfoModel(IStoredProcedureInformation parameter)
		{
			Parameter = parameter;
		}

		public string GetClassName()
		{
			return string.IsNullOrEmpty(NewTableName) ? Parameter.TableName : NewTableName;
		}
	}
}