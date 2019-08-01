#region

using System;
using System.Linq;
using System.Transactions;
using JPB.DataAccess.Framework.DbInfoConfig;
using JPB.DataAccess.Framework.Helper.LocalDb;
using JPB.DataAccess.Framework.Helper.LocalDb.Scopes;
using JPB.DataAccess.Framework.Helper.LocalDb.Trigger;
using JPB.DataAccess.Tests.Base;
using NUnit.Framework;
using TransactionScope = JPB.DataAccess.Framework.Helper.LocalDb.TransactionScope;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.LocalDbTests

{
	[TestFixture]
	public class LocalDbTriggerTestInReplication
	{
		public class DbScope : IDisposable
		{
			public DatabaseScope database;
			public LocalDbRepository<Users> users;

			public DbScope()
			{
				database = new DatabaseScope();
				users = new LocalDbRepository<Users>(new DbConfig(true));
			}

			public void Dispose()
			{
				database.Dispose();
			}
		}

		private DbScope MockRepro()
		{
			return new DbScope();
		}

		[Test]
		public void InsertTriggerWithCancelAfterOrder()
		{
			var orderFlag = false;
			using (var repro = MockRepro())
			{
				repro.users.Triggers.WithReplication.For.Insert += (sender, token) =>
				{
					Assert.That(orderFlag, Is.False);
					orderFlag = true;
				};
				repro.users.Triggers.WithReplication.After.Insert += (sender, token) => { token.Cancel("AFTER"); };
				repro.users.Triggers.NotForReplication.For.Insert += (sender, token) => { Assert.Fail(); };
				repro.users.Triggers.NotForReplication.After.Insert += (sender, token) => { Assert.Fail(); };

				repro.database.SetupDone += (sender, args) =>
				{
					Assert.That(orderFlag, Is.False);
					Assert.That(() => { repro.users.Add(new Users()); },
						Throws.Exception.InstanceOf<ITriggerException>().With.Property("Reason").EqualTo("AFTER"));
					Assert.That(orderFlag, Is.True);
					Assert.That(repro.users.Count, Is.EqualTo(0));
				};
			}
		}

		[Test]
		public void DeleteTriggerWithCancelAfterOrder()
		{
			DbScope repro;
			var orderFlag = false;
			using (repro = MockRepro())
			{
				repro.users.Triggers.WithReplication.For.Delete += (sender, token) =>
				{
					Assert.That(orderFlag, Is.False);
					orderFlag = true;
				};
				repro.users.Triggers.WithReplication.After.Delete += (sender, token) =>
				{
					token.Cancel("AFTER");
				};
			}
			Assert.That(orderFlag, Is.False);
			repro.users.Add(new Users());
			Assert.That(repro.users.Count, Is.EqualTo(1));
			Assert.That(orderFlag, Is.False);
			Assert.That(() =>
			{
				repro.users.Remove(repro.users.FirstOrDefault());
			}, Throws.Exception.InstanceOf<ITriggerException>().With.Property("Reason").EqualTo("AFTER"));
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.users.Count, Is.EqualTo(1));
		}

		[Test]
		public void InsertTriggerWithCancelForOrder()
		{
			DbScope repro;
			var orderFlag = false;
			using (repro = MockRepro())
			{
				repro.users.Triggers.WithReplication.For.Insert += (sender, token) =>
				{
					Assert.That(orderFlag, Is.False);
					orderFlag = true;
					token.Cancel("FOR");
				};
				repro.users.Triggers.WithReplication.After.Insert += (sender, token) =>
				{
					Assert.Fail("This should not be called");
				};
			}
			Assert.That(orderFlag, Is.False);
			Assert.That(() =>
			{
				repro.users.Add(new Users());
			}, Throws.Exception.InstanceOf<ITriggerException>().With.Property("Reason").EqualTo("FOR"));
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.users.Count, Is.EqualTo(0));
		}

		[Test]
		public void DeleteTriggerWithCancelForOrder()
		{
			DbScope repro;
			var orderFlag = false;
			using (repro = MockRepro())
			{
				repro.users.Triggers.WithReplication.For.Delete += (sender, token) =>
				{
					Assert.That(orderFlag, Is.False);
					orderFlag = true;
					token.Cancel("FOR");
				};
				repro.users.Triggers.WithReplication.After.Delete += (sender, token) =>
				{
					Assert.Fail("This should not be called");
				};
			}
			Assert.That(orderFlag, Is.False);
			repro.users.Add(new Users());
			Assert.That(repro.users.Count, Is.EqualTo(1));
			Assert.That(orderFlag, Is.False);
			Assert.That(() =>
			{
				repro.users.Remove(repro.users.FirstOrDefault());
			}, Throws.Exception.InstanceOf<ITriggerException>().With.Property("Reason").EqualTo("FOR"));
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.users.Count, Is.EqualTo(1));
		}

		[Test]
		public void InsertTriggerOrder()
		{
			var orderFlag = false;
			DbScope repro;
			using (repro = MockRepro())
			{
				repro.users.Triggers.WithReplication.For.Insert += (sender, token) =>
				{
					Assert.That(orderFlag, Is.False);
					orderFlag = true;
				};
				repro.users.Triggers.WithReplication.After.Insert += (sender, token) =>
				{
					Assert.That(orderFlag, Is.True);
				};
			}
			Assert.That(orderFlag, Is.False);
			repro.users.Add(new Users());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.users.Count, Is.EqualTo(1));
		}

		[Test]
		public void DeleteTriggerOrder()
		{
			DbScope repro;
			var orderFlag = false;
			using (repro = MockRepro())
			{
				repro.users.Triggers.WithReplication.For.Delete += (sender, token) =>
				{
					Assert.That(orderFlag, Is.False);
					orderFlag = true;
				};
				repro.users.Triggers.WithReplication.After.Delete += (sender, token) =>
				{
					Assert.That(orderFlag, Is.True);
				};
			}
			Assert.That(orderFlag, Is.False);
			repro.users.Add(new Users());
			Assert.That(repro.users.Count, Is.EqualTo(1));
			Assert.That(orderFlag, Is.False);
			repro.users.Remove(repro.users.FirstOrDefault());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.users.Count, Is.EqualTo(0));
		}

		[Test]
		public void DeleteIOTriggerOrder()
		{
			DbScope repro;
			var orderFlag = false;
			using (repro = MockRepro())
			{
				repro.users.Triggers.WithReplication.For.Delete += (sender, token) =>
				{
					Assert.That(orderFlag, Is.False);
					orderFlag = true;
				};
				repro.users.Triggers.WithReplication.After.Delete += (sender, token) =>
				{
					Assert.That(orderFlag, Is.True);
				};
			}
			Assert.That(orderFlag, Is.False);
			repro.users.Add(new Users());
			Assert.That(repro.users.Count, Is.EqualTo(1));
			Assert.That(orderFlag, Is.False);
			repro.users.Remove(repro.users.FirstOrDefault());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.users.Count, Is.EqualTo(0));
		}

		[Test]
		public void DeleteIORemoveTriggerOrder()
		{
			DbScope repro;
			var orderFlag = false;
			var deleted = false;
			using (repro = MockRepro())
			{
				repro.users.Triggers.WithReplication.For.Delete += (sender, token) =>
				{
					if (!deleted)
						Assert.That(orderFlag, Is.False);
					orderFlag = true;
				};

				repro.users.Triggers.WithReplication.After.Delete += (sender, token) =>
				{
					Assert.That(orderFlag, Is.True);
				};

				repro.users.Triggers.WithReplication.InsteadOf.Delete += (sender, token) =>
				{
					Assert.That(orderFlag, Is.True);
					deleted = true;
					Assert.That(token.Table.ContainsId(token.Item.UserID), Is.True);
					token.Table.Remove(token.Item);
					Assert.That(token.Table.ContainsId(token.Item.UserID), Is.False);
				};
			}
			Assert.That(orderFlag, Is.False);
			repro.users.Add(new Users());
			Assert.That(repro.users.Count, Is.EqualTo(1));
			Assert.That(orderFlag, Is.False);
			repro.users.Remove(repro.users.FirstOrDefault());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.users.Count, Is.EqualTo(0));
		}

		[Test]
		public void InsertIOTriggerOrder()
		{
			var orderFlag = false;
			DbScope repro;
			using (repro = MockRepro())
			{
				repro.users.Triggers.WithReplication.For.Insert += (sender, token) =>
				{
					Assert.That(orderFlag, Is.False);
					orderFlag = true;
				};
				repro.users.Triggers.WithReplication.After.Insert += (sender, token) =>
				{
					Assert.That(orderFlag, Is.True);
				};
				repro.users.Triggers.WithReplication.InsteadOf.Insert += (sender, token) =>
				{
					Assert.That(orderFlag, Is.True);
				};
			}
			Assert.That(orderFlag, Is.False);
			repro.users.Add(new Users());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.users.Count, Is.EqualTo(0));
		}

		[Test]
		public void InsertIOReaddTriggerOrder()
		{
			var orderFlag = false;
			var inserted = false;
			DbScope repro;
			using (repro = MockRepro())
			{
				repro.users.Triggers.WithReplication.For.Insert += (sender, token) =>
				{
					if (!inserted)
						Assert.That(orderFlag, Is.False);
					orderFlag = true;
				};

				repro.users.Triggers.WithReplication.After.Insert += (sender, token) =>
				{
					Assert.That(orderFlag, Is.True);
				};

				repro.users.Triggers.WithReplication.InsteadOf.Insert += (sender, token) =>
				{
					Assert.That(orderFlag, Is.True);
					inserted = true;
					using (var tr = new TransactionScope())
					{
						using (DbReposetoryIdentityInsertScope.CreateOrObtain())
						{
							token.Table.Add(token.Item);
						}
						tr.Complete();
					}
				};
			}

			Assert.That(orderFlag, Is.False);
			repro.users.Add(new Users());
			Assert.That(orderFlag, Is.True);
			Assert.That(repro.users.Count, Is.EqualTo(1));
		}
	}
}