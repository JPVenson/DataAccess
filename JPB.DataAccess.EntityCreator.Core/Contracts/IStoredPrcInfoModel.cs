using JPB.DataAccess.EntityCreator.Core.Poco;

namespace JPB.DataAccess.EntityCreator.Core.Contracts
{
	public interface IStoredPrcInfoModel
	{
		IStoredProcedureInformation Parameter { get; set; }
		bool Exclude { get; set; }
		string NewTableName { get; set; }
		string GetClassName();
	}
}