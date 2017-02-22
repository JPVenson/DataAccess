using System;
using System.Linq;
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
    public class UpdateRefreshTests
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

        public UpdateRefreshTests(DbAccessType type)
        {
            _type = type;
        }

        private DbAccessLayer _dbAccess;
        private IManager _mgr;

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void Update()
        {
            DataMigrationHelper.AddUsers(1, _dbAccess);
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

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void Refresh()
        {
            DataMigrationHelper.AddUsers(1, _dbAccess);

            var singleEntity = _dbAccess
                .Query()
                .Top<Base.Users>(1)
                .ForResult<Base.Users>()
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
            var refEntity = _dbAccess.Select<Base.Users>(id);

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
            DataMigrationHelper.AddUsers(1, _dbAccess);
            var singleEntity = _dbAccess
                .Query()
                .Top<Base.TestModels.CheckWrapperBaseTests.Users>(1)
                .ForResult<Base.Users>()
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
    }
}
