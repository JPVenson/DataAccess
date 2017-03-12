using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Users = JPB.DataAccess.Tests.Base.Users;

namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
    public class ProcedureTests : BaseTest
    {
        public ProcedureTests(DbAccessType type) : base(type)
        {
        }

        [Test]
        [Category("MsSQL")]
        public void ProcedureDirectParamTest()
        {
            if (_dbAccess.DbAccessType != DbAccessType.MsSql)
                return;
            DataMigrationHelper.AddUsers(100, _dbAccess);

            Assert.That(() => _dbAccess.Select<TestProcBParamsDirect>(new object[] {10}),
                Is.Not.Null.And.Property("Length").EqualTo(9));
        }

        [Test]
        [Category("MsSQL")]
        public void ProcedureParamLessTest()
        {
            if (_dbAccess.DbAccessType != DbAccessType.MsSql)
                return;
            DataMigrationHelper.AddUsers(100, _dbAccess);
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
            DataMigrationHelper.AddUsers(100, _dbAccess);

            Assert.That(() => _dbAccess.ExecuteProcedure<TestProcBParams, Users>(new TestProcBParams
            {
                Number = 10
            }), Is.Not.Null.And.Property("Length").EqualTo(9));
        }
    }
}