using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	[Serializable]
	public class ConfigStore
	{
		public string SourceConnectionString { get; set; }

		public List<ITableInfoModel> Tables { get; set; }
		public List<ITableInfoModel> Views { get; set; }
		public List<StoredPrcInfoModel> StoredPrcInfoModels { get; set; }

		public bool GenerateConstructor { get; set; }
		public bool GenerateForgeinKeyDeclarations { get; set; }
		public bool GenerateCompilerHeader { get; set; }
		public bool GenerateConfigMethod { get; set; }
		public string Namespace { get; set; }
		public string TargetDir { get; set; }
	}
}
