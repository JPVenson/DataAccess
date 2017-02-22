using System;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
    [TestFixture(DbAccessType.MsSql)]
    [TestFixture(DbAccessType.SqLite)]
#if MySqlTests
    [TestFixture(DbAccessType.MySql)]
#endif
    public class ConfigurationTests
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

        public ConfigurationTests(DbAccessType type)
        {
            _type = type;
        }

        private DbAccessLayer _dbAccess;
        private IManager _mgr;

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

            Assert.That(() => { _dbAccess.Insert(new ConfigLessUser { PropertyB = insGuid }); }, Throws.Exception);
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

    }
}
