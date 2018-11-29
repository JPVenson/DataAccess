#region

using System;
using System.Linq;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;
using JPB.DataAccess.Tests.TestFramework;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;
#pragma warning disable 618

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests
{
	[Parallelizable(GlobalTestContext.MainParallelScope)]
	public class SelectionTests : DatabaseStandardTest
	{
		public SelectionTests(DbAccessType type, bool egarLoading, bool asyncExecution, bool syncronised) 
			: base(type, egarLoading, asyncExecution, syncronised){
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
				DbAccess.Query().Select.Table<Base.TestModels.CheckWrapperBaseTests.Users>().LimitBy(1).ForResult<Users>().First();

			var refSelect = DbAccess.Select<Users_PK>(firstAvaibleUser.UserID);
			Assert.IsNotNull(refSelect);

			var userSelectAlternatingProperty = DbAccess.Select<Users_PK_IDFM>(firstAvaibleUser.UserID);
			Assert.IsNotNull(userSelectAlternatingProperty);

			var userSelectStaticSel = DbAccess.Select<Users_PK_IDFM_CLASSEL>(firstAvaibleUser.UserID);
			Assert.IsNotNull(userSelectStaticSel);
		}


		[Test]
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
					new { paramA = anyId });
			Assert.IsTrue(refSelect.Length > 0);
		}

		[Test]
		public void SelectPrimitivSelect()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var refSelect = DbAccess.RunPrimetivSelect<long>(UsersMeta.SelectPrimaryKeyStatement);
			Assert.IsTrue(refSelect.Any());

			var anyId = refSelect.FirstOrDefault();
			Assert.AreNotEqual(anyId, 0);

			refSelect =
				DbAccess.RunPrimetivSelect<long>(
					UsersMeta.SelectPrimaryKeyStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
					new QueryParameter("paramA", anyId));
			Assert.IsTrue(refSelect.Length > 0);

			refSelect =
				DbAccess.RunPrimetivSelect<long>(
					UsersMeta.SelectPrimaryKeyStatement + " WHERE " + UsersMeta.PrimaryKeyName + " = @paramA",
					new { paramA = anyId });
			Assert.IsTrue(refSelect.Length > 0);
		}

		[Test]
		public void SelectPropertyLessPoco()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			Assert.That(() => DbAccess.Select<UsersWithoutProperties>(), Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void SelectWhereBase()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var refSelect = DbAccess.SelectWhere<Users>("UserName IS NOT NULL");
			Assert.IsTrue(refSelect.Length > 0);

			var testInsertName = Guid.NewGuid().ToString();
			var testUser = DbAccess.InsertWithSelect(new Users { UserName = testInsertName });
			Assert.IsNotNull(testUser);
			Assert.AreNotEqual(testUser.UserID, default(long));

			var selTestUser = DbAccess.SelectWhere<Users>("User_ID = @id", new { id = testUser.UserID }).FirstOrDefault();
			Assert.AreEqual(selTestUser.UserName, testUser.UserName);
			Assert.AreEqual(selTestUser.UserID, testUser.UserID);
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

		[Test]
		[DbCategory(DbAccessType.MsSql)]
		public void AutoGenFactoryTestXmlMulti()
		{
			DbAccess.Insert(new UsersAutoGenerateConstructorWithMultiXml());
			DbAccess.Insert(new UsersAutoGenerateConstructorWithMultiXml());
			DbAccess.Insert(new UsersAutoGenerateConstructorWithMultiXml());

			var elements = DbAccess.Query()
								   .QueryText("SELECT")
								   .QueryText("res." + UsersMeta.PrimaryKeyName)
								   .QueryText(",res." + UsersMeta.ContentName)
								   .QueryText(",")
								   .InBracket(
								   s =>
										   s.Select.Table<UsersAutoGenerateConstructorWithMultiXml>()
											.ForXml(typeof(UsersAutoGenerateConstructorWithMultiXml)))
								   .QueryText("AS Subs")
								   .QueryText("FROM")
								   .QueryText(UsersMeta.TableName)
								   .QueryText("AS res")
								   .ForResult<UsersAutoGenerateConstructorWithMultiXml>();

			var result = elements.ToArray();

			Assert.IsNotNull(result);
			Assert.IsNotEmpty(result);
		}

		[Test]
		[DbCategory(DbAccessType.MsSql)]
		public void AutoGenFactoryTestXmlSingle()
		{
			DbAccess.Insert(new UsersAutoGenerateConstructorWithSingleXml());

			var query = DbAccess.Query()
								.QueryText("SELECT")
								.QueryText("res." + UsersMeta.PrimaryKeyName)
								.QueryText(",res." + UsersMeta.ContentName)
								.QueryText(",")
								.InBracket(s =>
										s.Select.Table<UsersAutoGenerateConstructorWithSingleXml>()
										 .ForXml(typeof(UsersAutoGenerateConstructorWithSingleXml)))
								.QueryText("AS Sub")
								.QueryText("FROM")
								.QueryText(UsersMeta.TableName)
								.QueryText("AS res");
			var elements =
					query.ForResult<UsersAutoGenerateConstructorWithSingleXml>();

			var result = elements.ToArray();

			Assert.IsNotNull(result);
			Assert.IsNotEmpty(result);
		}
	}
}