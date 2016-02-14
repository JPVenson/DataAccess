using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.QueryBuilder;
using JPB.DataAccess.UnitTests.Properties;
using JPB.DataAccess.UnitTests.TestModels;
using JPB.DataAccess.UnitTests.TestModels.CheckWrapperBaseTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JPB.DataAccess.ModelsAnotations;
using System.Data.SqlClient;

namespace JPB.DataAccess.UnitTests
{
	[TestClass]
	public class CheckWrapperBaseTests
	{
		DbAccessLayer expectWrapper;

		[TestInitialize]
		public void Init()
		{
			expectWrapper = new Manager().GetWrapper();
		}

		[TestMethod]
		public void CheckFactory()
		{
			this.InsertTest();
			var refSelect = expectWrapper.Select<Users_StaticQueryFactoryForSelect>();
			Assert.IsTrue(refSelect.Length > 0);

			var testInsertName = Guid.NewGuid().ToString();
			var testUser = expectWrapper.InsertWithSelect(new Users_StaticQueryFactoryForSelect() { UserName = testInsertName });
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserId, default(long));

			var selTestUser = expectWrapper.Select<Users_StaticQueryFactoryForSelect>(testUser.UserId);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.UserId);
		}

		[TestMethod]
		public void CheckFactoryWithArguments()
		{
			this.InsertTest();
			var refSelect = expectWrapper.Select<Users_StaticQueryFactoryForSelectWithArugments>();
			Assert.IsTrue(refSelect.Length > 0);

			var testInsertName = Guid.NewGuid().ToString();
			var testUser = expectWrapper.InsertWithSelect(new Users_StaticQueryFactoryForSelectWithArugments() { UserName = testInsertName });
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserId, default(long));

			var selTestUser = expectWrapper.Select<Users_StaticQueryFactoryForSelectWithArugments>(new object[] { testUser.UserId }).FirstOrDefault();
			Assert.IsNotNull(selTestUser);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.UserId);
		}

		[TestMethod]
		public void Update()
		{
			InsertTest();
			var query = expectWrapper
				.Query()
				.Select<Users>()
				.Top(1);
			var singleEntity = query
				.ForResult<Users>()
				.Single();
			Assert.IsNotNull(singleEntity);

			var preName = singleEntity.UserName;
			var postName = Guid.NewGuid().ToString();
			Assert.IsNotNull(preName);

			singleEntity.UserName = postName;
			expectWrapper.Update(singleEntity);

			var refEntity = expectWrapper.Select<Users>(singleEntity.User_ID);
			Assert.IsNotNull(refEntity);
			Assert.AreEqual(singleEntity.User_ID, refEntity.User_ID);
			Assert.AreEqual(singleEntity.UserName, refEntity.UserName);
		}

		[TestMethod]
		public void Refresh()
		{
			InsertTest();
			var singleEntity = expectWrapper
				.Query()
				.Select<Users>()
				.Top(1)
				.ForResult<Users>()
				.Single();
			var id = singleEntity.User_ID;
			Assert.IsNotNull(singleEntity);

			var preName = singleEntity.UserName;
			var postName = Guid.NewGuid().ToString();
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

		[TestMethod]
		public void RefreshInplace()
		{
			InsertTest();
			var singleEntity = expectWrapper
				.Query()
				.Select<Users>()
				.Top(1)
				.ForResult<Users>()
				.Single();
			var id = singleEntity.User_ID;
			Assert.IsNotNull(singleEntity);

			var preName = singleEntity.UserName;
			var postName = Guid.NewGuid().ToString();
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

		[TestMethod]
		public void ExecuteGenericCommand()
		{
			var resultSelect1 = expectWrapper.ExecuteGenericCommand("Select 10", null);
			Assert.AreEqual(resultSelect1, -1);

			resultSelect1 = expectWrapper.ExecuteGenericCommand("SELECT @test", new { test = 10 });
			Assert.AreEqual(resultSelect1, -1);

			resultSelect1 = expectWrapper.ExecuteGenericCommand("SELECT @test", new List<QueryParameter>() { new QueryParameter("test", 10) });
			Assert.AreEqual(resultSelect1, -1);
		}

		[TestMethod]
		public void InsertTest()
		{
			var insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			expectWrapper.Insert(new Users() { UserName = insGuid });
			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.UserTable);
			var selectTest = expectWrapper.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
		}

		[TestMethod]
		public void ConfigLess()
		{
			var insGuid = Guid.NewGuid().ToString();

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

			expectWrapper.Insert(new ConfigLessUser() { PropertyB = insGuid });

			var elements = expectWrapper.Select<ConfigLessUser>();
			Assert.AreEqual(elements.Length, 1);

			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.UserTable);
			var selectTest = expectWrapper.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
			DbConfig.Clear();
		}

		[SecurityCritical]
		[TestMethod]
		public void ConfigLessInplace()
		{
			DbConfig.Clear();
			var insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);
			expectWrapper.Insert(new ConfigLessUserInplaceConfig() { PropertyB = insGuid });

			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.UserTable);
			var selectTest = expectWrapper.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));
			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);

			var elements = expectWrapper.Select<ConfigLessUserInplaceConfig>();
			Assert.AreEqual(elements.Length, 1);
			DbConfig.Clear();
		}

		[TestMethod]
		[ExpectedException(typeof(SqlException))]
		public void ConfigLessFail()
		{
			DbConfig.Clear();
			var insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			var config = new DbConfig();
			config.SetConfig<ConfigLessUser>(f =>
			{
				f.SetClassAttribute(new ForModelAttribute(UsersMeta.UserTable));
				f.SetPrimaryKey(e => e.PropertyA);
				f.SetForModelKey(e => e.PropertyA, UsersMeta.UserIDCol + "TEST");
				f.SetForModelKey(e => e.PropertyB, UsersMeta.UserNameCol + "TEST");
			});

			expectWrapper.Insert(new ConfigLessUser() { PropertyB = insGuid });
			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.UserTable);
			var selectTest = expectWrapper.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
			DbConfig.Clear();
		}

		[TestMethod]
		public void InsertFactoryTest()
		{
			var insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			expectWrapper.Insert(new UsersWithStaticInsert() { UserName = insGuid });
			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.UserTable);
			var selectTest = expectWrapper.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
		}

		[TestMethod]
		public void InsertWithSelectTest()
		{
			var insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			var expectedUser = expectWrapper.InsertWithSelect(new Users() { UserName = insGuid });
			Assert.IsNotNull(expectedUser);
			Assert.AreEqual(expectedUser.UserName, insGuid);
			Assert.AreNotEqual(expectedUser.User_ID, default(long));
		}

		[TestMethod]
		public void InsertWithSelectStringTest()
		{
			var insGuid = Guid.NewGuid().ToString();

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			var expectedUser = expectWrapper.InsertWithSelect(new Users() { UserName = insGuid });
			Assert.IsNotNull(expectedUser);
			Assert.AreEqual(expectedUser.UserName, insGuid);
			Assert.AreNotEqual(expectedUser.User_ID, default(long));
		}

		[TestMethod]
		public void ProcedureParamLessTest()
		{
			RangeInsertTest();
			var expectedUser = expectWrapper.ExecuteProcedure<TestProcAParams, Users>(new TestProcAParams());

			Assert.IsNotNull(expectedUser);
			Assert.AreNotEqual(expectedUser.Count, 0);

			var refSelect = expectWrapper.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT (*) FROM {0}", UsersMeta.UserTable)));
			Assert.AreEqual(expectedUser.Count, refSelect);
		}

		//[TestMethod]
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


		[TestMethod]
		public void RangeInsertTest()
		{
			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			var upperCountTestUsers = 100;
			var testUers = new List<Users>();

			var insGuid = Guid.NewGuid().ToString();

			for (int i = 0; i < upperCountTestUsers; i++)
			{
				testUers.Add(new Users() { UserName = insGuid });
			}

			expectWrapper.InsertRange(testUers);

			var refSelect = expectWrapper.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.UserTable)));
			Assert.AreEqual(testUers.Count, refSelect);
		}

		[TestMethod]
		public void InsertWithSelect()
		{
			var val = new Users { UserName = "test" };
			var refSelect = expectWrapper.InsertWithSelect(val);

			Assert.AreEqual(refSelect.UserName, val.UserName);
			Assert.AreNotEqual(refSelect.User_ID, val.User_ID);
		}

		[TestMethod]
		public void SelectBase()
		{
			this.InsertTest();
			var refSelect = expectWrapper.Select<Users>();
			Assert.IsTrue(refSelect.Length > 0);

			var testInsertName = Guid.NewGuid().ToString();
			var testUser = expectWrapper.InsertWithSelect(new Users() { UserName = testInsertName });
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.User_ID, default(long));

			var selTestUser = expectWrapper.Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(testUser.User_ID);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.User_ID);
		}


		[TestMethod]
		public void SelectNative()
		{
			InsertTest();

			var refSelect = expectWrapper.SelectNative<Users>(UsersMeta.SelectStatement);
			Assert.IsTrue(refSelect.Any());

			var anyId = refSelect.FirstOrDefault().User_ID;
			Assert.AreNotEqual(anyId, 0);

			refSelect =
				expectWrapper.SelectNative<Users>(UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA", new QueryParameter("paramA", anyId));
			Assert.IsTrue(refSelect.Length > 0);

			refSelect =
				expectWrapper.SelectNative<Users>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA", new { paramA = anyId });
			Assert.IsTrue(refSelect.Length > 0);
		}

		[TestMethod]
		public void SelectPrimitivSelect()
		{
			InsertTest();
			var refSelect = expectWrapper.RunPrimetivSelect<long>(UsersMeta.SelectStatement);
			Assert.IsTrue(refSelect.Any());

			var anyId = refSelect.FirstOrDefault();
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


		[TestMethod]
		public void SelectModelsSelect()
		{
			RangeInsertTest();
			var firstAvaibleUser = expectWrapper.Query().Select<Users>().Top(1).ForResult<Users>().First();

			var refSelect = expectWrapper.Select<Users_PK>(firstAvaibleUser.User_ID);
			Assert.IsNotNull(refSelect);

			var userSelectAlternatingProperty = expectWrapper.Select<Users_PK_IDFM>(firstAvaibleUser.User_ID);
			Assert.IsNotNull(userSelectAlternatingProperty);

			var userSelectStaticSel = expectWrapper.Select<Users_PK_IDFM_CLASSEL>(firstAvaibleUser.User_ID);
			Assert.IsNotNull(userSelectStaticSel);
		}

		[TestMethod]
		public void MarsTest()
		{
			RangeInsertTest();

			var baseQuery = expectWrapper.Query().Select(typeof(Users));
			var queryA = baseQuery.Compile();
			var queryB = baseQuery.Compile();
			Assert.IsNotNull(queryA);
			Assert.IsNotNull(queryB);

			var marsCommand = expectWrapper.Database.MergeCommands(queryA, queryB, true);
			var returnValue = expectWrapper.ExecuteMARS(marsCommand, typeof(Users), typeof(Users));
			Assert.IsNotNull(returnValue);
			Assert.AreNotSame(returnValue.Count, 0);

			var queryAResult = returnValue.ElementAt(0);
			var queryBResult = returnValue.ElementAt(1);
			Assert.AreNotSame(queryAResult.Count, 0);
			Assert.AreEqual(queryAResult.Count, queryBResult.Count);

			var refCall = expectWrapper.Select<Users>();
			Assert.AreEqual(refCall.Length, queryAResult.Count);
		}

		[TestMethod]
		public void SyncCollectionTest()
		{
			RangeInsertTest();

			var dbCollection = expectWrapper.CreateDbCollection<Users_Col>();
			Assert.AreEqual(dbCollection.Count, 100);

			dbCollection.Add(new Users_Col());
			Assert.AreEqual(dbCollection.Count, 101);

			dbCollection.SaveChanges(expectWrapper);
			var refAfterAdd = expectWrapper.Select<Users_Col>();
			Assert.AreEqual(refAfterAdd.Length, 101);

			dbCollection.Remove(dbCollection.First());
			Assert.AreEqual(dbCollection.Count, 100);

			dbCollection.SaveChanges(expectWrapper);
			refAfterAdd = expectWrapper.Select<Users_Col>();
			Assert.AreEqual(refAfterAdd.Length, 100);

			var user25 = dbCollection[25];
			user25.UserName = Guid.NewGuid().ToString();

			Assert.AreEqual(dbCollection.GetEntryState(user25), DbCollection.CollectionStates.Changed);
			dbCollection.SaveChanges(expectWrapper);
			Assert.AreEqual(dbCollection.GetEntryState(user25), DbCollection.CollectionStates.Unchanged);

			var elementAfterChange = expectWrapper.Select<Users_Col>(user25.User_ID);

			Assert.AreEqual(elementAfterChange.User_ID, user25.User_ID);
			Assert.AreEqual(elementAfterChange.UserName, user25.UserName);
		}
	}
}
