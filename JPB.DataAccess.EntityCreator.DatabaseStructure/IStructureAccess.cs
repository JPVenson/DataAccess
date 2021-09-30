using JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure
{
	public interface IStructureAccess
	{
		IColumnInfo[] GetColumnsOf(string table, string database);
		string GetPrimaryKeyOf(string table, string database);
		IForgeinKeyInfoModel[] GetForeignKeys(string table, string database);
	}
}