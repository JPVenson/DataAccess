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
    public class SelectionTests : BaseTest
    {
        public SelectionTests(DbAccessType type) : base(type)
        {
        }

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void SelectBase()
        {
            DataMigrationHelper.AddUsers(1, _dbAccess);
            var refSelect = _dbAccess.Select<Users>();
            Assert.IsTrue(refSelect.Length > 0);

            var testInsertName = Guid.NewGuid().ToString();
            var testUser = _dbAccess.InsertWithSelect(new Users {UserName = testInsertName});
            Assert.IsNotNull(testUser);
            Assert.AreNotEqual(testUser.UserID, default(long));

            var selTestUser =
                _dbAccess.Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(new object[] {testUser.UserID}).FirstOrDefault();
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
                _dbAccess.Query().Top<Base.TestModels.CheckWrapperBaseTests.Users>(1).ForResult<Users>().First();

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

            var refSelect = _dbAccess.SelectNative<Users>(UsersMeta.SelectStatement);
            Assert.IsTrue(refSelect.Any());

            var anyId = refSelect.FirstOrDefault().UserID;
            Assert.AreNotEqual(anyId, 0);

            refSelect =
                _dbAccess.SelectNative<Users>(
                    UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
                    new QueryParameter("paramA", anyId));
            Assert.IsTrue(refSelect.Length > 0);

            refSelect =
                _dbAccess.SelectNative<Users>(
                    UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
                    new {paramA = anyId});
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
                    new {paramA = anyId});
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
                    UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA", new {paramA = n});
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
            var refSelect = _dbAccess.SelectWhere<Users>("UserName IS NOT NULL");
            Assert.IsTrue(refSelect.Length > 0);

            var testInsertName = Guid.NewGuid().ToString();
            var testUser = _dbAccess.InsertWithSelect(new Users {UserName = testInsertName});
            Assert.IsNotNull(testUser);
            Assert.AreNotEqual(testUser.UserID, default(long));

            var selTestUser = _dbAccess.SelectWhere<Users>("User_ID = @id", new {id = testUser.UserID}).FirstOrDefault();
            Assert.AreEqual(selTestUser.UserName, testUser.UserName);
            Assert.AreEqual(selTestUser.UserID, testUser.UserID);
        }

        [Test]
        [Category("MsSQL")]
        [Category("SqLite")]
        public void SelectAnonymous()
        {
            DataMigrationHelper.AddEntity<Users, long>(_dbAccess, 5, f => f.UserName = "Test");
            var usersUsernameAnonymouses = _dbAccess.Select<Users_UsernameAnonymous>();
            Assert.That(usersUsernameAnonymouses,
                Is.All.Property(UsersMeta.ContentName).Not.Null.And.Not.EqualTo("Test"));
        }
    }
}