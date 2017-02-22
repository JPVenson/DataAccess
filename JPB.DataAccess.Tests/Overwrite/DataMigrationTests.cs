using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Users = JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Users;

namespace JPB.DataAccess.Tests
{
    [TestFixture(DbAccessType.MsSql, TestOf = typeof(DataMigrationHelper))]
    [TestFixture(DbAccessType.SqLite, TestOf = typeof(DataMigrationHelper))]
#if MySqlTests
    [TestFixture(DbAccessType.MySql, TestOf = typeof(DataMigrationHelper))]
#endif
    public class DataMigrationTests
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

        public DataMigrationTests(DbAccessType type)
        {
            _type = type;
        }

        private DbAccessLayer _dbAccess;
        private IManager _mgr;

        [Test()]
        [TestCase(UsersMeta.TableName, typeof(Base.Users))]
        [TestCase(UsersMeta.TableName, typeof(Users_Col))]
        [TestCase(UsersMeta.TableName, typeof(UsersWithoutProperties))]
        [TestCase(UsersMeta.TableName, typeof(Users))]
        [TestCase(UsersMeta.TableName, typeof(UsersAutoGenerateConstructor))]
        [TestCase(UsersMeta.TableName, typeof(UsersAutoGenerateNullableConstructor))]
        [TestCase(UsersMeta.TableName, typeof(GeneratedUsers))]
        [TestCase(UsersMeta.TableName, typeof(ConfigLessUserInplaceConfig))]
        [TestCase(UsersMeta.TableName, typeof(ConfigLessUserInplaceDirectConfig))]
        [TestCase(UsersMeta.TableName, typeof(Users_PK))]
        [TestCase(UsersMeta.TableName, typeof(Users_PK_UFM))]
        [TestCase(UsersMeta.TableName, typeof(Users_PK_IDFM))]
        [TestCase(UsersMeta.TableName, typeof(Users_PK_IDFM_CLASSEL))]
        [TestCase(UsersMeta.TableName, typeof(Users_PK_IDFM_FUNCSELECT))]
        [TestCase(UsersMeta.TableName, typeof(Users_PK_IDFM_FUNCSELECTFAC))]
        [TestCase(UsersMeta.TableName, typeof(Users_PK_IDFM_FUNCSELECTFACWITHPARAM))]
        [TestCase(UsersMeta.TableName, typeof(Users_StaticQueryFactoryForSelect))]
        public void AddGenericTest(string tableName, Type type)
        {
            Assert.That(() => DataMigrationHelper.ClearDb(_dbAccess), Throws.Nothing);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + tableName)), Is.Zero);
            Assert.That(DataMigrationHelper.AddEntity(_dbAccess, 200, type), Is.Not.Empty.And.Unique);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + tableName)), Is.EqualTo(200));
            Assert.That( DataMigrationHelper.AddEntity(_dbAccess, 200, type), Is.Not.Empty.And.Unique);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + tableName)), Is.EqualTo(400));
        }

        [Test()]
        public void AddUserTest()
        {
            Assert.That(() => DataMigrationHelper.ClearDb(_dbAccess), Throws.Nothing);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + UsersMeta.TableName)), Is.Zero);
            Assert.That(DataMigrationHelper.AddUsers(200, _dbAccess), Is.Not.Empty.And.Unique);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + UsersMeta.TableName)), Is.EqualTo(200));
            Assert.That(DataMigrationHelper.AddUsers(200, _dbAccess), Is.Not.Empty.And.Unique);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + UsersMeta.TableName)), Is.EqualTo(400));
        }

        [Test()]
        public void AddBooksTest()
        {
            Assert.That(() => DataMigrationHelper.ClearDb(_dbAccess), Throws.Nothing);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)), Is.Zero);
            Assert.That(DataMigrationHelper.AddBooks(200, _dbAccess), Is.Not.Empty.And.Unique);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)), Is.EqualTo(200));
            Assert.That(DataMigrationHelper.AddBooks(200, _dbAccess), Is.Not.Empty.And.Unique);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)), Is.EqualTo(400));
        }

        [Test()]
        public void AddImagesTest()
        {
            Assert.That(() => DataMigrationHelper.ClearDb(_dbAccess), Throws.Nothing);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)), Is.Zero);
            Assert.That(DataMigrationHelper.AddImages(200, _dbAccess), Is.Not.Empty.And.Unique);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)), Is.EqualTo(200));
            Assert.That(DataMigrationHelper.AddImages(200, _dbAccess), Is.Not.Empty.And.Unique);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)), Is.EqualTo(400));
        }

        [Test()]
        public void AddBooksWithImage()
        {
            Assert.That(() => DataMigrationHelper.ClearDb(_dbAccess), Throws.Nothing);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)), Is.Zero);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)), Is.Zero);
            Assert.That(DataMigrationHelper.AddBooksWithImage(200, 5, _dbAccess), Is.Not.Empty.And.Unique);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)), Is.EqualTo(200));
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)), Is.EqualTo(200 * 5));
            Assert.That(DataMigrationHelper.AddBooksWithImage(200, 5, _dbAccess), Is.Not.Empty.And.Unique);
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + BookMeta.TableName)), Is.EqualTo(400));
            Assert.That(() => _dbAccess.Database.Run(s => s.GetSkalar("SELECT COUNT(1) FROM " + ImageMeta.TableName)), Is.EqualTo(400 * 5));
        }
    }
}
