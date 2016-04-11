using System.Collections.Generic;
using JPB.DataAccess.EntityCreator.Core.Contracts;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	public interface IMsSqlCreator
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