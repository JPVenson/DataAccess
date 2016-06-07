using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Tests.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.LocalDbTests
#if MsSql
.MsSQL
#endif

#if SqLite
.SqLite
#endif
{
	[TestFixture]
	public class LocalDbWithFkTest
	{
		private LocalDbReposetory<Book> _books;
		private LocalDbReposetory<Image> _images;

		[SetUp]
		public void TestInit()
		{
			using (new DatabaseScope())
			{
				_books = new LocalDbReposetory<Book>(new DbConfig());
				_images = new LocalDbReposetory<Image>(new DbConfig());
				Assert.IsFalse(_books.ReposetoryCreated);
				Assert.IsFalse(_images.ReposetoryCreated);
			}

			Assert.IsTrue(_books.ReposetoryCreated);
			Assert.IsTrue(_images.ReposetoryCreated);
		}

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