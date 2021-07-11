using JPB.DataAccess.EntityCreator.Core.Contracts;

namespace JPB.DataAccess.EntityCreator.Core.Poco
{
	public interface IStructureAccess
	{
		IColumnInfo[] GetColumnsOf(string table, string database);
		string GetPrimaryKeyOf(string table, string database);
		IForgeinKeyInfoModel[] GetForeignKeys(string table, string database);
	}
}