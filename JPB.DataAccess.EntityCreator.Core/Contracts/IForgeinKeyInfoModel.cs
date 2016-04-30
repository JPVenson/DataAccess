namespace JPB.DataAccess.EntityCreator.Core.Contracts
{
	public interface IForgeinKeyInfoModel
	{
		string TableName { get; set; }
		string SourceColumn { get; set; }
		string TargetColumn { get; set; }
	}
}