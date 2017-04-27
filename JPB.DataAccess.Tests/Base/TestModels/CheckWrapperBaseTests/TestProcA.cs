#region

using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[ForModel("TestProcA")]
	public class TestProcAParams
	{
	}

	[ForModel("TestProcB")]
	public class TestProcBParams
	{
		[ForModel("number")]
		public int Number { get; set; }
	}

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