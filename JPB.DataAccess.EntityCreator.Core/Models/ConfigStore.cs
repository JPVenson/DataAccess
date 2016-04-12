using System;
using System.Collections.Generic;
using JPB.DataAccess.EntityCreator.Core.Contracts;

namespace JPB.DataAccess.EntityCreator.Core.Models
{
	[Serializable]
	public class ConfigStore
	{
		public string SourceConnectionString { get; set; }

		public List<ITableInfoModel> Tables { get; set; }
		public List<ITableInfoModel> Views { get; set; }
		public List<IStoredPrcInfoModel> StoredPrcInfoModels { get; set; }

		public bool GenerateConstructor { get; set; }
		public bool GenerateForgeinKeyDeclarations { get; set; }
		public bool GenerateCompilerHeader { get; set; }
		public bool GenerateConfigMethod { get; set; }
		public string Namespace { get; set; }
		public string TargetDir { get; set; }
	}
}
