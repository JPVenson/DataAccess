using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.Helper;
using JPB.DataAccess.Framework.ModelsAnotations;
using JPB.DataAccess.Framework.QueryFactory;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[ForModel("Users")]
	public class TestProcBParamsDirect
	{
		public long UserID { get; set; }

		public string UserName { get; set; }

		[SelectFactoryMethod]
		public static IQueryFactoryResult ExecuteSp(int number)
		{
			return new QueryFactoryResult("EXEC TestProcB @nr", new QueryParameter("nr", number));
		}
	}
}