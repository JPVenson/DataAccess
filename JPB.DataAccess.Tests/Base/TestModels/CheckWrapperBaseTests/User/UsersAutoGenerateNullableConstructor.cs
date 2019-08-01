#region

using JPB.DataAccess.Framework.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User
{
	[AutoGenerateCtor]
	[ForModel(UsersMeta.TableName)]
	public class UsersAutoGenerateNullableConstructor
	{
		[PrimaryKey]
		public long? User_ID { get; set; }

		public string UserName { get; set; }
	}
}