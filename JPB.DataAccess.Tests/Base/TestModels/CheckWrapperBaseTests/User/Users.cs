#region

using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[AutoGenerateCtor(CtorGeneratorMode = CtorGeneratorMode.FactoryMethod)]
	public class Users
	{
		[PrimaryKey]
		public long User_ID { get; set; }

		public string UserName { get; set; }
	}
}