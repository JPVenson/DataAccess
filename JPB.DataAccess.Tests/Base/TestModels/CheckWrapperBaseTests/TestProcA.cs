using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

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
		private long _userID;
		public long UserID
		{
			get
			{
				return this._userID;
			}
			set
			{
				this._userID = value;
			}
		}
		private string _userName;
		public string UserName
		{
			get
			{
				return this._userName;
			}
			set
			{
				this._userName = value;
			}
		}

		[SelectFactoryMethod]
		public static IQueryFactoryResult ExecuteSp(int number)
		{
			return new QueryFactoryResult("EXEC TestProcB @nr", new QueryParameter("nr", number));
		}
	}
}