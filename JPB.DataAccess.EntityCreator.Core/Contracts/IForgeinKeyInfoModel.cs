namespace JPB.DataAccess.EntityCreator.MsSql
{
	public interface IForgeinKeyInfoModel
	{
		string TableName { get; set; }
		string SourceColumn { get; set; }
		string TargetColumn { get; set; }
	}
}