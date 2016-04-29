using System.Collections.Generic;

namespace JPB.DataAccess.EntityCreator.Core.Contracts
{
	public interface IMsSqlCreator : IEntryCreator
	{
		IEnumerable<ITableInfoModel> Tables { get; set; }
		IEnumerable<Dictionary<int, string>> Enums { get; }
		IEnumerable<ITableInfoModel> Views { get; set; }
		IEnumerable<IStoredPrcInfoModel> StoredProcs { get; }
		string TargetDir { get; set; }
		bool GenerateConstructor { get; set; }
		bool GenerateForgeinKeyDeclarations { get; set; }
		bool GenerateCompilerHeader { get; set; }
		bool GenerateConfigMethod { get; set; }
		string Namespace { get; set; }
		string SqlVersion { get; set; }
		void CreateEntrys(string connection, string outputPath, string database);
		void Compile();
	}
}