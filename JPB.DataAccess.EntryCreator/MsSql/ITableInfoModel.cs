using System.Collections.Generic;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	public interface ITableInfoModel
	{
		TableInformations Info { get; set; }
		string Database { get; set; }
		IEnumerable<IColumInfoModel> ColumnInfos { get; }
		string NewTableName { get; set; }
		bool Exclude { get; set; }
		bool CreateFallbackProperty { get; set; }
		bool CreateSelectFactory { get; set; }
		bool CreateDataRecordLoader { get; set; }
		string GetClassName();
	}
}