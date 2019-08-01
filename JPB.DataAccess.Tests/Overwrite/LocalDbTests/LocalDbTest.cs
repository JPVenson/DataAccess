#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JPB.DataAccess.Framework.DbInfoConfig;
using JPB.DataAccess.Framework.Helper.LocalDb;
using JPB.DataAccess.Framework.Helper.LocalDb.Scopes;
using JPB.DataAccess.Tests.Base;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.LocalDbTests
{
	[TestFixture(false)]
	[TestFixture(true)]
	public class LocalDbTest
	{
		[SetUp]
		public void TestInit()
		{
			using (new DatabaseScope())
			{
				_users = new LocalDbRepository<Users>(new DbConfig(true), _useObjectCopy, null);
			}

			Assert.IsTrue(_users.ReposetoryCreated);
		}

		private readonly bool _useObjectCopy;
		private LocalDbRepository<Users> _users;

		public LocalDbTest(bool useObjectCopy)
		{
			_useObjectCopy = useObjectCopy;
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
		public void AdvParallelAccess()
		{
			Assert.That(() =>
			{
				Parallel.For(0, 999, d =>
				{
					var enumerator = _users.GetEnumerator();
					enumerator.MoveNext();
					var i = 0;
					foreach (var userse in _users)
					{
						Assert.That(userse, Is.Not.Null);
						i += 1;
					}
					var firstOrDefault = _users.FirstOrDefault();
					if (d % 2 == 0)
					{
						_users.Add(new Users());
					}
					else
					{
						if (firstOrDefault != null)
						{
							_users.Remove(firstOrDefault);
						}
					}
					foreach (var userse in _users)
					{
						Assert.That(userse, Is.Not.Null);
						i -= 1;
					}
				});
			}, Throws.Nothing);
		}

		[Test]
		public void Contains()
		{
			var user = new Users();
			_users.Add(user);

			//Add TestMethod
			if (_useObjectCopy)
			{
				Assert.That(_users.Contains(user), Is.True);
			}
			else
			{
				Assert.That(_users.Contains(user), Is.False);
			}
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

			var arr = new object[_users.Count];
			Assert.That(() => _users.CopyTo(arr, 0), Throws.Nothing);

			var userArr = new Users[_users.Count];
			Assert.That(() => _users.CopyTo(userArr, 0), Throws.Nothing);
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
		[TestCase(999)]
		[TestCase(9999)]
		[TestCase(99999)]
		[TestCase(999999)]
		[Explicit("Long running Performance tests")]
		public void SimpleParallelAccess(int limit)
		{
			Assert.That(() => { Parallel.For(0, limit, new ParallelOptions() { MaxDegreeOfParallelism = 6 }, d => { _users.Add(new Users()); }); }, Throws.Nothing);
			Assert.That(_users.Count, Is.EqualTo(limit));
			var idCounter = 1;
			foreach (var user in _users)
			{
				Assert.That(user, Is.Not.Null.And.Property(nameof(Users.UserID)).EqualTo(idCounter));
				idCounter++;
			}
		}

		[TestCase(2, new[] { 999, 9999, 99999, 999999 })]
		[Explicit("Long running Performance tests")]
		public void ParallelAccessSpeedEval(int iterateCount, int[] limits)
		{
			var results = new List<Tuple<int, long>>();
			foreach (var limit in limits)
			{
				var iterationResults = new List<Tuple<int, long>>();
				for (int i = 0; i < iterateCount; i++)
				{
					var sp = new Stopwatch();
					sp.Start();
					SimpleParallelAccess(limit);
					sp.Stop();
					iterationResults.Add(new Tuple<int, long>(limit, sp.ElapsedMilliseconds));
					TestInit();
				}
				results.Add(iterationResults.Aggregate((e, f) => new Tuple<int, long>(e.Item1 + f.Item1, e.Item2 + f.Item2)));
			}

			foreach (var result in results)
			{
				TestContext.Out.WriteLine($"Enumerating {iterateCount} times over {result.Item1} users took {result.Item2} ms that is {result.Item2 / (float)result.Item1} ms per 1 user");
			}

			var allITems = results.Aggregate((e, f) => new Tuple<int, long>(e.Item1 + f.Item1, e.Item2 + f.Item2));
			TestContext.Out.WriteLine($"Enumerating All {allITems.Item1} users took {allITems.Item2} ms that is {allITems.Item2 / (float)allITems.Item1} ms per 1 user");
		}
	}
}