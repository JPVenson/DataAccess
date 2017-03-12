using JPB.DataAccess.Manager;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
    [TestFixture(DbAccessType.MsSql)]
    [TestFixture(DbAccessType.SqLite)]
    public abstract class BaseTest
    {
        protected BaseTest(DbAccessType type)
        {
            _type = type;
        }

        protected DbAccessLayer _dbAccess;
        protected IManager _mgr;
        protected readonly DbAccessType _type;

        [SetUp]
        public void Init()
        {
            _mgr = new Manager();
            _dbAccess = _mgr.GetWrapper(_type);
        }

        [TearDown]
        public void TestTearDown()
        {
            this.TearDown(_mgr);
        }

        [SetUp]
        public void Clear()
        {
            this.Clear(_dbAccess);
        }
    }
}