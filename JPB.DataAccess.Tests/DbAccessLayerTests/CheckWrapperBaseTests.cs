using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryBuilder;
using JPB.DataAccess.Tests.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;

#if SqLite
using System.Data.SQLite;
#endif

#if MSSQL
using System.Data.SqlClient;
#endif


namespace JPB.DataAccess.Tests.DbAccessLayerTests
#if MSSQL
.MsSQL
#endif

#if SqLite
.SqLite
#endif
{
	[TestFixture]
	public class CheckWrapperBaseTests
	{
		private DbAccessLayer expectWrapper;

		[SetUp]
		public void Init()
		{
			expectWrapper = new Manager().GetWrapper();
		}

		[Test]
		public void CheckFactory()
		{
			InsertTest();
			Users_StaticQueryFactoryForSelect[] refSelect = expectWrapper.Select<Users_StaticQueryFactoryForSelect>();
			Assert.IsTrue(refSelect.Length > 0);

			string testInsertName = Guid.NewGuid().ToString();
			Users_StaticQueryFactoryForSelect testUser =
				expectWrapper.InsertWithSelect(new Users_StaticQueryFactoryForSelect { UserName = testInsertName });
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserId, default(long));

			var selTestUser = expectWrapper.Select<Users_StaticQueryFactoryForSelect>(testUser.UserId);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.UserId);
		}

		[Test]
		public void CheckFactoryWithArguments()
		{
			InsertTest();
			Users_StaticQueryFactoryForSelectWithArugments[] refSelect =
				expectWrapper.Select<Users_StaticQueryFactoryForSelectWithArugments>();
			Assert.IsTrue(refSelect.Length > 0);

			string testInsertName = Guid.NewGuid().ToString();
			Users_StaticQueryFactoryForSelectWithArugments testUser =
				expectWrapper.InsertWithSelect(new Users_StaticQueryFactoryForSelectWithArugments { UserName = testInsertName });
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserId, default(long));

			Users_StaticQueryFactoryForSelectWithArugments selTestUser =
				expectWrapper.Select<Users_StaticQueryFactoryForSelectWithArugments>(new object[] { testUser.UserId })
					.FirstOrDefault();
			Assert.IsNotNull(selTestUser);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.UserId);
		}

		[Test]
		public void ConfigLess()
		{
			string insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			var config = new DbConfig();

			DbConfig.Clear();
			DbConfig.ConstructorSettings.CreateDebugCode = true;

			config.SetConfig<ConfigLessUser>(f =>
			{
				f.SetClassAttribute(new ForModelAttribute(UsersMeta.UserTable));
				f.SetPrimaryKey(e => e.PropertyA);
				f.SetForModelKey(e => e.PropertyA, UsersMeta.UserIDCol);
				f.SetForModelKey(e => e.PropertyB, UsersMeta.UserNameCol);
			});

			expectWrapper.Insert(new ConfigLessUser { PropertyB = insGuid });

			ConfigLessUser[] elements = expectWrapper.Select<ConfigLessUser>();
			Assert.AreEqual(elements.Length, 1);

			string selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.UserTable);
			object selectTest = expectWrapper.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
			DbConfig.Clear();
		}

		[Test]
		//#if MSSQL
		//		[ExpectedException(typeof(SqlException))]
		//#endif
		//#if SqLite		
		//		[ExpectedException(typeof(SQLiteException))]
		//#endif
		public void ConfigLessFail()
		{
			DbConfig.Clear();
			string insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			var config = new DbConfig();
			config.SetConfig<ConfigLessUser>(f =>
			{
				f.SetClassAttribute(new ForModelAttribute(UsersMeta.UserTable));
				f.SetPrimaryKey(e => e.PropertyA);
				f.SetForModelKey(e => e.PropertyA, UsersMeta.UserIDCol + "TEST");
				f.SetForModelKey(e => e.PropertyB, UsersMeta.UserNameCol + "TEST");
			});

			var unexpected = typeof(Exception);

#if MSSQL
			unexpected = typeof(SqlException);
#endif
#if SqLite
			unexpected = typeof(SQLiteException);
		
#endif

			Assert.That(() =>
			{
				expectWrapper.Insert(new ConfigLessUser { PropertyB = insGuid });
			}, Throws.Exception.TypeOf(unexpected));




			//string selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.UserTable);
			//object selectTest = expectWrapper.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			//Assert.IsNotNull(selectTest);
			//Assert.AreEqual(selectTest, insGuid);
			//DbConfig.Clear();
		}

		[SecurityCritical]
		[Test]
		public void ConfigLessInplace()
		{
			DbConfig.Clear();
			string insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);
			expectWrapper.Insert(new ConfigLessUserInplaceConfig { PropertyB = insGuid });

			string selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.UserTable);
			object selectTest = expectWrapper.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));
			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);

			ConfigLessUserInplaceConfig[] elements = expectWrapper.Select<ConfigLessUserInplaceConfig>();
			Assert.AreEqual(elements.Length, 1);
			DbConfig.Clear();
		}

		[Test]
		public void ExecuteGenericCommand()
		{
			int resultSelect1 = expectWrapper.ExecuteGenericCommand("Select 10", null);
			Assert.AreEqual(resultSelect1, -1);

			resultSelect1 = expectWrapper.ExecuteGenericCommand("SELECT @test", new { test = 10 });
			Assert.AreEqual(resultSelect1, -1);

			resultSelect1 = expectWrapper.ExecuteGenericCommand("SELECT @test",
				new List<QueryParameter> { new QueryParameter("test", 10) });
			Assert.AreEqual(resultSelect1, -1);
		}

		[Test]
		public void InsertFactoryTest()
		{
			string insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);
			expectWrapper.IsMultiProviderEnvironment = true;
			expectWrapper.Insert(new UsersWithStaticInsert { UserName = insGuid });
			expectWrapper.IsMultiProviderEnvironment = false;
			string selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.UserTable);
			object selectTest = expectWrapper.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
		}

		[Test]
		public void InsertTest()
		{
			string insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			expectWrapper.Insert(new Users { UserName = insGuid });
			string selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.UserTable);
			object selectTest = expectWrapper.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
		}

		[Test]
		public void InsertWithSelect()
		{
			var val = new Users { UserName = "test" };
			Users refSelect = expectWrapper.InsertWithSelect(val);

			Assert.AreEqual(refSelect.UserName, val.UserName);
			Assert.AreNotEqual(refSelect.User_ID, val.User_ID);
		}

		[Test]
		public void InsertWithSelectStringTest()
		{
			string insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			Users expectedUser = expectWrapper.InsertWithSelect(new Users { UserName = insGuid });
			Assert.IsNotNull(expectedUser);
			Assert.AreEqual(expectedUser.UserName, insGuid);
			Assert.AreNotEqual(expectedUser.User_ID, default(long));
		}

		[Test]
		public void InsertWithSelectTest()
		{
			string insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			Users expectedUser = expectWrapper.InsertWithSelect(new Users { UserName = insGuid });
			Assert.IsNotNull(expectedUser);
			Assert.AreEqual(expectedUser.UserName, insGuid);
			Assert.AreNotEqual(expectedUser.User_ID, default(long));
		}

		[Test]
		public void MarsTest()
		{
			RangeInsertTest();

			QueryBuilder.QueryBuilder baseQuery = expectWrapper.Query().Select(typeof(Users));
			IDbCommand queryA = baseQuery.Compile();
			IDbCommand queryB = baseQuery.Compile();
			Assert.IsNotNull(queryA);
			Assert.IsNotNull(queryB);

			IDbCommand marsCommand = expectWrapper.Database.MergeCommands(queryA, queryB, true);
			List<List<object>> returnValue = expectWrapper.ExecuteMARS(marsCommand, typeof(Users), typeof(Users));
			Assert.IsNotNull(returnValue);
			Assert.AreNotSame(returnValue.Count, 0);

			List<object> queryAResult = returnValue.ElementAt(0);
			List<object> queryBResult = returnValue.ElementAt(1);
			Assert.AreNotSame(queryAResult.Count, 0);
			Assert.AreEqual(queryAResult.Count, queryBResult.Count);

			Users[] refCall = expectWrapper.Select<Users>();
			Assert.AreEqual(refCall.Length, queryAResult.Count);
		}

		[Test]
#if SqLite
[Ignore("Stored Procedures are not Allowed in SqLite")]
#endif
		public void ProcedureParamLessTest()
		{
			RangeInsertTest();
			List<Users> expectedUser = expectWrapper.ExecuteProcedure<TestProcAParams, Users>(new TestProcAParams());

			Assert.IsNotNull(expectedUser);
			Assert.AreNotEqual(expectedUser.Count, 0);

			object refSelect =
				expectWrapper.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT (*) FROM {0}", UsersMeta.UserTable)));
			Assert.AreEqual(expectedUser.Count, refSelect);
		}

		//[Test]
		//public void ProcedureParamTest()
		//{
		//	RangeInsertTest();
		//	//const int procParamA = 5;

		//	var expectedUser =
		//		expectWrapper.ExecuteProcedure<TestProcBParams, Users>(new TestProcBParams()
		//		{
		//			Number = 20
		//		});

		//	Assert.IsNotNull(expectedUser);

		//	//var refselect =
		//	//    expectwrapper.database.run(
		//	//        s =>
		//	//            s.getskalar(string.format("select count(*) from {0} us where {1} > us.user_id", usersmeta.usertable,
		//	//                procparama)));
		//	//assert.areequal(expecteduser.count, refselect);
		//}


		[Test]
		public void RangeInsertTest()
		{
			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			int upperCountTestUsers = 100;
			var testUers = new List<Users>();

			string insGuid = Guid.NewGuid().ToString();

			for (int i = 0; i < upperCountTestUsers; i++)
			{
				testUers.Add(new Users { UserName = insGuid });
			}

			expectWrapper.InsertRange(testUers);

			object refSelect =
				expectWrapper.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.UserTable)));
			if (refSelect is long)
				refSelect = Convert.ChangeType(refSelect, typeof(int));

			Assert.AreEqual(testUers.Count, refSelect);
		}

		[Test]
		public void Refresh()
		{
			InsertTest();
			Users singleEntity = expectWrapper
				.Query()
				.Select<Users>()
				.Top(1)
				.ForResult<Users>()
				.Single();
			long id = singleEntity.User_ID;
			Assert.IsNotNull(singleEntity);

			string preName = singleEntity.UserName;
			string postName = Guid.NewGuid().ToString();
			Assert.IsNotNull(preName);

			singleEntity.UserName = postName;
			expectWrapper.Update(singleEntity);
			singleEntity.UserName = null;

			singleEntity = expectWrapper.Refresh(singleEntity);
			var refEntity = expectWrapper.Select<Users>(id);

			Assert.IsNotNull(refEntity);
			Assert.AreEqual(id, refEntity.User_ID);
			Assert.AreEqual(singleEntity.User_ID, refEntity.User_ID);
			Assert.AreEqual(singleEntity.UserName, refEntity.UserName);
		}

		[Test]
		public void RefreshInplace()
		{
			InsertTest();
			Users singleEntity = expectWrapper
				.Query()
				.Select<Users>()
				.Top(1)
				.ForResult<Users>()
				.Single();
			long id = singleEntity.User_ID;
			Assert.IsNotNull(singleEntity);

			string preName = singleEntity.UserName;
			string postName = Guid.NewGuid().ToString();
			Assert.IsNotNull(preName);

			singleEntity.UserName = postName;
			expectWrapper.Update(singleEntity);
			singleEntity.UserName = null;

			expectWrapper.RefreshKeepObject(singleEntity);
			var refEntity = expectWrapper.Select<Users>(id);

			Assert.IsNotNull(refEntity);
			Assert.AreEqual(id, refEntity.User_ID);
			Assert.AreEqual(singleEntity.User_ID, refEntity.User_ID);
			Assert.AreEqual(singleEntity.UserName, refEntity.UserName);
		}

		[Test]
		public void SelectBase()
		{
			InsertTest();
			Users[] refSelect = expectWrapper.Select<Users>();
			Assert.IsTrue(refSelect.Length > 0);

			string testInsertName = Guid.NewGuid().ToString();
			Users testUser = expectWrapper.InsertWithSelect(new Users { UserName = testInsertName });
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.User_ID, default(long));

			var selTestUser = expectWrapper.Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(testUser.User_ID);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.User_ID);
		}

		[Test]
		public void SelectModelsSelect()
		{
			RangeInsertTest();
			Users firstAvaibleUser = expectWrapper.Query().Select<Users>().Top(1).ForResult<Users>().First();

			var refSelect = expectWrapper.Select<Users_PK>(firstAvaibleUser.User_ID);
			Assert.IsNotNull(refSelect);

			var userSelectAlternatingProperty = expectWrapper.Select<Users_PK_IDFM>(firstAvaibleUser.User_ID);
			Assert.IsNotNull(userSelectAlternatingProperty);

			var userSelectStaticSel = expectWrapper.Select<Users_PK_IDFM_CLASSEL>(firstAvaibleUser.User_ID);
			Assert.IsNotNull(userSelectStaticSel);
		}


		[Test]
		public void SelectNative()
		{
			InsertTest();

			Users[] refSelect = expectWrapper.SelectNative<Users>(UsersMeta.SelectStatement);
			Assert.IsTrue(refSelect.Any());

			long anyId = refSelect.FirstOrDefault().User_ID;
			Assert.AreNotEqual(anyId, 0);

			refSelect =
				expectWrapper.SelectNative<Users>(UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA",
					new QueryParameter("paramA", anyId));
			Assert.IsTrue(refSelect.Length > 0);

			refSelect =
				expectWrapper.SelectNative<Users>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA", new { paramA = anyId });
			Assert.IsTrue(refSelect.Length > 0);
		}

		[Test]
		public void SelectPrimitivSelect()
		{
			InsertTest();
			long[] refSelect = expectWrapper.RunPrimetivSelect<long>(UsersMeta.SelectStatement);
			Assert.IsTrue(refSelect.Any());

			long anyId = refSelect.FirstOrDefault();
			Assert.AreNotEqual(anyId, 0);

			refSelect =
				expectWrapper.RunPrimetivSelect<long>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA",
					new QueryParameter("paramA", anyId));
			Assert.IsTrue(refSelect.Length > 0);

			refSelect =
				expectWrapper.RunPrimetivSelect<long>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA", new { paramA = anyId });
			Assert.IsTrue(refSelect.Length > 0);
		}


		[Test]
		public void SyncCollectionTest()
		{
			RangeInsertTest();

			DbCollection<Users_Col> dbCollection = expectWrapper.CreateDbCollection<Users_Col>();
			Assert.AreEqual(dbCollection.Count, 100);

			dbCollection.Add(new Users_Col());
			Assert.AreEqual(dbCollection.Count, 101);

			dbCollection.SaveChanges(expectWrapper);
			Users_Col[] refAfterAdd = expectWrapper.Select<Users_Col>();
			Assert.AreEqual(refAfterAdd.Length, 101);

			dbCollection.Remove(dbCollection.First());
			Assert.AreEqual(dbCollection.Count, 100);

			dbCollection.SaveChanges(expectWrapper);
			refAfterAdd = expectWrapper.Select<Users_Col>();
			Assert.AreEqual(refAfterAdd.Length, 100);

			Users_Col user25 = dbCollection[25];
			user25.UserName = Guid.NewGuid().ToString();

			Assert.AreEqual(dbCollection.GetEntryState(user25), CollectionStates.Changed);
			dbCollection.SaveChanges(expectWrapper);
			Assert.AreEqual(dbCollection.GetEntryState(user25), CollectionStates.Unchanged);

			var elementAfterChange = expectWrapper.Select<Users_Col>(user25.User_ID);

			Assert.AreEqual(elementAfterChange.User_ID, user25.User_ID);
			Assert.AreEqual(elementAfterChange.UserName, user25.UserName);
		}

		[Test]
		public void Update()
		{
			InsertTest();
			QueryBuilder.QueryBuilder query = expectWrapper
				.Query()
				.Select<Users>()
				.Top(1);
			Users singleEntity = query
				.ForResult<Users>()
				.Single();
			Assert.IsNotNull(singleEntity);

			string preName = singleEntity.UserName;
			string postName = Guid.NewGuid().ToString();
			Assert.IsNotNull(preName);

			singleEntity.UserName = postName;
			expectWrapper.Update(singleEntity);

			var refEntity = expectWrapper.Select<Users>(singleEntity.User_ID);
			Assert.IsNotNull(refEntity);
			Assert.AreEqual(singleEntity.User_ID, refEntity.User_ID);
			Assert.AreEqual(singleEntity.UserName, refEntity.UserName);
		}
	}
}