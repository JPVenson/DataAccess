#region

using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[AutoGenerateCtor(CtorGeneratorMode = CtorGeneratorMode.FactoryMethod)]
	[ForModel(UsersMeta.TableName)]
	public class UsersWithoutProperties
	{
		[PrimaryKey]
		public long User_ID { get; set; }
	}
}