namespace JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts
{
	public interface IStoredPrcInfoModel
	{
		IStoredProcedureInformation Parameter { get; set; }
		bool Exclude { get; set; }
		string NewTableName { get; set; }
		string GetClassName();
	}
}