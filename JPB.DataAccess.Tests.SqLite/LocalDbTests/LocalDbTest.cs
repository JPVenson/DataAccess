using System;
using System.Linq;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Helper.LocalDb.Scopes;
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
	[TestFixture(false)]
	[TestFixture(true)]
	public class LocalDbTest
	{
		private readonly bool _useObjectCopy;
		private LocalDbRepository<Users> _users;

		public LocalDbTest(bool useObjectCopy)
		{
			_useObjectCopy = useObjectCopy;
		}

		[SetUp]
		public void TestInit()
		{
			using (new DatabaseScope())
			{
				_users = new LocalDbRepository<Users>(new DbConfig(), _useObjectCopy, null);
			}

			Assert.IsTrue(_users.ReposetoryCreated);
		}

		[Test]
		public void SimpleParallelAccess()
		{
			Assert.That(() =>
			{
				Parallel.For(0, 999, (d) =>
				{
					_users.Add(new Users());
				});
			}, Throws.Nothing);
		}

		[Test]
		public void AdvParallelAccess()
		{
			Assert.That(() =>
			{
				Parallel.For(0, 999, (d) =>
				{
					foreach (var userse in _users)
					{

					}
					var firstOrDefault = _users.FirstOrDefault();
					if (d % 2 == 0)
					{
						_users.Add(new Users());
					}
					else
					{
						_users.Remove(firstOrDefault);
					}
					foreach (var userse in _users)
					{

					}
				});
			}, Throws.Nothing);
		}

		[Test]
		public void Add()
		{
			var user = new Users();
			_users.Add(user);
			Assert.That(user, Is.Not.Null);
			Assert.That(user.UserID, Is.EqualTo(1));
		}

		[Test]
		public void Contains()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			if (_useObjectCopy)
				Assert.That(_users.Contains(user), Is.True);
			else
				Assert.That(_users.Contains(user), Is.False);
		}

		[Test]
		public void ContainsId()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			Assert.That(_users.Contains(user.UserID), Is.True);
		}

		[Test]
		public void Count()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			Assert.That(_users.Count, Is.EqualTo(1));
		}

		[Test]
		public void Remove()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			Assert.That(_users.Remove(user), Is.True);
			Assert.That(_users.Count, Is.EqualTo(0));
		}

		[Test]
		public void Enumerate()
		{
			var user = new Users();
			_users.Add(user);
			if (_useObjectCopy)
			{
				_users.Add(user);
				Assert.That(_users.Count, Is.EqualTo(1));
				Assert.That(_users.ToArray(), Is.Not.Null.And.Property("Length").EqualTo(1));
			}
			else
			{
				Assert.That(() => _users.Add(user), Throws.Exception.TypeOf<InvalidOperationException>());
				Assert.That(_users.Count, Is.EqualTo(1));
				Assert.That(_users.ToArray(), Is.Not.Null.And.Property("Length").EqualTo(1));
			}
			
			_users.Add(new Users());
			_users.Add(new Users());
			_users.Add(new Users());
			_users.Add(new Users());
			Assert.That(_users.Count, Is.EqualTo(5));
			Assert.That(_users.ToArray(), Is.Not.Null.And.Property("Length").EqualTo(5));
		}
	}
}