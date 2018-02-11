#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;

#endregion

namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
	[Parallelizable(ParallelScope.Fixtures | ParallelScope.Self | ParallelScope.Children)]
	public class CheckWrapperBaseTests : BaseTest
	{
		public CheckWrapperBaseTests(DbAccessType type) : base(type)
		{
		}

		[Test]
		public void AutoGenFactoryTestNullableSimple()
		{
			DbAccess.Insert(new UsersAutoGenerateNullableConstructor());
			var elements = DbAccess.Select<UsersAutoGenerateNullableConstructor>();
			Assert.IsNotNull(elements);
			Assert.IsNotEmpty(elements);
		}

		[Test]
		public void AutoGenFactoryTestSimple()
		{
			DbAccess.Insert(new UsersAutoGenerateConstructor());
			var elements = DbAccess.Select<UsersAutoGenerateConstructor>();
			Assert.IsNotNull(elements);
			Assert.IsNotEmpty(elements);
		}

		[Test]
		public void AutoGenFactoryTestXmlMulti()
		{
			if (DbAccess.DbAccessType != DbAccessType.MsSql)
			{
				return;
			}

			DbAccess.Insert(new UsersAutoGenerateConstructorWithMultiXml());
			DbAccess.Insert(new UsersAutoGenerateConstructorWithMultiXml());
			DbAccess.Insert(new UsersAutoGenerateConstructorWithMultiXml());

			var elements = DbAccess.Query()
			                       .QueryText("SELECT")
			                       .QueryText("res." + UsersMeta.PrimaryKeyName)
			                       .QueryText(",res." + UsersMeta.ContentName)
			                       .QueryText(",")
			                       .InBracket(
			                       s =>
				                       s.Select.Table<UsersAutoGenerateConstructorWithMultiXml>()
				                        .ForXml(typeof(UsersAutoGenerateConstructorWithMultiXml)))
			                       .QueryText("AS Subs")
			                       .QueryText("FROM")
			                       .QueryText(UsersMeta.TableName)
			                       .QueryText("AS res")
			                       .ForResult<UsersAutoGenerateConstructorWithMultiXml>();

			var result = elements.ToArray();

			Assert.IsNotNull(result);
			Assert.IsNotEmpty(result);
		}

		[Test]
		public void AutoGenFactoryTestXmlSingle()
		{
			if (DbAccess.DbAccessType != DbAccessType.MsSql)
			{
				return;
			}

			DbAccess.Insert(new UsersAutoGenerateConstructorWithSingleXml());

			var query = DbAccess.Query()
			                    .QueryText("SELECT")
			                    .QueryText("res." + UsersMeta.PrimaryKeyName)
			                    .QueryText(",res." + UsersMeta.ContentName)
			                    .QueryText(",")
			                    .InBracket(s =>
				                    s.Select.Table<UsersAutoGenerateConstructorWithSingleXml>()
				                     .ForXml(typeof(UsersAutoGenerateConstructorWithSingleXml)))
			                    .QueryText("AS Sub")
			                    .QueryText("FROM")
			                    .QueryText(UsersMeta.TableName)
			                    .QueryText("AS res");
			var elements =
					query.ForResult<UsersAutoGenerateConstructorWithSingleXml>();

			var result = elements.ToArray();

			Assert.IsNotNull(result);
			Assert.IsNotEmpty(result);
		}

		[Test]
		public void CheckFactory()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			Assert.That(() => DbAccess.Select<Users_StaticQueryFactoryForSelect>(), Is.Not.Empty);

			var testInsertName = Guid.NewGuid().ToString();
			Users_StaticQueryFactoryForSelect testUser = null;
			Assert.That(
			() =>
				testUser =
						DbAccess.InsertWithSelect(new Users_StaticQueryFactoryForSelect { UserName = testInsertName }),
			Is.Not.Null
			  .And.Property("UserId").Not.EqualTo(0));

			var selTestUser = DbAccess.Select<Users_StaticQueryFactoryForSelect>(testUser.UserId);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.UserId);
		}

		[Test]
		public void CheckFactoryWithArguments()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			Assert.That(() => DbAccess.Select<Users_StaticQueryFactoryForSelect>(), Is.Not.Empty);

			var testInsertName = Guid.NewGuid().ToString();
			Users_StaticQueryFactoryForSelect testUser = null;
			Assert.That(
			() =>
				testUser =
						DbAccess.InsertWithSelect(new Users_StaticQueryFactoryForSelect { UserName = testInsertName }),
			Is.Not.Null
			  .And.Property("UserId").Not.EqualTo(0));

			var selTestUser =
					DbAccess.Select<Users_StaticQueryFactoryForSelectWithArugments>(new object[] { testUser.UserId })
					        .FirstOrDefault();
			Assert.That(selTestUser, Is.Not.Null
			                           .And.Property("UserName").EqualTo(testUser.UserName)
			                           .And.Property("UserId").EqualTo(testUser.UserId));
		}


		[Test]
		public void ExecuteGenericCommand()
		{
			var resultSelect1 = DbAccess.ExecuteGenericCommand("Select 10", null);
			Assert.AreEqual(resultSelect1, -1);

			resultSelect1 = DbAccess.ExecuteGenericCommand("SELECT @test", new { test = 10 });
			Assert.AreEqual(resultSelect1, -1);

			resultSelect1 = DbAccess.ExecuteGenericCommand("SELECT @test",
			new List<QueryParameter> { new QueryParameter("test", 10) });
			Assert.AreEqual(resultSelect1, -1);
		}

		[Test]
		public void GeneratedTest()
		{
			DbAccess.Insert(new GeneratedUsers());
			var elements = DbAccess.Select<GeneratedUsers>();
			Assert.IsNotNull(elements);
			Assert.IsNotEmpty(elements);
		}

		[Test]
		public void MarsTest()
		{
			DataMigrationHelper.AddUsers(100, DbAccess);

			var baseQuery = DbAccess.Query().Select.Table<Users>();
			var queryA = baseQuery.ContainerObject.Compile();
			var queryB = baseQuery.ContainerObject.Compile();
			Assert.IsNotNull(queryA);
			Assert.IsNotNull(queryB);

			var marsCommand = DbAccess.Database.MergeCommands(queryA, queryB, true);
			var returnValue = DbAccess.ExecuteMARS(marsCommand, typeof(Users), typeof(Users));
			Assert.IsNotNull(returnValue);
			Assert.AreNotSame(returnValue.Count, 0);

			var queryAResult = returnValue.ElementAt(0);
			var queryBResult = returnValue.ElementAt(1);
			Assert.AreNotSame(queryAResult.Count, 0);
			Assert.AreEqual(queryAResult.Count, queryBResult.Count);

			var refCall = DbAccess.Select<Users>();
			Assert.AreEqual(refCall.Length, queryAResult.Count);
		}

		[Test]
		public void SyncCollectionTest()
		{
			DataMigrationHelper.AddUsers(100, DbAccess);

			DbCollection<Users_Col> dbCollection = null;
			Assert.That(() => dbCollection = DbAccess.CreateDbCollection<Users_Col>(), Throws.Nothing);
			Assert.That(dbCollection, Is.Not.Empty);
			Assert.That(dbCollection.Count, Is.EqualTo(100));

			Assert.That(() => dbCollection.Add(new Users_Col()), Throws.Nothing);
			Assert.That(dbCollection.Count, Is.EqualTo(101));

			Assert.That(() => dbCollection.SaveChanges(DbAccess), Throws.Nothing);
			Assert.That(() => DbAccess.Select<Users_Col>().Length, Is.EqualTo(101));

			Assert.That(() => dbCollection.Remove(dbCollection.First()), Throws.Nothing);
			Assert.That(dbCollection.Count, Is.EqualTo(100));

			Assert.That(() => dbCollection.SaveChanges(DbAccess), Throws.Nothing);
			Assert.That(() => DbAccess.Select<Users_Col>().Length, Is.EqualTo(100));

			var user25 = dbCollection[25];
			user25.UserName = Guid.NewGuid().ToString();

			Assert.That(() => dbCollection.GetEntryState(user25), Is.EqualTo(CollectionStates.Changed));
			Assert.That(() => dbCollection.SaveChanges(DbAccess), Throws.Nothing);
			Assert.That(() => dbCollection.GetEntryState(user25), Is.EqualTo(CollectionStates.Unchanged));

			Assert.That(() => DbAccess.Select<Users_Col>(user25.User_ID), Is.Not.Null.And
			                                                                .Property("User_ID").EqualTo(user25.User_ID)
			                                                                .And
			                                                                .Property("UserName").EqualTo(user25.UserName));
		}


		[Test]
		public void TransactionTestRollback()
		{
			base.DbAccess.Database.AllowNestedTransactions = Type == DbAccessType.SqLite;

			DataMigrationHelper.AddUsers(250, DbAccess);
			var count =
					DbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();

			DbAccess.Database.RunInTransaction(dd =>
			{
				DbAccess.Delete<Users>();
				dd.TransactionRollback();
			});

			var countAfter =
					DbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();
			Assert.That(count, Is.EqualTo(countAfter));
		}

		[Test]
		public void TransactionTestExceptional()
		{
			base.DbAccess.Database.AllowNestedTransactions = Type == DbAccessType.SqLite;

			DataMigrationHelper.AddUsers(250, DbAccess);
			var count =
					DbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();

			Assert.That(() => DbAccess.Database.RunInTransaction(dd =>
			{
				DbAccess.Delete<Users>();
				throw new Exception();
			}), Throws.Exception);

			var countAfter =
					DbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();
			Assert.That(count, Is.EqualTo(countAfter));
		}

		[Test]
		public async Task TransactionAsyncTest()
		{
			base.DbAccess.Database.AllowNestedTransactions = Type == DbAccessType.SqLite;

			DataMigrationHelper.AddUsers(250, DbAccess);
			var count =
					DbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();

			await DbAccess.Database.RunInTransactionAsync(async dd =>
			{
				DbAccess.Delete<Users>();
				await Task.FromResult("");
				dd.TransactionRollback();
				return "";
			});

			var countAfter =
					DbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();
			Assert.That(count, Is.EqualTo(countAfter));
		}

		[Test]
		public void TransactionAsyncTestExceptional()
		{
			base.DbAccess.Database.AllowNestedTransactions = Type == DbAccessType.SqLite;

			DataMigrationHelper.AddUsers(250, DbAccess);
			var count =
					DbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();
			Assert.That(() =>
			{
				DbAccess.Database.RunInTransactionAsync(async dd =>
				{
					DbAccess.Delete<Users>();
					await Task.FromResult("");
					throw new Exception();
				}).Wait();
			}, Throws.Exception);

			var countAfter =
					DbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();
			Assert.That(count, Is.EqualTo(countAfter));
		}

		[Test]
		public void CallCloseConnectionShouldFail()
		{
			Assert.That(() => DbAccess.Database.CloseConnection(true), Throws.Exception.TypeOf<InvalidOperationException>());
			Assert.That(() => DbAccess.Database.CloseConnection(false), Throws.Nothing);
		}

		[Test]
		public void SharedTransactionCounter()
		{
			var dbOne = new DefaultDatabaseAccess(new ThreadConnectionController());
			dbOne.Attach(new MsSql(DbAccess.Database.ConnectionString));
			var rootAccess = new DbAccessLayer(dbOne, DbAccess.Config);
			Assert.That(rootAccess.Database.ConnectionController.InstanceCounter, Is.EqualTo(0));
			Assert.That(rootAccess.Database.ConnectionController.LockRoot, Is.Not.Null);
			Assert.That(rootAccess.Database.ConnectionController.Transaction, Is.Null);

			var dbTwo = new DefaultDatabaseAccess(new ThreadConnectionController());
			dbTwo.Attach(new MsSql(DbAccess.Database.ConnectionString));
			var nestedAccess = new DbAccessLayer(dbTwo, DbAccess.Config);
			Assert.That(nestedAccess.Database.ConnectionController.InstanceCounter, Is.EqualTo(0));
			Assert.That(nestedAccess.Database.ConnectionController.LockRoot, Is.EqualTo(dbOne.ConnectionController.LockRoot));
			Assert.That(nestedAccess.Database.ConnectionController.Transaction, Is.Null);

			Assert.That(rootAccess.Database, Is.Not.EqualTo(nestedAccess.Database));
			Assert.That(rootAccess.Database.ConnectionController, Is.Not.EqualTo(nestedAccess.Database.ConnectionController));

			DbAccess.ExecuteGenericCommand("ALTER DATABASE " + DbAccess.Database.DatabaseName +
			                               " SET ALLOW_SNAPSHOT_ISOLATION ON");

			rootAccess.Database.RunInTransaction(d =>
			{
				Assert.That(rootAccess.Database.ConnectionController.InstanceCounter, Is.EqualTo(nestedAccess.Database.ConnectionController.InstanceCounter));
				Assert.That(rootAccess.Database.ConnectionController.Transaction, Is.EqualTo(nestedAccess.Database.ConnectionController.Transaction));
				Assert.That(rootAccess.Database.ConnectionController.InstanceCounter, Is.EqualTo(1));
				DataMigrationHelper.AddUsers(10, rootAccess);

				nestedAccess.Database.RunInTransaction((de) =>
				{
					Assert.That(rootAccess.Database.ConnectionController.InstanceCounter, Is.EqualTo(2));
					Assert.That(nestedAccess.Select<Users>().Length, Is.EqualTo(10));
					DataMigrationHelper.AddUsers(10, nestedAccess);
				});

				Assert.That(nestedAccess.Select<Users>().Length, Is.EqualTo(20));
				DbAccess.Database.RunInTransaction((de) =>
				{
					Assert.That(DbAccess.Database.ConnectionController.InstanceCounter, Is.EqualTo(1));
					Assert.That(DbAccess.Select<Users>().Length, Is.EqualTo(0));
				}, IsolationLevel.Snapshot);
			}, IsolationLevel.Snapshot);
			DbAccess.Database.RunInTransaction((de) =>
			{
				Assert.That(DbAccess.Database.ConnectionController.InstanceCounter, Is.EqualTo(1));
				Assert.That(DbAccess.Select<Users>().Length, Is.EqualTo(20));
			}, IsolationLevel.Snapshot);
		}
	}
}