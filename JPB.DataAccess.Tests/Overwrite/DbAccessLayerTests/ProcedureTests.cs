using JPB.DataAccess.Manager;
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
    public class ProcedureTests
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

        public ProcedureTests(DbAccessType type)
        {
            _type = type;
        }

        private DbAccessLayer _dbAccess;
        private IManager _mgr;

        [Test]
        [Category("MsSQL")]
        public void ProcedureDirectParamTest()
        {
            if (_dbAccess.DbAccessType != DbAccessType.MsSql)
                return;
            DataMigrationHelper.AddUsers(100, _dbAccess);

            Assert.That(() => _dbAccess.Select<TestProcBParamsDirect>(new object[] { 10 }),
                Is.Not.Null.And.Property("Length").EqualTo(9));
        }

        [Test]
        [Category("MsSQL")]
        public void ProcedureParamLessTest()
        {
            if (_dbAccess.DbAccessType != DbAccessType.MsSql)
                return;
            DataMigrationHelper.AddUsers(100, _dbAccess);
            var expectedUser = _dbAccess.ExecuteProcedure<TestProcAParams, Base.Users>(new TestProcAParams());

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
            DataMigrationHelper.AddUsers(100, _dbAccess);

            Assert.That(() => _dbAccess.ExecuteProcedure<TestProcBParams, Users>(new TestProcBParams
            {
                Number = 10
            }), Is.Not.Null.And.Property("Length").EqualTo(9));
        }
    }
}
