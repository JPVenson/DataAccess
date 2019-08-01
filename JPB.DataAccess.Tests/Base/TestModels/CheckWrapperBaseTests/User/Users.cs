#region

#endregion

using JPB.DataAccess.Framework.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User
{
	[AutoGenerateCtor(CtorGeneratorMode = CtorGeneratorMode.FactoryMethod)]
	public class Users
	{
		[PrimaryKey]
		public long User_ID { get; set; }

		public string UserName { get; set; }
	}
}