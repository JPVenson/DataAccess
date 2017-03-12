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
    public class UpdateRefreshTests : BaseTest
    {
        public UpdateRefreshTests(DbAccessType type) : base(type)
        {
        }

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
                .Top<Users>(1)
                .ForResult<Users>()
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
            var refEntity = _dbAccess.Select<Users>(id);

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
                .ForResult<Users>()
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