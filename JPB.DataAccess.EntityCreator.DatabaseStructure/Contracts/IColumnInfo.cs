using System;
using System.Data;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts
{
	public interface IColumnInfo
	{
		string ColumnName { get; set; }
		int PositionFromTop { get; set; }
		bool Nullable { get; set; }
		Type TargetType { get; set; }
		SqlDbType SqlType { get; set; }
		int? MaxLength { get; set; }
	}
}