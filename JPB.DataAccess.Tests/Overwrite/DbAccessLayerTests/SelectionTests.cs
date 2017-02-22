using System;
using System.Linq;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Users = JPB.DataAccess.Tests.Base.Users;

namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
    [TestFixture(DbAccessType.MsSql)]
    [TestFixture(DbAccessType.SqLite)]
#if MySqlTests
    [TestFixture(DbAccessType.MySql)]
#endif
    public class SelectionTests
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

        public SelectionTests(DbAccessType type)
        {
            _type = type;
        }

        private DbAccessLayer _dbAccess;
        private IManager _mgr;

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void SelectBase()
        {
            DataMigrationHelper.AddUsers(1, _dbAccess);
            var refSelect = _dbAccess.Select<Base.Users>();
            Assert.IsTrue(refSelect.Length > 0);

            var testInsertName = Guid.NewGuid().ToString();
            var testUser = _dbAccess.InsertWithSelect(new Base.Users { UserName = testInsertName });
            Assert.IsNotNull(testUser);
            Assert.AreNotEqual(testUser.UserID, default(long));

            var selTestUser =
                _dbAccess.Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(new object[] { testUser.UserID }).FirstOrDefault();
            Assert.That(selTestUser, Is.Not.Null);
            Assert.AreEqual(selTestUser.UserName, testUser.UserName);
            Assert.AreEqual(selTestUser.UserId, testUser.UserID);
        }

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void SelectModelsSelect()
        {
            DataMigrationHelper.AddUsers(100, _dbAccess);
            var firstAvaibleUser =
                _dbAccess.Query().Top<Base.TestModels.CheckWrapperBaseTests.Users>(1).ForResult<Base.Users>().First();

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
            DataMigrationHelper.AddUsers(1, _dbAccess);

            var refSelect = _dbAccess.SelectNative<Base.Users>(UsersMeta.SelectStatement);
            Assert.IsTrue(refSelect.Any());

            var anyId = refSelect.FirstOrDefault().UserID;
            Assert.AreNotEqual(anyId, 0);

            refSelect =
                _dbAccess.SelectNative<Base.Users>(
                    UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
                    new QueryParameter("paramA", anyId));
            Assert.IsTrue(refSelect.Length > 0);

            refSelect =
                _dbAccess.SelectNative<Base.Users>(
                    UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
                    new { paramA = anyId });
            Assert.IsTrue(refSelect.Length > 0);
        }

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void SelectPrimitivSelect()
        {
            DataMigrationHelper.AddUsers(1, _dbAccess);
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
                    UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
                    new { paramA = anyId });
            Assert.IsTrue(refSelect.Length > 0);
        }

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void SelectPrimitivSelectNullHandling()
        {
            if (_dbAccess.DbAccessType != DbAccessType.MsSql)
                return;
            DataMigrationHelper.AddUsers(1, _dbAccess);
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
        public void SelectPropertyLessPoco()
        {
            DataMigrationHelper.AddUsers(1, _dbAccess);
            Assert.That(() => _dbAccess.Select<UsersWithoutProperties>(), Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void SelectWhereBase()
        {
            DataMigrationHelper.AddUsers(1, _dbAccess);
            var refSelect = _dbAccess.SelectWhere<Base.Users>("UserName IS NOT NULL");
            Assert.IsTrue(refSelect.Length > 0);

            var testInsertName = Guid.NewGuid().ToString();
            var testUser = _dbAccess.InsertWithSelect(new Base.Users { UserName = testInsertName });
            Assert.IsNotNull(testUser);
            Assert.AreNotEqual(testUser.UserID, default(long));

            var selTestUser = _dbAccess.SelectWhere<Users>("User_ID = @id", new { id = testUser.UserID }).FirstOrDefault();
            Assert.AreEqual(selTestUser.UserName, testUser.UserName);
            Assert.AreEqual(selTestUser.UserID, testUser.UserID);
        }
    }
}
