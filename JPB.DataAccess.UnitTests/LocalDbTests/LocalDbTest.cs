using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.UnitTests.TestModels.CheckWrapperBaseTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JPB.DataAccess.UnitTests.LocalDbTests
{
	[TestClass]
	public class LocalDbTest
	{
		public LocalDbTest()
		{

		}

		private LocalDbReposetory<Users> _users;

		[TestInitialize]
		public void TestInit()
		{
			using (new DatabaseScope())
			{
				_users = new LocalDbReposetory<Users>();
			}

			Assert.IsTrue(_users.ReposetoryCreated);
		}

		[TestMethod]
		public void Add()
		{
			var user = new Users();
			_users.Add(user);
			Assert.IsNotNull(user);
			Assert.AreNotEqual(user.User_ID, 0);
			Assert.AreEqual(user.User_ID, 1);
		}

		[TestMethod]
		public void Contains()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			Assert.IsTrue(_users.Contains(user));
		}

		[TestMethod]
		public void ContainsId()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			Assert.IsTrue(_users.Contains(user.User_ID));
		}

		[TestMethod]
		public void Count()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			Assert.AreEqual(_users.Count, 1);
		}

		[TestMethod]
		public void Remove()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			Assert.IsTrue(_users.Remove(user));
			Assert.AreEqual(_users.Count, 0);
		}
	}
}
