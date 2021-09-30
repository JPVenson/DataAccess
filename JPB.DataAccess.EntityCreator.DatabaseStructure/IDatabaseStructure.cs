using JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure
{
	public interface IDatabaseStructure : IStructureAccess
	{
		string GetVersion();
		ITableInformations[] GetTables();
		ITableInformations[] GetViews();
		IStoredProcedureInformation[] GetStoredProcedures();
		Any[] GetEnumValuesOfType(string tableName);
		string GetDatabaseName();
	}
}