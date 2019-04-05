#region

using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MetaApi.Model;
using JPB.DataAccess.ModelsAnotations;
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
	[TestFixture(DbAccessType.MsSql, true, false, true)]
	[TestFixture(DbAccessType.MsSql, true, false, false)]

	[TestFixture(DbAccessType.MsSql, false, false, true)]
	[TestFixture(DbAccessType.MsSql, false, false, false)]

	[TestFixture(DbAccessType.MsSql, false, true, true)]
	[TestFixture(DbAccessType.MsSql, false, true, false)]

	[TestFixture(DbAccessType.SqLite, false, true, true)]
	[TestFixture(DbAccessType.SqLite, false, true, false)]

	[TestFixture(DbAccessType.SqLite, false, false, true)]
	[TestFixture(DbAccessType.SqLite, false, false, false)]

	[TestFixture(DbAccessType.SqLite, true, false, true)]
	[TestFixture(DbAccessType.SqLite, true, false, false)]

	[Parallelizable(ParallelScope.Fixtures | ParallelScope.Self | ParallelScope.Children)]
	public class QueryBuilderTests : DatabaseBaseTest
	{
		private readonly bool _asyncEnumeration;

		public QueryBuilderTests(DbAccessType type, bool asyncExecution, bool syncronised, bool asyncEnumeration)
				: base(type, asyncExecution, syncronised, asyncEnumeration)
		{
			_asyncEnumeration = asyncEnumeration;
		}

		private RootQuery CreateQuery()
		{
			return DbAccess.Query();
		}

		[Test]
		public void JoinChilds()
		{
			var addBooksWithImage = DataMigrationHelper.AddBooksWithImage(250, 10, DbAccess);
			var books = Measure(() => CreateQuery()
				.Select.Table<BookWithFkImages>()
				.Join(nameof(BookWithFkImages.Images))
				.ToArray());

			Assert.That(books, Is.Not.Null);

			for (var index = 0; index < books.Length; index++)
			{
				var bookWithFkImagese = books[index];
				var addBook = addBooksWithImage[index];
				Assert.That(bookWithFkImagese.BookId, Is.EqualTo(addBook));
				Assert.That(bookWithFkImagese.Images, Is.Not.Null);
				Assert.That(bookWithFkImagese.Images.Count, Is.EqualTo(10));
				var bookOnId = DbAccess.Select<Book>(addBook);
				Assert.That(bookWithFkImagese.Text, Is.EqualTo(bookOnId.Text));

				var imagesOfThatBook = DbAccess.Query().Select.Table<Image>()
					.Where
					.Column(f => f.IdBook).Is.EqualsTo(addBook).ToArray();
				foreach (var imageWithFkBook in bookWithFkImagese.Images)
				{
					var img = imagesOfThatBook.FirstOrDefault(f => f.ImageId == imageWithFkBook.ImageId);

					Assert.That(imageWithFkBook, Is.Not.Null);
					Assert.That(imageWithFkBook.Text, Is.EqualTo(img.Text));
				}
			}
		}

		[Test]
		public void JoinParent()
		{
			DataMigrationHelper.AddBooksWithImage(250, 10, DbAccess);
			var books = Measure(() => CreateQuery()
				.Select.Table<ImageWithFkBooks>()
				.Join(nameof(ImageWithFkBooks.Book))
				.ToArray());

			foreach (var imageWithFkBookse in books)
			{
				Assert.That(imageWithFkBookse.Book, Is.Not.Null);
				Assert.That(imageWithFkBookse.Book.Text, Is.Not.Null);
				Assert.That(imageWithFkBookse.Book.BookId, Is.Not.Zero);
			}

			Assert.That(books, Is.Not.Null);
		}

		[Test]
		public void JoinMultipleParent()
		{
			DataMigrationHelper.AddBooksWithImageAndUser(250, 10, DbAccess);

			var books = Measure(() => CreateQuery()
				.Select.Table<BookWithFkImages>()
				.Join(nameof(BookWithFkImages.User))
				.Join(nameof(BookWithFkImages.User1))
				.ToArray());

			foreach (var book in books)
			{
				Assert.That(book.User, Is.Not.Null);
				Assert.That(book.User.UserName, Is.Not.Null);
				Assert.That(book.User.User_ID, Is.Not.Zero);

				Assert.That(book.User1, Is.Not.Null);
				Assert.That(book.User1.UserName, Is.Not.Null);
				Assert.That(book.User1.User_ID, Is.Not.Zero);
			}

			Assert.That(books, Is.Not.Null);
		}

		[Test]
		public void JoinInCte()
		{
			DataMigrationHelper.AddBooksWithImage(250, 10, DbAccess);
			var books = Measure(() => CreateQuery()
				.WithCte(e => e
						.Select
						.Table<ImageWithFkBooks>()
						.Join(f => f.Book),
					out var cteId)
				.Select
				.Identifier<ImageWithFkBooks>(cteId)
				.ToArray());

			foreach (var imageWithFkBookse in books)
			{
				Assert.That(imageWithFkBookse.Book, Is.Not.Null);
				Assert.That(imageWithFkBookse.Book.Text, Is.Not.Null);
				Assert.That(imageWithFkBookse.Book.BookId, Is.EqualTo(imageWithFkBookse.IdBook));
			}

			Assert.That(books, Is.Not.Null);
		}

		[Test]
		public void JoinMultipleInCteParent()
		{
			DataMigrationHelper.AddBooksWithImageAndUser(250, 10, DbAccess);

			var books = Measure(() => CreateQuery()
				.WithCte(e => e
					.Select
					.Table<BookWithFkImages>()
					.Join(f => f.User)
					.Join(f => f.User1),
					out var cteAlias)
				.Select
					.Identifier<BookWithFkImages>(cteAlias)
				.ToArray());

			foreach (var book in books)
			{
				Assert.That(book.User, Is.Not.Null);
				Assert.That(book.User.UserName, Is.Not.Null);
				Assert.That(book.User.User_ID, Is.Not.Zero);

				Assert.That(book.User1, Is.Not.Null);
				Assert.That(book.User1.UserName, Is.Not.Null);
				Assert.That(book.User1.User_ID, Is.EqualTo(book.IdUser));
			}

			Assert.That(books, Is.Not.Null);
		}
		
		[Test]
		public void JoinMultipleOnCteParent()
		{
			DataMigrationHelper.AddBooksWithImageAndUser(250, 10, DbAccess);

			var books = Measure(() => CreateQuery()
				.WithCte(e => e
					.Select.Table<BookWithFkImages>(),
					out var cteAlias)
				.Select
					.Identifier<BookWithFkImages>(cteAlias)
					.Join(nameof(BookWithFkImages.User))
					.Join(nameof(BookWithFkImages.User1))
				.ToArray());

			foreach (var book in books)
			{
				Assert.That(book.User, Is.Not.Null);
				Assert.That(book.User.UserName, Is.Not.Null);
				Assert.That(book.User.User_ID, Is.Not.Zero);

				Assert.That(book.User1, Is.Not.Null);
				Assert.That(book.User1.UserName, Is.Not.Null);
				Assert.That(book.User1.User_ID, Is.Not.Zero);
			}

			Assert.That(books, Is.Not.Null);
		}

	

		[Test]
		public void SelectJoinParentCondition()
		{
			var book = DbAccess.InsertWithSelect(new Book()
			{
				Text = "Test1"
			});
			book = DbAccess.InsertWithSelect(new Book()
			{
				Text = "Test"
			});
			var image = DbAccess.InsertWithSelect(new Image()
			{
				IdBook = book.BookId
			});

			var books = Measure(() => CreateQuery()
				.Select.Table<ImageWithFkBooks>()
				.Join(nameof(ImageWithFkBooks.Book))
				.Where
				.Column(f => f.Book.Text).Is.EqualsTo("Test")
				.ToArray()
				.FirstOrDefault());

			Assert.That(books, Is.Not.Null);
			Assert.That(books.Book.Text, Is.EqualTo("Test"));
		}

		[Test]
		public void SelectJoinParentOrder()
		{
			var book = DbAccess.InsertWithSelect(new Book()
			{
				Text = "Test1"
			});
			book = DbAccess.InsertWithSelect(new Book()
			{
				Text = "Test"
			});
			var image = DbAccess.InsertWithSelect(new Image()
			{
				IdBook = book.BookId
			});

			var books = Measure(() => CreateQuery()
				.Select.Table<ImageWithFkBooks>()
				.Join(nameof(ImageWithFkBooks.Book))
				.Order.By(e => e.Book.Text)
				.ToArray()
				.FirstOrDefault());

			Assert.That(books, Is.Not.Null);
			Assert.That(books.Book.Text, Is.EqualTo("Test"));
		}

		[Test]
		public void JoinParentAndThenChild()
		{
			DataMigrationHelper.AddBooksWithImage(250, 10, DbAccess);
			var images = Measure(() => CreateQuery()
				.Select.Table<ImageWithFkBooks>()
				.Join(f => f.Book.Images)
				.ToArray());

			Assert.That(images, Is.Not.Null);

			var allBooks = DbAccess.Select<Book>();
			var allImages = DbAccess.Select<Image>();

			foreach (var imageWithFkBookse in images)
			{
				Assert.That(imageWithFkBookse.Text, Is.Not.Null);
				Assert.That(imageWithFkBookse.Book, Is.Not.Null);

				var book = allBooks.FirstOrDefault(e => e.BookId == imageWithFkBookse.IdBook);

				Assert.That(imageWithFkBookse.Book.Text, Is.EqualTo(book.Text));

				Assert.That(imageWithFkBookse.Book.Images, Is.Not.Null);
				Assert.That(imageWithFkBookse.Book.Images, Is.Not.Empty);

				var imgs = allImages.Where(f => f.IdBook == imageWithFkBookse.IdBook);
				foreach (var image in imageWithFkBookse.Book.Images)
				{
					var img = imgs.FirstOrDefault(e => e.ImageId == image.ImageId);
					Assert.That(image.Book, Is.Null);
					Assert.That(image.Text, Is.Not.Null);
					Assert.That(image.IdBook, Is.EqualTo(imageWithFkBookse.Book.BookId));
					Assert.That(img, Is.Not.Null);
					Assert.That(img.Text, Is.EqualTo(image.Text));
				}
			}
		}

		[Test]
		public void JoinChildAndThenParent()
		{
			DataMigrationHelper.AddBooksWithImage(250, 10, DbAccess);
			var books = Measure(() => CreateQuery()
				.Select.Table<BookWithFkImages>()
				.Join(f => f.Images.Type.Book)
				.ToArray());

			Assert.That(books, Is.Not.Null);

			var allBooks = DbAccess.Select<Book>();
			var allImages = DbAccess.Select<Image>();

			foreach (var book in books)
			{
				Assert.That(book, Is.Not.Null);
				Assert.That(book.Text, Is.Not.Null);
				
				Assert.That(book.Images, Is.Not.Null);
				Assert.That(book.Images, Is.Not.Empty);

				var imgsOfBook = allImages.Where(f => f.IdBook == book.BookId);
				foreach (var image in imgsOfBook)
				{
					var imageInBook = book.Images.FirstOrDefault(f => f.ImageId == image.ImageId);
					Assert.That(imageInBook, Is.Not.Null);
					Assert.That(imageInBook.Text, Is.EqualTo(image.Text));
				}
			}
		}

		[Test]
		public void JoinEmptyParent()
		{
			DataMigrationHelper.AddBooks(250, DbAccess);
			var books = Measure(() => CreateQuery()
				.Select.Table<BookWithFkImages>()
				.Join(f => f.User)
				.Join(f => f.User1)
				.ToArray());

			Assert.That(books, Is.Not.Null);
		}

		[Test]
		public void JoinEmptyChilds()
		{
			DataMigrationHelper.AddBooks(250, DbAccess);
			var books = Measure(() => CreateQuery()
				.Select.Table<BookWithFkImages>()
				.Join(f => f.Images)
				.ToArray());

			Assert.That(books, Is.Not.Null);
		}

		[Test]
		public void JoinEmptyChildsThenParent()
		{
			DataMigrationHelper.AddBooks(250, DbAccess);
			var books = Measure(() => CreateQuery()
				.Select.Table<BookWithFkImages>()
				.Join(f => f.Images.Type.Book)
				.ToArray());

			Assert.That(books, Is.Not.Null);
		}

		[Test]
		public void JoinEmptyParentThenChildsThenParent()
		{
			DataMigrationHelper.AddBooks(250, DbAccess);
			var books = Measure(() => CreateQuery()
				.Select.Table<BookWithFkImages>()
				.Join(f => f.Images.Type.Book, JoinMode.FullOuter)
				.Join(f => f.User)
				.Join(f => f.User1)
				.ToArray());

			Assert.That(books, Is.Not.Null);
		}

		[Test]
		public void JoinEmpty()
		{
			DataMigrationHelper.AddImages(250, DbAccess);
			var books = Measure(() => CreateQuery()
				.Select.Table<ImageWithFkBooks>()
				.Join(f => f.Book, JoinMode.FullOuter)
				.ToArray());

			Assert.That(books, Is.Not.Null);
		}

		[Test]
		public void JoinParentsMultiple()
		{
			DataMigrationHelper.AddBooksWithImageAndUser(250, 10, DbAccess);

			var books = Measure(() => CreateQuery()
				.Select.Table<ImageWithFkBooks>()
				.Join(f => f.Book.User1)
				.Join(f => f.Book.User)
				.ToArray());

			Assert.That(books, Is.Not.Null);

			foreach (var imageWithFkBookse in books)
			{
				Assert.That(imageWithFkBookse.Book, Is.Not.Null);
				Assert.That(imageWithFkBookse.Book.User, Is.Not.Null);
				Assert.That(imageWithFkBookse.Book.User1, Is.Not.Null);
				Assert.That(imageWithFkBookse.Book.User.User_ID, Is.EqualTo(imageWithFkBookse.Book.User1.User_ID));
			}
		}

		[Test]
		public void JoinChildAndThenParentAndThenChilds()
		{
			DataMigrationHelper.AddBooksWithImageAndUser(250, 10, DbAccess);

			var books = Measure(() => CreateQuery()
				.Select.Table<BookWithFkImages>()
				.Join(f => f.User1)
				.Join(f => f.Images.Type.Book.Images.Type.Book)
				.Join(f => f.User)
				.ToArray());

			Assert.That(books, Is.Not.Null);

			var allBooks = DbAccess.Select<Book>();
			var allImages = DbAccess.Select<Image>();

			foreach (var book in books)
			{
				Assert.That(book, Is.Not.Null);
				Assert.That(book.Text, Is.Not.Null);
				
				Assert.That(book.Images, Is.Not.Null);
				Assert.That(book.Images, Is.Not.Empty);
				Assert.That(book.IdUser, Is.EqualTo(book.User.User_ID));

				var imgsOfBook = allImages.Where(f => f.IdBook == book.BookId);
				foreach (var image in imgsOfBook)
				{
					var imageInBook = book.Images.FirstOrDefault(f => f.ImageId == image.ImageId);
					Assert.That(imageInBook, Is.Not.Null);
					Assert.That(imageInBook.Text, Is.EqualTo(image.Text));
				}
			}
		}
		
		[Test]
		public void Count()
		{
			var addUsers = DataMigrationHelper.AddUsers(250, DbAccess);
			Assert.That(addUsers.Length, Is.EqualTo(250));

			var runPrimetivSelect = CountUsersSqlStatement();
			var forResult = -1;
			var deSelect = DbAccess.Select<Users>();
			forResult = CreateQuery().Count.Table<Users>()
				.ForResult(_asyncEnumeration)
				.FirstOrDefault();

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
			var basePager = CreateQuery()
				.Select.Table<Users>()
				.Order.By(e => e.UserID)
				.ForPagedResult(1, 10);
			basePager.LoadPage(DbAccess);

			Assert.That(basePager.CurrentPage, Is.EqualTo(1));
			Assert.That(basePager.MaxPage, Is.EqualTo(maxItems / basePager.PageSize));
		}

		[Test]
		public void PagerWithOrdering()
		{
			var maxItems = 250;

			DataMigrationHelper.AddUsers(maxItems, DbAccess);
			var basePager = CreateQuery()
				.Select.Table<Users>()
				.Order.By(e => e.UserID)
				.ForPagedResult(1, 10);
			basePager.LoadPage(DbAccess);

			Assert.That(basePager.CurrentPage, Is.EqualTo(1));
			Assert.That(basePager.MaxPage, Is.EqualTo(maxItems / basePager.PageSize));
		}

		[Test]
		public void PagerWithCondtion()
		{
			var maxItems = 250;
			var pageSize = 25;

			var addUsers = DataMigrationHelper.AddUsers(maxItems, DbAccess).Skip(pageSize).ToArray();
			var beginWithId = addUsers.FirstOrDefault();

			var queryPager = CreateQuery().Select.Table<Users>()
									 .Where
									 .Column(f => f.UserID).Is.BiggerThen(beginWithId)
									 .Or
									 .Column(f => f.UserID).Is.EqualsTo(beginWithId)
									 .Order.By(e => e.UserID)
									 .ForPagedResult(1, pageSize);
			queryPager.LoadPage(DbAccess);

			Assert.That(1, Is.EqualTo(queryPager.CurrentPage));
			Assert.That(Math.Ceiling((double)addUsers.Length / pageSize), Is.EqualTo(queryPager.MaxPage));

			for (int i = 0; i < pageSize; i++)
			{
				var queryPageItem = queryPager.CurrentPageItems.ElementAt(i);
				Assert.That(queryPageItem.UserID, Is.EqualTo(addUsers[i]));
			}
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

		private int CountUsersSqlStatement(int? imageId = null)
		{
			string query;
			query = $"SELECT COUNT(1) FROM {UsersMeta.TableName}";
			switch (DbAccess.DbAccessType)
			{
				case DbAccessType.MsSql:
					return DbAccess.RunPrimetivSelect<int>(query)[0];
				case DbAccessType.SqLite:
				case DbAccessType.MySql:
					return (int)DbAccess.RunPrimetivSelect<long>(query)[0];
			}

			return -1;
		}

		[Test]
		public void Select()
		{
			DataMigrationHelper.AddUsers(250, DbAccess);

			var countOfImages = CountUsersSqlStatement();

			var deSelect = DbAccess.Select<Users>();
			var forResult = CreateQuery().Select.Table<Users>().ForResult(_asyncEnumeration).ToArray();

			Assert.That(countOfImages, Is.EqualTo(forResult.Count()));
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
		public void SelectFkRelation()
		{
			var addBooks = DataMigrationHelper.AddBooksWithImage(10, 2, DbAccess);

			foreach (var bookId in addBooks)
			{
				var images = CreateQuery().Select.Table<Image>()
					.Where
					.ForginKey<Book>().Is.EqualsTo(bookId)
					.ToArray();
				Assert.That(images, Is.Not.Null);
				Assert.That(images.Length, Is.EqualTo(2));
				foreach (var image in images)
				{
					Assert.That(image.IdBook, Is.EqualTo(bookId));
				}
			}
		}

		[Test]
		public void SelectWithBrakets()
		{
			DataMigrationHelper.AddUsers(1, DbAccess);
			var deSelect = DbAccess.Select<Users>();
			var queryCompile = CreateQuery().Select.Table<Users>()
				.Where
				.InBracket(e => e.Column(f => f.UserName).Is.EqualsTo(deSelect[0].UserName))
				.And
				.InBracket(e => e.InBracket(f => f.Column(g => g.UserName).Is.Not.Null).And.PrimaryKey().Is.EqualsTo(1))
				.ContainerObject
				.Compile(out _);
		}

		[Test]
		public void SelectWithEscapedInConfiguration()
		{
			DataMigrationHelper.AddUsers(250, DbAccess);

			var userConfigCache = DbAccess.Config.GetOrCreateClassInfoCache(typeof(Users));
			var dbPropertyInfoCache = userConfigCache.Propertys[nameof(Users.UserName)];
			var newUserTextName = $"[{UsersMeta.ContentName}]";
			dbPropertyInfoCache.Attributes.Add(new DbAttributeInfoCache(new ForModelAttribute(newUserTextName)));
			dbPropertyInfoCache.Refresh();
			Assert.That(dbPropertyInfoCache.DbName, Is.EqualTo(newUserTextName));

			var countOfUsers = CountUsersSqlStatement();

			var deSelect = DbAccess.Select<Users>();
			var forResult = CreateQuery().Select.Table<Users>().ForResult(_asyncEnumeration).ToArray();

			Assert.That(countOfUsers, Is.EqualTo(forResult.Count()));
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
		public void SelectWithCondition()
		{
			var addUsers = DataMigrationHelper.AddUsers(2, DbAccess);
			var contentConditionValue = DbAccess.Select<Users>()
				.LastOrDefault();

			var forResult = CreateQuery().Select.Table<Users>()
				.Where.Column(f => f.UserName)
				.Is
				.EqualsTo(contentConditionValue.UserName)
				.ForResult(_asyncEnumeration)
				.ToArray()
				.FirstOrDefault();

			Assert.That(forResult.UserName, Is.EqualTo(contentConditionValue.UserName));
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
		public void SubSelect()
		{
			var addUsers = DataMigrationHelper.AddUsers(10, DbAccess);

			foreach (var addUser in addUsers)
			{
				var subQuery = CreateQuery()
					.SubSelect(() => CreateQuery().Select.Table<Users>().Where.PrimaryKey().Is.EqualsTo(addUser));
				var user = subQuery
					.Where
					.Column(f => f.UserName).Is.Not.Null
					.FirstOrDefault();
				Assert.That(user, Is.Not.Null);
				Assert.That(user.UserID, Is.EqualTo(addUser));
			}
		}

		[Test]
		public void Delete()
		{
			var addUsers = DataMigrationHelper.AddUsers(1, DbAccess)[0];
			CreateQuery().Delete<Users>().Where.Column(f => f.UserID).Is.EqualsTo(addUsers).ExecuteNonQuery();
			Assert.That(CreateQuery().Count.Table<Users>().FirstOrDefault(), Is.EqualTo(0));
		}

		[Test]
		public void Update()
		{
			var addUsers = DataMigrationHelper.AddUsers(1, DbAccess)[0];
			var user = DbAccess.Select<Users>(addUsers);
			var userIdPre = user.UserID;
			var usernamePre = user.UserName;
			user.UserName = Guid.NewGuid().ToString();
			CreateQuery().Update.Entity(user).ExecuteNonQuery();
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

			Measure(() => CreateQuery().Update.Table<Users>()
				.Set
				.Column(f => f.UserName).Value(user.UserName)
				.ExecuteNonQuery());

			user = DbAccess.Select<Users>(addUsers);
			Assert.That(user.UserID, Is.EqualTo(userIdPre));
			Assert.That(user.UserName, Is.Not.EqualTo(usernamePre));

			Measure(() => CreateQuery().Update.Table<Users>().Set
				.Column(f => f.UserName).Value(null)
				.ExecuteNonQuery());

			user = DbAccess.Select<Users>(addUsers);
			Assert.That(user.UserID, Is.EqualTo(userIdPre));
			Assert.That(user.UserName, Is.Null);
		}
	}
}