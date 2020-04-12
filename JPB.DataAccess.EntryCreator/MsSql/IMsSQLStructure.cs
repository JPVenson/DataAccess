using JPB.DataAccess.EntityCreator.Core.Poco;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	public interface IMsSqlStructure : IStructureAccess
	{
		string GetVersion();
		TableInformations[] GetTables();
		ViewInformation[] GetViews();
		StoredProcedureInformation[] GetStoredProcedures();
		Any[] GetEnumValuesOfType(string tableName);
		string GetDatabaseName();
	}
}