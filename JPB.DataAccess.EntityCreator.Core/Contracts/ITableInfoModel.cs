using System.Collections.Generic;

namespace JPB.DataAccess.EntityCreator.Core.Contracts
{
	public interface ITableInfoModel
	{
		ITableInformations Info { get; set; }
		string Database { get; set; }
		IEnumerable<IColumInfoModel> ColumnInfos { get; }
		string NewTableName { get; set; }
		bool Exclude { get; set; }
		bool CreateFallbackProperty { get; set; }
		bool CreateSelectFactory { get; set; }
		bool CreateDataRecordLoader { get; set; }
		string GetClassName();
	}

	public interface ISharedInterface
	{
		ISharedInterface Parent { get; set; }
		IList<IColumInfoModel> ContainsColumns { get; }
		string Name { get; set; }
	}
}