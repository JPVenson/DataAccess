using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query;
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
            var fullNameOfTheMethod = TestContext.CurrentContext.Test.FullName;
            // method name only
            var methodName = TestContext.CurrentContext.Test.Name;
            // the state of the test execution
            var state = TestContext.CurrentContext.Result.Outcome == ResultState.Failure; // TestState enum

            if (state)
                _mgr.FlushErrorData();
        }

        [SetUp]
        public void Clear()
        {
            _dbAccess.Config.Dispose();
            _dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
            if (_dbAccess.DbAccessType == DbAccessType.MsSql)
                _dbAccess.ExecuteGenericCommand(string.Format("TRUNCATE TABLE {0} ", UsersMeta.TableName), null);
        }

        private readonly DbAccessType _type;

        public CheckWrapperBaseTests(DbAccessType type)
        {
            _type = type;
        }

        private DbAccessLayer _dbAccess;
        private IManager _mgr;

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
        public void AutoGenFactoryTestSimple()
        {
            _dbAccess.Insert(new UsersAutoGenerateConstructor());
            var elements = _dbAccess.Select<UsersAutoGenerateConstructor>();
            Assert.IsNotNull(elements);
            Assert.IsNotEmpty(elements);
        }

        [Test]
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

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void CheckFactory()
        {
            DataMigrationHelper.AddUsers(1, _dbAccess);
            Assert.That(() => _dbAccess.Select<Users_StaticQueryFactoryForSelect>(), Is.Not.Empty);

            var testInsertName = Guid.NewGuid().ToString();
            Users_StaticQueryFactoryForSelect testUser = null;
            Assert.That(
                () =>
                    testUser =
                        _dbAccess.InsertWithSelect(new Users_StaticQueryFactoryForSelect {UserName = testInsertName}),
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
            DataMigrationHelper.AddUsers(1, _dbAccess);
            Assert.That(() => _dbAccess.Select<Users_StaticQueryFactoryForSelect>(), Is.Not.Empty);

            var testInsertName = Guid.NewGuid().ToString();
            Users_StaticQueryFactoryForSelect testUser = null;
            Assert.That(
                () =>
                    testUser =
                        _dbAccess.InsertWithSelect(new Users_StaticQueryFactoryForSelect {UserName = testInsertName}),
                Is.Not.Null
                    .And.Property("UserId").Not.EqualTo(0));

            var selTestUser =
                _dbAccess.Select<Users_StaticQueryFactoryForSelectWithArugments>(new object[] {testUser.UserId})
                    .FirstOrDefault();
            Assert.That(selTestUser, Is.Not.Null
                .And.Property("UserName").EqualTo(testUser.UserName)
                .And.Property("UserId").EqualTo(testUser.UserId));
        }


        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void ExecuteGenericCommand()
        {
            var resultSelect1 = _dbAccess.ExecuteGenericCommand("Select 10", null);
            Assert.AreEqual(resultSelect1, -1);

            resultSelect1 = _dbAccess.ExecuteGenericCommand("SELECT @test", new {test = 10});
            Assert.AreEqual(resultSelect1, -1);

            resultSelect1 = _dbAccess.ExecuteGenericCommand("SELECT @test",
                new List<QueryParameter> {new QueryParameter("test", 10)});
            Assert.AreEqual(resultSelect1, -1);
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
        public void MarsTest()
        {
            DataMigrationHelper.AddUsers(100, _dbAccess);

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
        [Category("SqLite")]
        public void SyncCollectionTest()
        {
            DataMigrationHelper.AddUsers(100, _dbAccess);

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
        public void TransactionTest()
        {
            DataMigrationHelper.AddUsers(250, _dbAccess);
            var count =
                _dbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();

            _dbAccess.Database.RunInTransaction(dd =>
            {
                _dbAccess.Delete<Users>();
                dd.TransactionRollback();
            });

            var countAfter =
                _dbAccess.SelectNative(typeof(long), "SELECT COUNT(1) FROM " + UsersMeta.TableName).FirstOrDefault();
            Assert.That(count, Is.EqualTo(countAfter));
        }
    }
}