using System;
using System.Collections.Generic;
using System.Diagnostics;
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
#if MySqlTests
    [TestFixture(DbAccessType.MySql)]
#endif
    public class InsertTests
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

        public InsertTests(DbAccessType type)
        {
            _type = type;
        }

        private DbAccessLayer _dbAccess;
        private IManager _mgr;

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void InsertDefaultValues()
        {
            _dbAccess.Config
                .Include<Users>()
                .SetConfig<Users>(
                    conf => { conf.SetPropertyAttribute(s => s.UserName, new IgnoreReflectionAttribute()); });

            _dbAccess.Insert(new Users());
            var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.TableName);
            var selectTest = _dbAccess.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

            Assert.IsNotNull(selectTest);
            Assert.AreEqual(selectTest, DBNull.Value);
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
        public void InsertPropertyLessPoco()
        {
            Assert.That(() => _dbAccess.Insert(new UsersWithoutProperties()), Throws.Nothing);
        }

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void InsertRange10kEachItemTest()
        {
            var insGuid = Guid.NewGuid().ToString();
            var containingList = new List<Users>();

            for (var i = 0; i < 10000; i++)
                containingList.Add(new Users { UserName = Guid.NewGuid().ToString("N") });

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
        public void InsertRange10kTest()
        {
            var insGuid = Guid.NewGuid().ToString();
            var containingList = new List<Users>();

            for (var i = 0; i < 10000; i++)
                containingList.Add(new Users { UserName = Guid.NewGuid().ToString("N") });

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
        public void InsertRangeTest()
        {
            _dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);

            var upperCountTestUsers = 100;
            var testUers = new List<Users>();

            var insGuid = Guid.NewGuid().ToString();

            for (var i = 0; i < upperCountTestUsers; i++)
                testUers.Add(new Users { UserName = insGuid });

            _dbAccess.InsertRange(testUers);

            var refSelect =
                _dbAccess.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.TableName)));
            if (refSelect is long)
                refSelect = Convert.ChangeType(refSelect, typeof(int));

            Assert.AreEqual(testUers.Count, refSelect);
        }
    }
}
