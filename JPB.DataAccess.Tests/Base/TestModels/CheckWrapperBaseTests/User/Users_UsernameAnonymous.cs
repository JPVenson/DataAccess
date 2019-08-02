using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User
{
	[AutoGenerateCtor(CtorGeneratorMode = CtorGeneratorMode.FactoryMethod)]
	[ForModel(UsersMeta.TableName)]
	public class Users_UsernameAnonymous
	{
		[PrimaryKey]
		public long User_ID { get; set; }

		[AnonymousObjectGeneration]
		public string UserName { get; set; }
	}
}