#region

using System;
using System.Linq;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;

#endregion

namespace JPB.DataAccess.Tests.DbAccessLayerTests
{
	[TestFixture(DbAccessType.MsSql, true)]
	[TestFixture(DbAccessType.SqLite, true)]
	[TestFixture(DbAccessType.MsSql, false)]
	[TestFixture(DbAccessType.SqLite, false)]
	[Parallelizable(ParallelScope.Fixtures | ParallelScope.Self | ParallelScope.Children)]
	public class SelectionTests : DatabaseBaseTest
	{
		[SetUp]
		public void InitEgarLoading()
		{
			DbAccess.LoadCompleteResultBeforeMapping = _egarLoading;
		}

		private readonly bool _egarLoading;

		public SelectionTests(DbAccessType type, bool egarLoading) : base(type, egarLoading.ToString())
		{
			_egarLoading = egarLoading;
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectAnonymous()
		{
			DataMigrationHelper.AddEntity<Users, long>(DbAccess, 5, f => f.UserName = "Test");
			var usersUsernameAnonymouses = DbAccess.Select<Users_UsernameAnonymous>();
			Assert.That(usersUsernameAnonymouses,
				Is.All.Property(UsersMeta.ContentName).Not.Null.And.Not.EqualTo("Test"));
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectBase()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var refSelect = DbAccess.Select<Users>();
			Assert.IsTrue(refSelect.Length > 0);

			var testInsertName = Guid.NewGuid().ToString();
			var testUser = DbAccess.InsertWithSelect(new Users {UserName = testInsertName});
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserID, default(long));

			var selTestUser =
				DbAccess.Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(new object[] {testUser.UserID}).FirstOrDefault();
			Assert.That(selTestUser, Is.Not.Null);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.UserID);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectModelsSelect()
		{
			DataMigrationHelper.AddUsers(100, DbAccess);
			var firstAvaibleUser =
				DbAccess.Query().Select.Table<Base.TestModels.CheckWrapperBaseTests.Users>().LimitBy(1).ForResult<Users>().First();

			var refSelect = DbAccess.Select<Users_PK>(firstAvaibleUser.UserID);
			Assert.IsNotNull(refSelect);

			var userSelectAlternatingProperty = DbAccess.Select<Users_PK_IDFM>(firstAvaibleUser.UserID);
			Assert.IsNotNull(userSelectAlternatingProperty);

			var userSelectStaticSel = DbAccess.Select<Users_PK_IDFM_CLASSEL>(firstAvaibleUser.UserID);
			Assert.IsNotNull(userSelectStaticSel);
		}


		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectNative()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);

			var refSelect = DbAccess.SelectNative<Users>(UsersMeta.SelectStatement);
			Assert.IsTrue(refSelect.Any());

			var anyId = refSelect.FirstOrDefault().UserID;
			Assert.AreNotEqual(anyId, 0);

			refSelect =
				DbAccess.SelectNative<Users>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
					new QueryParameter("paramA", anyId));
			Assert.IsTrue(refSelect.Length > 0);

			refSelect =
				DbAccess.SelectNative<Users>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
					new {paramA = anyId});
			Assert.IsTrue(refSelect.Length > 0);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectPrimitivSelect()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var refSelect = DbAccess.RunPrimetivSelect<long>(UsersMeta.SelectStatement);
			Assert.IsTrue(refSelect.Any());

			var anyId = refSelect.FirstOrDefault();
			Assert.AreNotEqual(anyId, 0);

			refSelect =
				DbAccess.RunPrimetivSelect<long>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
					new QueryParameter("paramA", anyId));
			Assert.IsTrue(refSelect.Length > 0);

			refSelect =
				DbAccess.RunPrimetivSelect<long>(
					UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
					new {paramA = anyId});
			Assert.IsTrue(refSelect.Length > 0);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectPropertyLessPoco()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			Assert.That(() => DbAccess.Select<UsersWithoutProperties>(), Is.Not.Null.And.Not.Empty);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectWhereBase()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var refSelect = DbAccess.SelectWhere<Users>("UserName IS NOT NULL");
			Assert.IsTrue(refSelect.Length > 0);

			var testInsertName = Guid.NewGuid().ToString();
			var testUser = DbAccess.InsertWithSelect(new Users {UserName = testInsertName});
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserID, default(long));

			var selTestUser = DbAccess.SelectWhere<Users>("User_ID = @id", new {id = testUser.UserID}).FirstOrDefault();
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserID, testUser.UserID);
		}

		[Test]
		[Category("MsSQL")]
		[Category("SqLite")]
		public void SelectWithEgarLoading()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var refSelect = DbAccess.Select<Users>();
			Assert.IsTrue(refSelect.Length > 0);

			var testInsertName = Guid.NewGuid().ToString();
			var testUser = DbAccess.InsertWithSelect(new Users {UserName = testInsertName});
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserID, default(long));

			var selTestUser =
				DbAccess.Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(new object[] {testUser.UserID}).FirstOrDefault();
			Assert.That(selTestUser, Is.Not.Null);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.UserID);
		}
	}
}