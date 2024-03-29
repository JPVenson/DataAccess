using System.Collections.Generic;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts
{
	public interface IMsSqlCreator : IEntryCreator
	{
		IEnumerable<ISharedInterface> SharedInterfaces { get; set; }
		IEnumerable<ITableInfoModel> Tables { get; set; }
		IEnumerable<Dictionary<int, string>> Enums { get; }
		IEnumerable<ITableInfoModel> Views { get; set; }
		IEnumerable<IStoredPrcInfoModel> StoredProcs { get; }
		string TargetDir { get; set; }
		bool GenerateConstructor { get; set; }
		bool GenerateFactory { get; set; }
		bool GenerateForgeinKeyDeclarations { get; set; }
		bool GenerateCompilerHeader { get; set; }
		bool SetNotifyProperties { get; set; }
		bool GenerateConfigMethod { get; set; }
		bool SplitByType { get; set; }
		bool GenerateDbValidationAnnotations { get; set; }
		string Namespace { get; set; }
		string SqlVersion { get; set; }
		bool WrapNullables { get; set; }
		void Compile();
	}
}