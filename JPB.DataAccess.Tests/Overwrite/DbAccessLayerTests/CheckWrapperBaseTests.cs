using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.Query;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Users = JPB.DataAccess.Tests.Base.Users;


namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
	[TestFixture(DbAccessType.MsSql)]
	[TestFixture(DbAccessType.SqLite)]
	//[TestFixture(DbAccessType.MySql)]
	public class CheckWrapperBaseTests
	{
		private readonly DbAccessType _type;

		public CheckWrapperBaseTests(DbAccessType type)
		{
			_type = type;
		}

		private DbAccessLayer _dbAccess;
		private IManager _mgr;

		[SetUp]
		public void Init()
		{
			_mgr = new Manager();
			_dbAccess = _mgr.GetWrapper(_type);
		}

		[TearDown]
		public void TestTearDown()
		{
			// inc. class name
			var fullNameOfTheMethod = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
			// method name only
			var methodName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
			// the state of the test execution
			var state = NUnit.Framework.TestContext.CurrentContext.Result.Outcome == ResultState.Failure; // TestState enum

			if (state)
			{
				_mgr.FlushErrorData();
			}
		}

		[SetUp]
		public void Clear()
		{
			_dbAccess.Config.Dispose();
			_dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
			if (_dbAccess.DbAccessType == DbAccessType.MsSql)
				_dbAccess.ExecuteGenericCommand(string.Format("TRUNCATE TABLE {0} ", UsersMeta.TableName), null);
		}



		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void TransactionTest()
		{
			DataMigrationHelper.AddUsers(250, _dbAccess);
			var count = _dbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();

			_dbAccess.Database.RunInTransaction(dd =>
			{
				_dbAccess.Delete<Users>();
				dd.TransactionRollback();
			});

			var countAfter = _dbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();
			Assert.That(count, Is.EqualTo(countAfter));
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertPropertyLessPoco()
		{
			Assert.That(() => _dbAccess.Insert(new UsersWithoutProperties()), Throws.Nothing);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectPropertyLessPoco()
		{
			InsertPropertyLessPoco();
			Assert.That(() => _dbAccess.Select<UsersWithoutProperties>(), Is.Not.Null.And.Not.Empty);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void CheckFactory()
		{
			InsertTest();
			Assert.That(() => _dbAccess.Select<Users_StaticQueryFactoryForSelect>(), Is.Not.Empty);

			var testInsertName = Guid.NewGuid().ToString();
			Users_StaticQueryFactoryForSelect testUser = null;
			Assert.That(() => testUser = _dbAccess.InsertWithSelect(new Users_StaticQueryFactoryForSelect { UserName = testInsertName }),
				Is.Not.Null
				.And.Property("UserId").Not.EqualTo(0));

			var selTestUser = _dbAccess.Select<Users_StaticQueryFactoryForSelect>(testUser.UserId);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.UserId);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void CheckFactoryWithArguments()
		{
			InsertTest();
			Assert.That(() => _dbAccess.Select<Users_StaticQueryFactoryForSelect>(), Is.Not.Empty);

			var testInsertName = Guid.NewGuid().ToString();
			Users_StaticQueryFactoryForSelect testUser = null;
			Assert.That(() => testUser = _dbAccess.InsertWithSelect(new Users_StaticQueryFactoryForSelect { UserName = testInsertName }),
						Is.Not.Null
						.And.Property("UserId").Not.EqualTo(0));

			var selTestUser =
				_dbAccess.Select<Users_StaticQueryFactoryForSelectWithArugments>(new object[] { testUser.UserId })
					.FirstOrDefault();
			Assert.That(selTestUser, Is.Not.Null
				.And.Property("UserName").EqualTo(testUser.UserName)
				.And.Property("UserId").EqualTo(testUser.UserId));
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void ConfigLess()
		{
			var insGuid = Guid.NewGuid().ToString();

			_dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
			_dbAccess.Config.SetConfig<ConfigLessUser>(f =>
			{
				f.SetClassAttribute(new ForModelAttribute(UsersMeta.TableName));
				f.SetPrimaryKey(e => e.PropertyA);
				f.SetForModelKey(e => e.PropertyA, UsersMeta.PrimaryKeyName);
				f.SetForModelKey(e => e.PropertyB, UsersMeta.ContentName);
			});

			_dbAccess.Insert(new ConfigLessUser { PropertyB = insGuid });

			var elements = _dbAccess.Select<ConfigLessUser>();
			Assert.AreEqual(elements.Length, 1);

			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.TableName);
			var selectTest = _dbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void ConfigLessFail()
		{
			DbConfig.Clear();
			var insGuid = Guid.NewGuid().ToString();

			_dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);

			_dbAccess.Config.SetConfig<ConfigLessUser>(f =>
			{
				f.SetClassAttribute(new ForModelAttribute(UsersMeta.TableName));
				f.SetPrimaryKey(e => e.PropertyA);
				f.SetForModelKey(e => e.PropertyA, UsersMeta.PrimaryKeyName + "TEST");
				f.SetForModelKey(e => e.PropertyB, UsersMeta.ContentName + "TEST");
			});

			//			var unexpected = typeof(Exception);

			//#if MsSql
			//			unexpected = typeof(SqlException);
			//#endif
			//#if SqLite
			//			unexpected = typeof(SQLiteException);
			//#endif

			Assert.That(() =>
			{
				_dbAccess.Insert(new ConfigLessUser { PropertyB = insGuid });
			}, Throws.Exception);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void AutoGenFactoryTestNullableSimple()
		{

			_dbAccess.Insert(new UsersAutoGenerateNullableConstructor());
			var elements = _dbAccess.Select<UsersAutoGenerateNullableConstructor>();
			Assert.IsNotNull(elements);
			Assert.IsNotEmpty(elements);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void GeneratedTest()
		{

			_dbAccess.Insert(new GeneratedUsers());
			var elements = _dbAccess.Select<GeneratedUsers>();
			Assert.IsNotNull(elements);
			Assert.IsNotEmpty(elements);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void AutoGenFactoryTestSimple()
		{

			_dbAccess.Insert(new UsersAutoGenerateConstructor());
			var elements = _dbAccess.Select<UsersAutoGenerateConstructor>();
			Assert.IsNotNull(elements);
			Assert.IsNotEmpty(elements);
		}

		[Test]
		[Category("MsSQL")]
		public void AutoGenFactoryTestXmlSingle()
		{
			if (_dbAccess.DbAccessType != DbAccessType.MsSql)
				return;


			_dbAccess.Insert(new UsersAutoGenerateConstructorWithSingleXml());

			var query = _dbAccess.Query()
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

		[Test()]
		[Category("MsSQL")]
		public void AutoGenFactoryTestXmlMulti()
		{
			if (_dbAccess.DbAccessType != DbAccessType.MsSql)
				return;

			_dbAccess.Insert(new UsersAutoGenerateConstructorWithMultiXml());
			_dbAccess.Insert(new UsersAutoGenerateConstructorWithMultiXml());
			_dbAccess.Insert(new UsersAutoGenerateConstructorWithMultiXml());

			var elements = _dbAccess.Query()
				.QueryText("SELECT")
				.QueryText("res." + UsersMeta.PrimaryKeyName)
				.QueryText(",res." + UsersMeta.ContentName)
				.QueryText(",")
				.InBracket(s => s.Select.Table<UsersAutoGenerateConstructorWithMultiXml>().ForXml(typeof(UsersAutoGenerateConstructorWithMultiXml)))
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
		[Category("MsSQL")]
		[Category("SqLite")]
		public void ConfigLessInplace()
		{
			var insGuid = Guid.NewGuid().ToString();

			_dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
			_dbAccess.Insert(new ConfigLessUserInplaceConfig { PropertyB = insGuid });

			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.TableName);
			var selectTest = _dbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));
			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);

			var elements = _dbAccess.Select<ConfigLessUserInplaceConfig>();
			Assert.AreEqual(elements.Length, 1);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void ExecuteGenericCommand()
		{
			var resultSelect1 = _dbAccess.ExecuteGenericCommand("Select 10", null);
			Assert.AreEqual(resultSelect1, -1);

			resultSelect1 = _dbAccess.ExecuteGenericCommand("SELECT @test", new { test = 10 });
			Assert.AreEqual(resultSelect1, -1);

			resultSelect1 = _dbAccess.ExecuteGenericCommand("SELECT @test",
				new List<QueryParameter> { new QueryParameter("test", 10) });
			Assert.AreEqual(resultSelect1, -1);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertFactoryTest()
		{
			var insGuid = Guid.NewGuid().ToString();

			_dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
			_dbAccess.IsMultiProviderEnvironment = true;
			_dbAccess.Insert(new UsersWithStaticInsert { UserName = insGuid });
			_dbAccess.IsMultiProviderEnvironment = false;
			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.TableName);
			var selectTest = _dbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertTest()
		{
			var insGuid = Guid.NewGuid().ToString();


			_dbAccess.Insert(new Users { UserName = insGuid });
			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.TableName);
			var selectTest = _dbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, insGuid);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertRange10kTest()
		{
			var insGuid = Guid.NewGuid().ToString();
			var containingList = new List<Users>();

			for (int i = 0; i < 10000; i++)
			{
				containingList.Add(new Users() { UserName = Guid.NewGuid().ToString("N") });
			}

			var stopWatch = new Stopwatch();
			stopWatch.Start();
			_dbAccess.InsertRange(containingList);
			stopWatch.Stop();
			//Assert.That(stopWatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(7)));

			var selectUsernameFromWhere = string.Format("SELECT COUNT(1) FROM {0}", UsersMeta.TableName);
			var selectTest = _dbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, 10000);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertRange10kEachItemTest()
		{
			var insGuid = Guid.NewGuid().ToString();
			var containingList = new List<Users>();

			for (int i = 0; i < 10000; i++)
			{
				containingList.Add(new Users() { UserName = Guid.NewGuid().ToString("N") });
			}

			_dbAccess.RangerInsertPation = 1;
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			_dbAccess.InsertRange(containingList);
			stopWatch.Stop();
			//Assert.That(stopWatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(7)));

			var selectUsernameFromWhere = string.Format("SELECT COUNT(1) FROM {0}", UsersMeta.TableName);
			var selectTest = _dbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, 10000);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertDefaultValues()
		{
			_dbAccess.Config
				.Include<Users>()
				.SetConfig<Users>(conf =>
				{
					conf.SetPropertyAttribute(s => s.UserName, new IgnoreReflectionAttribute());
				});

			_dbAccess.Insert(new Users());
			var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.TableName);
			var selectTest = _dbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

			Assert.IsNotNull(selectTest);
			Assert.AreEqual(selectTest, DBNull.Value);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertWithSelect()
		{
			var val = new Users { UserName = "test" };
			var refSelect = _dbAccess.InsertWithSelect(val);

			Assert.AreEqual(refSelect.UserName, val.UserName);
			Assert.AreNotEqual(refSelect.UserID, val.UserID);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void InsertWithSelectStringTest()
		{
			var insGuid = Guid.NewGuid().ToString();

			_dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);

			var expectedUser = _dbAccess.InsertWithSelect(new Users { UserName = insGuid });
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

			_dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);

			var expectedUser = _dbAccess.InsertWithSelect(new Users { UserName = insGuid });
			Assert.IsNotNull(expectedUser);
			Assert.AreEqual(expectedUser.UserName, insGuid);
			Assert.AreNotEqual(expectedUser.UserID, default(long));
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void MarsTest()
		{
			RangeInsertTest();

			var baseQuery = _dbAccess.Query().Select.Table<Users>();
			var queryA = baseQuery.ContainerObject.Compile();
			var queryB = baseQuery.ContainerObject.Compile();
			Assert.IsNotNull(queryA);
			Assert.IsNotNull(queryB);

			var marsCommand = _dbAccess.Database.MergeCommands(queryA, queryB, true);
			var returnValue = _dbAccess.ExecuteMARS(marsCommand, typeof(Users), typeof(Users));
			Assert.IsNotNull(returnValue);
			Assert.AreNotSame(returnValue.Count, 0);

			var queryAResult = returnValue.ElementAt(0);
			var queryBResult = returnValue.ElementAt(1);
			Assert.AreNotSame(queryAResult.Count, 0);
			Assert.AreEqual(queryAResult.Count, queryBResult.Count);

			var refCall = _dbAccess.Select<Users>();
			Assert.AreEqual(refCall.Length, queryAResult.Count);
		}

		[Test]
		[Category("MsSQL")]
		public void ProcedureParamLessTest()
		{
			if (_dbAccess.DbAccessType != DbAccessType.MsSql)
				return;
			RangeInsertTest();
			var expectedUser = _dbAccess.ExecuteProcedure<TestProcAParams, Users>(new TestProcAParams());

			Assert.IsNotNull(expectedUser);
			Assert.AreNotEqual(expectedUser.Length, 0);

			var refSelect =
				_dbAccess.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT (*) FROM {0}", UsersMeta.TableName)));
			Assert.AreEqual(expectedUser.Length, refSelect);
		}

		[Test]
		[Category("MsSQL")]
		public void ProcedureParamTest()
		{
			if (_dbAccess.DbAccessType != DbAccessType.MsSql)
				return;
			RangeInsertTest();

			Assert.That(() => _dbAccess.ExecuteProcedure<TestProcBParams, Users>(new TestProcBParams()
			{
				Number = 10
			}), Is.Not.Null.And.Property("Length").EqualTo(9));
		}

		[Test]
		[Category("MsSQL")]
		public void ProcedureDirectParamTest()
		{
			if (_dbAccess.DbAccessType != DbAccessType.MsSql)
				return;
			RangeInsertTest();

			Assert.That(() => _dbAccess.Select<TestProcBParamsDirect>(new object[] { 10 }),
				Is.Not.Null.And.Property("Length").EqualTo(9));
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void RangeInsertTest()
		{
			_dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);

			var upperCountTestUsers = 100;
			var testUers = new List<Users>();

			var insGuid = Guid.NewGuid().ToString();

			for (var i = 0; i < upperCountTestUsers; i++)
			{
				testUers.Add(new Users { UserName = insGuid });
			}

			_dbAccess.InsertRange(testUers);

			var refSelect =
				_dbAccess.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.TableName)));
			if (refSelect is long)
				refSelect = Convert.ChangeType(refSelect, typeof(int));

			Assert.AreEqual(testUers.Count, refSelect);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void Refresh()
		{
			InsertTest();

			var singleEntity = _dbAccess
				.Query()
				.Top<Users>(1)
				.ForResult<Users>()
				.Single();

			var id = singleEntity.UserID;
			Assert.IsNotNull(singleEntity);

			var preName = singleEntity.UserName;
			var postName = Guid.NewGuid().ToString();
			Assert.IsNotNull(preName);

			singleEntity.UserName = postName;
			_dbAccess.Update(singleEntity);
			singleEntity.UserName = null;

			singleEntity = _dbAccess.Refresh(singleEntity);
			var refEntity = _dbAccess.Select<Users>(id);

			Assert.IsNotNull(refEntity);
			Assert.AreEqual(id, refEntity.UserID);
			Assert.AreEqual(singleEntity.UserID, refEntity.UserID);
			Assert.AreEqual(singleEntity.UserName, refEntity.UserName);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void RefreshInplace()
		{
			InsertTest();
			var singleEntity = _dbAccess
				.Query()
				.Top<Base.TestModels.CheckWrapperBaseTests.Users>(1)
				.ForResult<Users>()
				.Single();
			var id = singleEntity.UserID;
			Assert.IsNotNull(singleEntity);

			var preName = singleEntity.UserName;
			var postName = Guid.NewGuid().ToString();
			Assert.IsNotNull(preName);

			singleEntity.UserName = postName;
			_dbAccess.Update(singleEntity);
			singleEntity.UserName = null;

			_dbAccess.RefreshKeepObject(singleEntity);
			var refEntity = _dbAccess.Select<Users>(id);

			Assert.IsNotNull(refEntity);
			Assert.AreEqual(id, refEntity.UserID);
			Assert.AreEqual(singleEntity.UserID, refEntity.UserID);
			Assert.AreEqual(singleEntity.UserName, refEntity.UserName);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectBase()
		{
			InsertTest();
			var refSelect = _dbAccess.Select<Users>();
			Assert.IsTrue(refSelect.Length > 0);

			var testInsertName = Guid.NewGuid().ToString();
			var testUser = _dbAccess.InsertWithSelect(new Users { UserName = testInsertName });
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserID, default(long));

			var selTestUser = _dbAccess.Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(new object[] { testUser.UserID }).FirstOrDefault();
			Assert.That(selTestUser, Is.Not.Null);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.UserID);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectWhereBase()
		{
			InsertTest();
			var refSelect = _dbAccess.SelectWhere<Users>("UserName IS NOT NULL");
			Assert.IsTrue(refSelect.Length > 0);

			var testInsertName = Guid.NewGuid().ToString();
			var testUser = _dbAccess.InsertWithSelect(new Users { UserName = testInsertName });
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserID, default(long));

			var selTestUser = _dbAccess.SelectWhere<Users>("User_ID = @id", new { id = testUser.UserID }).FirstOrDefault();
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserID, testUser.UserID);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectModelsSelect()
		{
			RangeInsertTest();
			var firstAvaibleUser = _dbAccess.Query().Top<Base.TestModels.CheckWrapperBaseTests.Users>(1).ForResult<Users>().First();

			var refSelect = _dbAccess.Select<Users_PK>(firstAvaibleUser.UserID);
			Assert.IsNotNull(refSelect);

			var userSelectAlternatingProperty = _dbAccess.Select<Users_PK_IDFM>(firstAvaibleUser.UserID);
			Assert.IsNotNull(userSelectAlternatingProperty);

			var userSelectStaticSel = _dbAccess.Select<Users_PK_IDFM_CLASSEL>(firstAvaibleUser.UserID);
			Assert.IsNotNull(userSelectStaticSel);
		}


		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectNative()
		{
			InsertTest();

			var refSelect = _dbAccess.SelectNative<Users>(UsersMeta.SelectStatement);
			Assert.IsTrue(refSelect.Any());

			var anyId = refSelect.FirstOrDefault().UserID;
			Assert.AreNotEqual(anyId, 0);

			refSelect =
				_dbAccess.SelectNative<Users>(UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
					new QueryParameter("paramA", anyId));
			Assert.IsTrue(refSelect.Length > 0);

			refSelect =
				_dbAccess.SelectNative<Users>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA", new { paramA = anyId });
			Assert.IsTrue(refSelect.Length > 0);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectPrimitivSelect()
		{
			InsertTest();
			var refSelect = _dbAccess.RunPrimetivSelect<long>(UsersMeta.SelectStatement);
			Assert.IsTrue(refSelect.Any());

			var anyId = refSelect.FirstOrDefault();
			Assert.AreNotEqual(anyId, 0);

			refSelect =
				_dbAccess.RunPrimetivSelect<long>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
					new QueryParameter("paramA", anyId));
			Assert.IsTrue(refSelect.Length > 0);

			refSelect =
				_dbAccess.RunPrimetivSelect<long>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA", new { paramA = anyId });
			Assert.IsTrue(refSelect.Length > 0);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectPrimitivSelectNullHandling()
		{
			if (_dbAccess.DbAccessType != DbAccessType.MsSql)
				return;
			InsertTest();
			Assert.That(() =>
			{
				_dbAccess.RunPrimetivSelect<long>(
						UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
						new QueryParameter("paramA", null));
			}, Throws.Exception);

			Assert.That(() =>
			{
				string n = null;
				_dbAccess.RunPrimetivSelect<long>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA", new { paramA = n });
			}, Throws.Exception);
		}


		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SyncCollectionTest()
		{
			RangeInsertTest();

			DbCollection<Users_Col> dbCollection = null;
			Assert.That(() => dbCollection = _dbAccess.CreateDbCollection<Users_Col>(), Throws.Nothing);
			Assert.That(dbCollection, Is.Not.Empty);
			Assert.That(dbCollection.Count, Is.EqualTo(100));

			Assert.That(() => dbCollection.Add(new Users_Col()), Throws.Nothing);
			Assert.That(dbCollection.Count, Is.EqualTo(101));

			Assert.That(() => dbCollection.SaveChanges(_dbAccess), Throws.Nothing);
			Assert.That(() => _dbAccess.Select<Users_Col>().Length, Is.EqualTo(101));

			Assert.That(() => dbCollection.Remove(dbCollection.First()), Throws.Nothing);
			Assert.That(dbCollection.Count, Is.EqualTo(100));

			Assert.That(() => dbCollection.SaveChanges(_dbAccess), Throws.Nothing);
			Assert.That(() => _dbAccess.Select<Users_Col>().Length, Is.EqualTo(100));

			var user25 = dbCollection[25];
			user25.UserName = Guid.NewGuid().ToString();

			Assert.That(() => dbCollection.GetEntryState(user25), Is.EqualTo(CollectionStates.Changed));
			Assert.That(() => dbCollection.SaveChanges(_dbAccess), Throws.Nothing);
			Assert.That(() => dbCollection.GetEntryState(user25), Is.EqualTo(CollectionStates.Unchanged));

			Assert.That(() => _dbAccess.Select<Users_Col>(user25.User_ID), Is.Not.Null.And
				.Property("User_ID").EqualTo(user25.User_ID)
				.And
				.Property("UserName").EqualTo(user25.UserName));
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void Update()
		{
			InsertTest();
			var query = _dbAccess
				.Query()
				.Top<Users>(1);
			var singleEntity = query
				.ForResult<Users>()
				.Single();
			Assert.IsNotNull(singleEntity);

			var preName = singleEntity.UserName;
			var postName = Guid.NewGuid().ToString();
			Assert.IsNotNull(preName);

			singleEntity.UserName = postName;
			_dbAccess.Update(singleEntity);

			var refEntity = _dbAccess.Select<Users>(singleEntity.UserID);
			Assert.IsNotNull(refEntity);
			Assert.AreEqual(singleEntity.UserID, refEntity.UserID);
			Assert.AreEqual(singleEntity.UserName, refEntity.UserName);
		}
	}
}