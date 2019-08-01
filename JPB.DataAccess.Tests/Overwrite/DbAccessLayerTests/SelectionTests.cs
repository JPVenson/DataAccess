#region

using System;
using System.Linq;
using JPB.DataAccess.Framework;
using JPB.DataAccess.Framework.Helper;
using JPB.DataAccess.Framework.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Books;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.User;
using JPB.DataAccess.Tests.TestFramework;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;
#pragma warning disable 618

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests
{
	[TestFixture(DbAccessType.MsSql, true, true, false)]
	[TestFixture(DbAccessType.SqLite, true, true, false)]
	[TestFixture(DbAccessType.MsSql, false, true, false)]
	[TestFixture(DbAccessType.SqLite, false, true, false)]
	[TestFixture(DbAccessType.MsSql, true, false, false)]
	[TestFixture(DbAccessType.SqLite, true, false, false)]
	[TestFixture(DbAccessType.MsSql, false, false, false)]
	[TestFixture(DbAccessType.SqLite, false, false, false)]
	[TestFixture(DbAccessType.MsSql, true, false, true)]
	[TestFixture(DbAccessType.SqLite, true, false, true)]
	[TestFixture(DbAccessType.MsSql, false, false, true)]
	[TestFixture(DbAccessType.SqLite, false, false, true)]

	[Parallelizable( ParallelScope.Self)]
	public class SelectionTests : DatabaseBaseTest
	{
		[SetUp]
		public void InitEgarLoading()
		{
			DbAccess.LoadCompleteResultBeforeMapping = _egarLoading;
		}

		private readonly bool _egarLoading;

		public SelectionTests(DbAccessType type, bool egarLoading, bool asyncExecution, bool syncronised) : base(type, asyncExecution, syncronised, egarLoading.ToString())
		{
			_egarLoading = egarLoading;
		}

		[Test]
		public void SelectAnonymous()
		{
			DataMigrationHelper.AddEntity<Users, long>(DbAccess, 5, f => f.UserName = "Test");
			var usersUsernameAnonymouses = DbAccess.Select<Users_UsernameAnonymous>();
			Assert.That(usersUsernameAnonymouses,
				Is.All.Property(UsersMeta.ContentName).Not.Null.And.Not.EqualTo("Test"));
		}

		[Test]
		public void SelectBase()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var refSelect = DbAccess.Select<Users>();
			Assert.IsTrue(refSelect.Length > 0);

			var testInsertName = Guid.NewGuid().ToString();
			var testUser = DbAccess.InsertWithSelect(new Users { UserName = testInsertName });
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserID, default(long));

			var selTestUser =
				DbAccess.Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(new object[] { testUser.UserID }).FirstOrDefault();
			Assert.That(selTestUser, Is.Not.Null);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.UserID);
		}

		[Test]
		public void SelectModelsSelect()
		{
			DataMigrationHelper.AddUsers(100, DbAccess);
			var firstAvaibleUser =
				DbAccess.Query().Select.Table<Users>().LimitBy(1).First();

			var refSelect = DbAccess.SelectSingle<Users_PK>(firstAvaibleUser.UserID);
			Assert.IsNotNull(refSelect);

			var userSelectAlternatingProperty = DbAccess.SelectSingle<Users_PK_IDFM>(firstAvaibleUser.UserID);
			Assert.IsNotNull(userSelectAlternatingProperty);

			var userSelectStaticSel = DbAccess.SelectSingle<Users_PK_IDFM_CLASSEL>(firstAvaibleUser.UserID);
			Assert.IsNotNull(userSelectStaticSel);
		}


		[Test]
		public void SelectNative()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);

			var refSelect = DbAccess.RunSelect<Users>(DbAccess.Database.CreateCommand(UsersMeta.SelectStatement));
			Assert.IsTrue(refSelect.Any());

			var anyId = refSelect.FirstOrDefault().UserID;
			Assert.AreNotEqual(anyId, 0);

			refSelect =
				DbAccess.RunSelect<Users>(
					DbAccess.Database.CreateCommandWithParameterValues($"{UsersMeta.SelectStatement} WHERE {UsersMeta.PrimaryKeyName} = @paramA",
						new QueryParameter("paramA", anyId)));
			Assert.IsTrue(refSelect.Length > 0);

			refSelect =
				DbAccess.RunSelect<Users>(
					DbAccess.Database.CreateCommandWithParameterValues(UsersMeta.SelectStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
						new QueryParameter("paramA", anyId)));
			Assert.IsTrue(refSelect.Length > 0);
		}

		[Test]
		public void SelectPrimitivSelect()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var refSelect = DbAccess
				.RunSelect<long>(DbAccess.Database.CreateCommand(UsersMeta.SelectPrimaryKeyStatement));
			Assert.IsTrue(refSelect.Any());

			var anyId = refSelect.FirstOrDefault();
			Assert.AreNotEqual(anyId, 0);

			refSelect =
				DbAccess.RunSelect<long>(
					DbAccess.Database.CreateCommandWithParameterValues($"{UsersMeta.SelectPrimaryKeyStatement} WHERE {UsersMeta.PrimaryKeyName} = @paramA",
					new QueryParameter("paramA", anyId)));
			Assert.IsTrue(refSelect.Length > 0);

			refSelect =
				DbAccess.RunSelect<long>(
					DbAccess.Database.CreateCommandWithParameterValues(UsersMeta.SelectPrimaryKeyStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
						new QueryParameter("paramA", anyId)));
			Assert.IsTrue(refSelect.Length > 0);
		}

		[Test]
		public void SelectPropertyLessPoco()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			Assert.That(() => DbAccess.Select<UsersWithoutProperties>(), Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void SelectWithEgarLoading()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var refSelect = DbAccess.Select<Users>();
			Assert.IsTrue(refSelect.Length > 0);

			var testInsertName = Guid.NewGuid().ToString();
			var testUser = DbAccess.InsertWithSelect(new Users { UserName = testInsertName });
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserID, default(long));

			var selTestUser =
				DbAccess.Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(new object[] { testUser.UserID }).FirstOrDefault();
			Assert.That(selTestUser, Is.Not.Null);
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserId, testUser.UserID);
		}

		[Test]
		[DbCategory(DbAccessType.MsSql)]
		public void SelectBooksWithXmlImages()
		{
			DataMigrationHelper.AddBooksWithImage(5, 10, DbAccess);
			var bookXmls = DbAccess.Select<BookXml>();
			Assert.That(bookXmls, Is.Not.Null);
			Assert.That(bookXmls.Length, Is.EqualTo(5));
		}
	}
}