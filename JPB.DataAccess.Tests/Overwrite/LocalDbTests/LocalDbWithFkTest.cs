#region

using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Helper.LocalDb.Constraints;
using JPB.DataAccess.Helper.LocalDb.Scopes;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Books;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Images;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.LocalDbTests

{
	[TestFixture]
	public class LocalDbWithFkTest
	{
		[SetUp]
		public void TestInit()
		{
			using (new DatabaseScope())
			{
				_books = new LocalDbRepository<Book>(new DbConfig(true));
				_images = new LocalDbRepository<Image>(new DbConfig(true));
				_imagesNullable = new LocalDbRepository<ImageNullable>(new DbConfig(true));
				Assert.IsFalse(_books.ReposetoryCreated);
				Assert.IsFalse(_images.ReposetoryCreated);
				Assert.IsFalse(_imagesNullable.ReposetoryCreated);
			}

			Assert.IsTrue(_books.ReposetoryCreated);
			Assert.IsTrue(_images.ReposetoryCreated);
			Assert.IsTrue(_imagesNullable.ReposetoryCreated);
		}

		private LocalDbRepository<Book> _books;
		private LocalDbRepository<Image> _images;
		private LocalDbRepository<ImageNullable> _imagesNullable;

		[Test]
		public void AddChildWithoutParent()
		{
			var image = new Image();

			Assert.That(() => _images.Add(image), Throws.Exception.TypeOf<ForginKeyConstraintException>());
		}

		[Test]
		public void AddParentWithChild()
		{
			var book = new Book();
			_books.Add(book);
			Assert.IsNotNull(book);
			Assert.AreNotEqual(book.BookId, 0);
			Assert.AreEqual(book.BookId, 1);
		}

		[Test]
		public void ParentWithChild()
		{
			var book = new Book();
			_books.Add(book);

			var image = new Image();
			image.IdBook = book.BookId;

			_books.Remove(book);
			Assert.That(() => _images.Add(image), Throws.Exception.TypeOf<ForginKeyConstraintException>());
		}

		[Test]
		public void ParentWithNullableChild()
		{
			var book = new Book();
			var image = new ImageNullable();

			Assert.That(() => _books.Add(book), Throws.Nothing);
			image.IdBook = book.BookId;
			Assert.That(() => _imagesNullable.Add(image), Throws.Nothing);
		}

		[Test]
		public void ParentWithNullableChildIsNull()
		{
			var book = new Book();
			var image = new ImageNullable();

			Assert.That(() => _books.Add(book), Throws.Nothing);
			image.IdBook = null;
			Assert.That(() => _imagesNullable.Add(image), Throws.Nothing);
		}

		[Test]
		public void RemoveParentWithChild()
		{
			var book = new Book();
			_books.Add(book);

			var image = new Image();
			image.IdBook = book.BookId;
			_images.Add(image);

			Assert.AreNotEqual(book.BookId, 0);
			Assert.AreNotEqual(image.ImageId, 0);

			Assert.AreNotEqual(_books.Count, 0);
			Assert.AreNotEqual(_images.Count, 0);
			Assert.IsTrue(_images.Remove(image));
			Assert.IsTrue(_books.Remove(book));
			Assert.AreEqual(_books.Count, 0);
			Assert.AreEqual(_images.Count, 0);
		}
	}
}