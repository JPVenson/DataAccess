#region

using JPB.DataAccess.Framework.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User
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