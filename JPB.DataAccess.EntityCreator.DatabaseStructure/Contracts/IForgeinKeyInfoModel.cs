namespace JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts
{
	public interface IForgeinKeyInfoModel
	{
		string TableName { get; set; }
		string SourceColumn { get; set; }
		string TargetColumn { get; set; }
	}
}