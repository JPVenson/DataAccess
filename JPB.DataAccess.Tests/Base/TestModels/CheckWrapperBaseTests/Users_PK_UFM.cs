#region

using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[ForModel(UsersMeta.TableName)]
	public class Users_PK_UFM
	{
		[PrimaryKey]
		public long User_ID { get; set; }

		[ForModel(UsersMeta.ContentName)]
		public string UserName { get; set; }
	}
}