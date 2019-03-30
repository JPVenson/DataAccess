#region

using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[AutoGenerateCtor]
	[ForModel(UsersMeta.TableName)]
	public class UsersAutoGenerateConstructor
	{
		[PrimaryKey]
		public long User_ID { get; set; }

		public string UserName { get; set; }
	}
}