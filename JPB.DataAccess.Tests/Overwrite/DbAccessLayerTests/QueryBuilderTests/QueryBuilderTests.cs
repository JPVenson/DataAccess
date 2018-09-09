#region

using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query;
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;
#pragma warning disable 618

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests.QueryBuilderTests
{
	[TestFixture(DbAccessType.MsSql, true , false, true, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MsSql, true , false, true, EnumerationMode.OnCall)]
	[TestFixture(DbAccessType.MsSql, true , false, false, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MsSql, true , false, false, EnumerationMode.OnCall)]

	[TestFixture(DbAccessType.MsSql, false, false, true, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MsSql, false, false, true, EnumerationMode.OnCall)]
	[TestFixture(DbAccessType.MsSql, false, false, false, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MsSql, false, false, false, EnumerationMode.OnCall)]

	[TestFixture(DbAccessType.MsSql, false, true, true, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MsSql, false, true, true, EnumerationMode.OnCall)]
	[TestFixture(DbAccessType.MsSql, false, true, false, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MsSql, false, true, false, EnumerationMode.OnCall)]

	[TestFixture(DbAccessType.SqLite, false, true, true, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.SqLite, false, true, true, EnumerationMode.OnCall)]
	[TestFixture(DbAccessType.SqLite, false, true, false, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.SqLite, false, true, false, EnumerationMode.OnCall)]

	[TestFixture(DbAccessType.SqLite, false, false, true, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.SqLite, false, false, true, EnumerationMode.OnCall)]
	[TestFixture(DbAccessType.SqLite, false, false, false, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.SqLite, false, false, false, EnumerationMode.OnCall)]
										  
	[TestFixture(DbAccessType.SqLite, true, false, true, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.SqLite, true, false, true, EnumerationMode.OnCall)]
	[TestFixture(DbAccessType.SqLite, true, false, false, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.SqLite, true, false, false, EnumerationMode.OnCall)]

	[TestFixture(DbAccessType.MySql, false, true, true, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MySql, false, true, true, EnumerationMode.OnCall)]
	[TestFixture(DbAccessType.MySql, false, true, false, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MySql, false, true, false, EnumerationMode.OnCall)]

	[TestFixture(DbAccessType.MySql, false, false, true, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MySql, false, false, true, EnumerationMode.OnCall)]
	[TestFixture(DbAccessType.MySql, false, false, false, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MySql, false, false, false, EnumerationMode.OnCall)]
										  
	[TestFixture(DbAccessType.MySql, true, false, true, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MySql, true, false, true, EnumerationMode.OnCall)]
	[TestFixture(DbAccessType.MySql, true, false, false, EnumerationMode.FullOnLoad)]
	[TestFixture(DbAccessType.MySql, true, false, false, EnumerationMode.OnCall)]

	[Parallelizable(ParallelScope.Fixtures | ParallelScope.Self | ParallelScope.Children)]
	public class QueryBuilderTests : DatabaseBaseTest
	{
		private readonly EnumerationMode _enumerationMode;
		private readonly bool _asyncEnumeration;

		public QueryBuilderTests(DbAccessType type, bool asyncExecution, bool syncronised, bool asyncEnumeration, EnumerationMode enumerationMode) 
				: base(type, asyncExecution, syncronised, enumerationMode, asyncEnumeration)
		{
			_enumerationMode = enumerationMode;
			_asyncEnumeration = asyncEnumeration;
		}

		private RootQuery CreateQuery()
		{
			return DbAccess.Query().ConfigEnumerationMode(_enumerationMode);
		}

		[Test]
		public void AsCte()
		{
			var maxItems = 250;
			DataMigrationHelper.AddUsers(maxItems, DbAccess);
			Assert.That(() =>
			{
				var elementProducer = CreateQuery().Select.Table<Users>().AsCte<Users, Users>("cte");
				var query = elementProducer.ContainerObject.Compile();
				Assert.That(query, Is.Not.Null);
			}, Throws.Nothing);
		}

		[Test]

		public void CheckFactory()
		{
			var addUsers = DataMigrationHelper.AddUsers(250, DbAccess);
			Assert.That(
			() => CreateQuery().Select.Table<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>()
						  .ForResult(_asyncEnumeration).ToArray(), Is.Not.Empty);

			var testInsertName = Guid.NewGuid().ToString();
			Users_PK_IDFM_FUNCSELECTFACWITHPARAM testUser = null;
			Assert.That(
			() =>
					testUser =
							DbAccess.InsertWithSelect(new Users_PK_IDFM_FUNCSELECTFACWITHPARAM
							{
								UserName = testInsertName
							}),
			Is.Not.Null
			  .And.Property("UserId").Not.EqualTo(0));

			var selTestUser =
					CreateQuery()
							.Select.Table<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(testUser.UserId)
							.ForResult(_asyncEnumeration)
							.FirstOrDefault();
			Assert.That(selTestUser, Is.Not.Null);
			Assert.That(selTestUser.UserName, Is.EqualTo(testUser.UserName));
			Assert.That(selTestUser.UserId, Is.EqualTo(testUser.UserId));
		}

		[Test]

		public void Count()
		{
			DataMigrationHelper.AddUsers(250, DbAccess);

			var runPrimetivSelect = -1;
			var forResult = -1;
			var deSelect = DbAccess.Select<Users>();

			if (DbAccess.DbAccessType == DbAccessType.MsSql)
			{
				runPrimetivSelect =
						DbAccess.RunPrimetivSelect<int>(string.Format("SELECT COUNT(1) FROM {0}", UsersMeta.TableName))
								[0];
				forResult = CreateQuery().Select.Table<Users>().CountInt()
										 .ForResult(_asyncEnumeration).FirstOrDefault();
			}

			if (DbAccess.DbAccessType == DbAccessType.SqLite)
			{
				runPrimetivSelect =
						(int)
						DbAccess.RunPrimetivSelect<long>(string.Format("SELECT COUNT(1) FROM {0}", UsersMeta.TableName))
								[0];
				forResult = (int)CreateQuery().Select.Table<Users>().CountLong()
										  .ForResult(_asyncEnumeration).FirstOrDefault();
			}

			if (DbAccess.DbAccessType == DbAccessType.MySql)
			{
				runPrimetivSelect =
						(int)
						DbAccess.RunPrimetivSelect<long>(string.Format("SELECT COUNT(1) FROM {0}", UsersMeta.TableName))
								[0];
				forResult = (int)CreateQuery().Count<Users>().ForResult<long>(_asyncEnumeration)
										  .FirstOrDefault();
			}

			Assert.That(runPrimetivSelect, Is.EqualTo(forResult));
			Assert.That(deSelect.Length, Is.EqualTo(forResult));
		}

		[Test]
		public void ForginKeyTest()
		{
			var addBooksWithImage = DataMigrationHelper.AddBooksWithImage(1, 20, DbAccess);
			var array = CreateQuery().Select.Table<Image>().Where.ForginKey<Book>().Is.Not.Null.ForResult(_asyncEnumeration).ToArray();
			Assert.That(array, Is.Not.Empty);
			Assert.That(array[0].IdBook, Is.EqualTo(addBooksWithImage[0]));
		}

		[Test]
		public void In()
		{
			var maxItems = 250;
			var addUsers = DataMigrationHelper.AddUsers(maxItems, DbAccess);

			var rand = new Random(54541117);

			var elementsToRead = new List<long>();

			for (var i = 0; i < 25; i++)
			{
				elementsToRead.Add(addUsers[rand.Next(0, addUsers.Length)]);
			}

			var elementProducer =
					CreateQuery()
							.Select.Table<Users>()
							.Where.Column(f => f.UserID)
							.Is.In(elementsToRead.ToArray())
							.ForResult(_asyncEnumeration)
							.ToArray();
			var directQuery = DbAccess.SelectNative<Users>(UsersMeta.SelectStatement
														   + string.Format(" WHERE User_ID IN ({0})",
														   elementsToRead.Select(f => f.ToString())
																		 .Aggregate((e, f) => e + "," + f)));

			Assert.That(directQuery.Length, Is.EqualTo(elementProducer.Length));

			for (var index = 0; index < directQuery.Length; index++)
			{
				var userse = directQuery[index];
				var userbe = elementProducer[index];
				Assert.That(userbe.UserID, Is.EqualTo(userse.UserID));
				Assert.That(userbe.UserName, Is.EqualTo(userse.UserName));
			}
		}

		[Test]
		public void OrderBy()
		{
			var maxItems = 250;
			DataMigrationHelper.AddUsers(maxItems, DbAccess);
			var elementProducer =
					CreateQuery().Select.Table<Users>().Order.By(s => s.UserName)
							.ThenBy(f => f.UserID)
								 .ForResult(_asyncEnumeration).ToArray();
			var directQuery =
					DbAccess.SelectNative<Users>(UsersMeta.SelectStatement + " ORDER BY UserName, User_ID");

			Assert.That(directQuery.Length, Is.EqualTo(elementProducer.Length));

			for (var index = 0; index < directQuery.Length; index++)
			{
				var userse = directQuery[index];
				var userbe = elementProducer[index];
				Assert.That(userbe.UserID, Is.EqualTo(userse.UserID));
				Assert.That(userbe.UserName, Is.EqualTo(userse.UserName));
			}
		}

		[Test]
		public void Pager()
		{
			var maxItems = 250;

			DataMigrationHelper.AddUsers(maxItems, DbAccess);
			var basePager = DbAccess.Database.CreatePager<Users>();
			basePager.PageSize = 10;
			basePager.LoadPage(DbAccess);

			Assert.That(basePager.CurrentPage, Is.EqualTo(1));
			Assert.That(basePager.MaxPage, Is.EqualTo(maxItems / basePager.PageSize));

			var queryPager = CreateQuery()
									 .Select.Table<Users>()
									 .Order.By(f => f.UserID)
									 .ForPagedResult(1, basePager.PageSize);
			queryPager.LoadPage(DbAccess);

			Assert.That(basePager.CurrentPage, Is.EqualTo(queryPager.CurrentPage));
			Assert.That(basePager.MaxPage, Is.EqualTo(queryPager.MaxPage));
		}

		[Test]
		public void PagerWithCondtion()
		{
			var maxItems = 250;
			DataMigrationHelper.AddUsers(maxItems, DbAccess);

			var basePager = DbAccess.Database.CreatePager<Users>();
			basePager.BaseQuery = DbAccess.CreateSelect<Users>(" WHERE User_ID < 25");
			basePager.PageSize = 10;
			basePager.LoadPage(DbAccess);

			Assert.That(basePager.CurrentPage, Is.EqualTo(1));
			Assert.That(basePager.MaxPage, Is.EqualTo(Math.Ceiling(25F / basePager.PageSize)));

			var queryPager = CreateQuery().Select.Table<Users>()
									 .Where
									 .Column(f => f.UserID)
									 .IsQueryOperatorValue("< 25")
									 .Order.By(f => f.UserID)
									 .ForPagedResult(1, basePager.PageSize);
			queryPager.LoadPage(DbAccess);

			Assert.That(basePager.CurrentPage, Is.EqualTo(queryPager.CurrentPage));
			Assert.That(basePager.MaxPage, Is.EqualTo(queryPager.MaxPage));
		}

		[Test]
		public void RefIn()
		{
			var addBooksWithImage = DataMigrationHelper.AddBooksWithImage(250, 2, DbAccess);
			foreach (var id in addBooksWithImage)
			{
				var countOfImages = -1;
				if (DbAccess.DbAccessType == DbAccessType.MsSql)
				{
					countOfImages =
							DbAccess.RunPrimetivSelect<int>(
							string.Format("SELECT COUNT(1) FROM {0} WHERE {0}.{1} = {2}",
							ImageMeta.TableName, ImageMeta.ForgeinKeyName,
							id))[0];
				}

				if (DbAccess.DbAccessType == DbAccessType.SqLite)
				{
					countOfImages =
							(int)
							DbAccess.RunPrimetivSelect<long>(
							string.Format("SELECT COUNT(1) FROM {0} WHERE {0}.{1} = {2}",
							ImageMeta.TableName, ImageMeta.ForgeinKeyName,
							id))[0];
				}

				Assert.That(countOfImages, Is.EqualTo(2));
				var deSelect =
						DbAccess.SelectNative<Image>(string.Format("{2} AS b WHERE b.{0} = {1}",
						ImageMeta.ForgeinKeyName, id, ImageMeta.SelectStatement));
				Assert.That(deSelect, Is.Not.Empty);
				Assert.That(deSelect.Length, Is.EqualTo(countOfImages));
				var book = DbAccess.Select<Book>(id);
				var forResult = CreateQuery().Select.Table<Image>().In(book).ForResult(_asyncEnumeration).ToArray();
				Assert.That(forResult, Is.Not.Empty);
				Assert.That(forResult.Count, Is.EqualTo(countOfImages));
			}
		}

		[Test]
		public void Select()
		{
			DataMigrationHelper.AddUsers(250, DbAccess);

			var runPrimetivSelect = -1;
			switch (DbAccess.DbAccessType)
			{
				case DbAccessType.MsSql:
					runPrimetivSelect =
						DbAccess.RunPrimetivSelect<int>($"SELECT COUNT(1) FROM {UsersMeta.TableName}")
							[0];
					break;
			}

			if (DbAccess.DbAccessType == DbAccessType.SqLite)
			{
				runPrimetivSelect =
						(int)
						DbAccess.RunPrimetivSelect<long>($"SELECT COUNT(1) FROM {UsersMeta.TableName}")
								[0];
			}

			if (DbAccess.DbAccessType == DbAccessType.MySql)
			{
				runPrimetivSelect =
						(int)
						DbAccess.RunPrimetivSelect<long>($"SELECT COUNT(1) FROM {UsersMeta.TableName}")
								[0];
			}

			var deSelect = DbAccess.Select<Users>();
			var forResult = CreateQuery().Select.Table<Users>().ForResult(_asyncEnumeration).ToArray();

			Assert.That(runPrimetivSelect, Is.EqualTo(forResult.Count()));
			Assert.That(deSelect.Length, Is.EqualTo(forResult.Count()));

			for (var index = 0; index < forResult.Length; index++)
			{
				var userse = forResult[index];
				var userbe = deSelect[index];
				Assert.That(userse.UserID, Is.EqualTo(userbe.UserID));
				Assert.That(userse.UserName, Is.EqualTo(userbe.UserName));
			}
		}

		[Test]
		public void SelectedLimit()
		{
			var addUsers = DataMigrationHelper.AddUsers(10, DbAccess);
			var userses = CreateQuery().Select.Table<Users>().Where.PrimaryKey().Is.In(addUsers)
				.LimitBy(3)
				.ToArray();
			Assert.That(userses.Length, Is.EqualTo(3));
		}

		[Test]
		public void SelectWithFactory()
		{
			var addUsers = DataMigrationHelper.AddUsers(10, DbAccess);

			foreach (var addUser in addUsers)
			{
				var query = CreateQuery().Select.Table<Users_StaticQueryFactoryForSelectWithArugments>(addUser);
				var subQuery = CreateQuery().SubSelect(() => query, "query");
				var user = subQuery
					.Where
					.Column(f => f.UserName).Is.Not.Null
					.FirstOrDefault();
				Assert.That(user, Is.Not.Null);
				Assert.That(user.UserId, Is.EqualTo(addUser));
			}
		}

		[Test]
		public void SelectSingleColumnTest()
		{
			var addUsers = DataMigrationHelper.AddUsers(10, DbAccess);

			var userses = CreateQuery().Select.Only<Users>().Column(f => f.UserID).ForResult<long>(_asyncEnumeration).ToArray();

			Assert.That(userses.Length == addUsers.Length, Is.True);
			CollectionAssert.AreEqual(addUsers, userses);
		}

		[Test]
		public void Update()
		{
			var addUsers = DataMigrationHelper.AddUsers(1, DbAccess)[0];
			var user = DbAccess.Select<Users>(addUsers);
			var userIdPre = user.UserID;
			var usernamePre = user.UserName;
			user.UserName = Guid.NewGuid().ToString();
			CreateQuery().UpdateEntity(user).ExecuteNonQuery();
			user = DbAccess.Select<Users>(addUsers);
			Assert.That(user.UserID, Is.EqualTo(userIdPre));
			Assert.That(user.UserName, Is.Not.EqualTo(usernamePre));
		}

		[Test]
		public void UpdateExplicit()
		{
			var addUsers = DataMigrationHelper.AddUsers(1, DbAccess)[0];
			var user = DbAccess.Select<Users>(addUsers);
			var userIdPre = user.UserID;
			var usernamePre = user.UserName;
			user.UserName = Guid.NewGuid().ToString();
			CreateQuery().Update.Table<Users>().Set
					.Column(f => f.UserName).Value(user.UserName)
					.ExecuteNonQuery();
			user = DbAccess.Select<Users>(addUsers);
			Assert.That(user.UserID, Is.EqualTo(userIdPre));
			Assert.That(user.UserName, Is.Not.EqualTo(usernamePre));

			CreateQuery().Update.Table<Users>().Set
					.Column(f => f.UserName).Value(null)
					.ExecuteNonQuery();

			user = DbAccess.Select<Users>(addUsers);
			Assert.That(user.UserID, Is.EqualTo(userIdPre));
			Assert.That(user.UserName, Is.Null);
		}
	}
}