#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JPB.DataAccess.Helper.LocalDb.Scopes;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests
{
	[Parallelizable(ParallelScope.Fixtures | ParallelScope.Self | ParallelScope.Children)]
	public class InsertTests : DatabaseBaseTest
	{

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertDefaultValues()
		{
			DbAccess.Config
				.Include<Users>()
				.SetConfig<Users>(
					conf => { conf.SetPropertyAttribute(s => s.UserName, new IgnoreReflectionAttribute()); });

			DbAccess.Insert(new Users());
			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.TableName);
			var selectTest = DbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, DBNull.Value);
		}

		[Test]
		[Category("MsSQL")]
		[Category("MySql")]
		[Category("SqLite")]
		public void InsertIdentity()
		{
			DbAccess.Database.RunInTransaction(f =>
			{
				using (new DbIdentityInsertScope())
				{
					DbAccess.Insert(new Users() { UserID = 999999, UserName = "TEST" });
				}
			});

			var nineNineNineNineNineNine = DbAccess.SelectSingle<Users>(999999);
			Assert.That(nineNineNineNineNineNine, Is.Not.Null);

			//check cleanup
			Assert.That(DbIdentityInsertScope.Current, Is.Null);
		}

		[Test]
		[Category("MsSQL")]
		[Category("MySql")]
		[Category("SqLite")]
		public void InsertIdentityOnWrongScope()
		{
			//ThreadConnectionController.UseTransactionClass();
			DbAccess.Database.RunInTransaction(f =>
			{
				Assert.That(() =>
				{
					using (DbReposetoryIdentityInsertScope.CreateOrObtain())
					{
					}
				}, Throws.Exception);
			});
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertFactoryTest()
		{
			var insGuid = Guid.NewGuid().ToString();

			DbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
			DbAccess.IsMultiProviderEnvironment = true;
			DbAccess.Insert(new UsersWithStaticInsert { UserName = insGuid });
			DbAccess.IsMultiProviderEnvironment = false;
			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.TableName);
			var selectTest = DbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertPropertyLessPoco()
		{
			Assert.That(() => DbAccess.Insert(new UsersWithoutProperties()), Throws.Nothing);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertRange10kEachItemTest()
		{
			var insGuid = Guid.NewGuid().ToString();
			var containingList = new List<Users>();

			for (var i = 0; i < 10000; i++)
			{
				containingList.Add(new Users { UserName = Guid.NewGuid().ToString("N") });
			}

			DbAccess.RangerInsertPation = 1;
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			DbAccess.InsertRange(containingList);
			stopWatch.Stop();
			//Assert.That(stopWatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(7)));

			var selectUsernameFromWhere = string.Format("SELECT COUNT(1) FROM {0}", UsersMeta.TableName);
			var selectTest = DbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, 10000);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertRange10kTest()
		{
			var insGuid = Guid.NewGuid().ToString();
			var containingList = new List<Users>();

			for (var i = 0; i < 10000; i++)
			{
				containingList.Add(new Users { UserName = Guid.NewGuid().ToString("N") });
			}

			var stopWatch = new Stopwatch();
			stopWatch.Start();
			DbAccess.InsertRange(containingList);
			stopWatch.Stop();
			//Assert.That(stopWatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(7)));

			var selectUsernameFromWhere = string.Format("SELECT COUNT(1) FROM {0}", UsersMeta.TableName);
			var selectTest = DbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, 10000);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertRangeTest()
		{
			DbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);

			var upperCountTestUsers = 100;
			var testUers = new List<Users>();

			var insGuid = Guid.NewGuid().ToString();

			for (var i = 0; i < upperCountTestUsers; i++)
			{
				testUers.Add(new Users { UserName = insGuid });
			}

			DbAccess.InsertRange(testUers);

			var refSelect =
				DbAccess.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.TableName)));
			if (refSelect is long)
			{
				refSelect = Convert.ChangeType(refSelect, typeof(int));
			}

			Assert.AreEqual(testUers.Count, refSelect);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertTest()
		{
			var insGuid = Guid.NewGuid().ToString();
			DbAccess.Insert(new Users { UserName = insGuid });
			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.TableName);
			var selectTest = DbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertWithSelect()
		{
			var val = new Users { UserName = "test" };
			var refSelect = DbAccess.InsertWithSelect(val);

			Assert.AreEqual(refSelect.UserName, val.UserName);
			Assert.AreNotEqual(refSelect.UserID, val.UserID);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertWithSelectStringTest()
		{
			var insGuid = Guid.NewGuid().ToString();

			DbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);

			var expectedUser = DbAccess.InsertWithSelect(new Users { UserName = insGuid });
			Assert.IsNotNull(expectedUser);
			Assert.AreEqual(expectedUser.UserName, insGuid);
			Assert.AreNotEqual(expectedUser.UserID, default(long));
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertWithSelectTest()
		{
			var insGuid = Guid.NewGuid().ToString();

			DbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);

			var expectedUser = DbAccess.InsertWithSelect(new Users { UserName = insGuid });
			Assert.IsNotNull(expectedUser);
			Assert.AreEqual(expectedUser.UserName, insGuid);
			Assert.AreNotEqual(expectedUser.UserID, default(long));
		}

		/// <inheritdoc />
		public InsertTests(DbAccessType type, bool asyncExecution,bool syncronised) : base(type, asyncExecution, syncronised)
		{
		}
	}
}