using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Poco;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.EntityCreator.MsSql
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