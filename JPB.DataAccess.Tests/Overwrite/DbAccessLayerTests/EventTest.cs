using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
    public class EventTest : BaseTest
    {
        public EventTest(DbAccessType type) : base(type)
        {
        }

        [Test]
        public void TestOnUpdate()
        {
            _dbAccess.RaiseEvents = true;
            var insertWithSelect = _dbAccess.InsertWithSelect(new Users());

            var riseFlag = false;
            _dbAccess.OnUpdate += (sender, eventx) => { riseFlag = true; };
            _dbAccess.Update(insertWithSelect);
            Assert.True(riseFlag);
            _dbAccess.RaiseEvents = false;
            riseFlag = false;
            _dbAccess.Update(insertWithSelect);
            Assert.False(riseFlag);
        }

        [Test]
        public void TestOnInsert()
        {
            _dbAccess.RaiseEvents = true;
            var riseFlag = false;
            _dbAccess.OnInsert += (sender, eventx) => { riseFlag = true; };
            _dbAccess.Insert(new Users());
            Assert.True(riseFlag);

            _dbAccess.RaiseEvents = false;
            riseFlag = false;
            _dbAccess.Insert(new Users());
            Assert.False(riseFlag);
        }

        public void TestOnSelect()
        {
            _dbAccess.RaiseEvents = true;
        }

        public void TestOnDelete()
        {
            _dbAccess.RaiseEvents = true;
        }
    }
}