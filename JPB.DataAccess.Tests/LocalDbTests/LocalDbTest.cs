using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Tests.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.LocalDbTests
#if MsSql
.MsSQL
#endif

#if SqLite
.SqLite
#endif
{
	[TestFixture]
	public class LocalDbTest
	{
		private LocalDbReposetory<Users> _users;

		[SetUp]
		public void TestInit()
		{
			using (new DatabaseScope())
			{
				_users = new LocalDbReposetory<Users>(new DbConfig());
			}

			Assert.IsTrue(_users.ReposetoryCreated);
		}

		[Test]
		public void Add()
		{
			var user = new Users();
			_users.Add(user);
			Assert.IsNotNull(user);
			Assert.AreNotEqual(user.UserID, 0);
			Assert.AreEqual(user.UserID, 1);
		}

		[Test]
		public void Contains()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			Assert.IsTrue(_users.Contains(user));
		}

		[Test]
		public void ContainsId()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			Assert.IsTrue(_users.Contains(user.UserID));
		}

		[Test]
		public void Count()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			Assert.AreEqual(_users.Count, 1);
		}

		[Test]
		public void Remove()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			Assert.IsTrue(_users.Remove(user));
			Assert.AreEqual(_users.Count, 0);
		}


		[Test]
		public void Enumerate()
		{
			var user = new Users();
			_users.Add(user);
			_users.Add(user);
			Assert.That(_users.Count, Is.EqualTo(1));
			Assert.That(_users.ToArray(), Is.Not.Null.And.Property("Length").EqualTo(1));
			_users.Add(new Users());
			_users.Add(new Users());
			_users.Add(new Users());
			_users.Add(new Users());
			Assert.That(_users.Count, Is.EqualTo(5));
			Assert.That(_users.ToArray(), Is.Not.Null.And.Property("Length").EqualTo(5));
		}
	}
}