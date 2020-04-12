namespace JPB.DataAccess.EntityCreator.Core.Poco
{
	public interface IStructureAccess
	{
		ColumnInfo[] GetColumnsOf(string table, string database);
		string GetPrimaryKeyOf(string table, string database);
		ForgeinKeyInfoModel[] GetForeignKeys(string table, string database);
	}
}