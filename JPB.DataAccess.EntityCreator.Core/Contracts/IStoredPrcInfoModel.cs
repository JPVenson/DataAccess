using JPB.DataAccess.EntityCreator.Core.Poco;

namespace JPB.DataAccess.EntityCreator.Core.Contracts
{
	public interface IStoredPrcInfoModel
	{
		StoredProcedureInformation Parameter { get; set; }
		bool Exclude { get; set; }
		string NewTableName { get; set; }
		string GetClassName();
	}
}