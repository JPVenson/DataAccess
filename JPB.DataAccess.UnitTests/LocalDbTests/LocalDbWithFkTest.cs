using System;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.UnitTests.TestModels.CheckWrapperBaseTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JPB.DataAccess.UnitTests.LocalDbTests
{
	[TestClass]
	public class LocalDbWithFkTest
	{
		public LocalDbWithFkTest()
		{

		}

		private LocalDbReposetory<Book> _books;
		private LocalDbReposetory<Image> _images;

		[TestInitialize]
		public void TestInit()
		{
			using (new DatabaseScope())
			{
				_books = new LocalDbReposetory<Book>();
				_images = new LocalDbReposetory<Image>();
				Assert.IsFalse(_books.ReposetoryCreated);
				Assert.IsFalse(_images.ReposetoryCreated);
			}

			Assert.IsTrue(_books.ReposetoryCreated);
			Assert.IsTrue(_images.ReposetoryCreated);
		}

		[TestMethod]
		public void AddParentWithChild()
		{
			var book = new Book();
			_books.Add(book);
			Assert.IsNotNull(book);
			Assert.AreNotEqual(book.BookId, 0);
			Assert.AreEqual(book.BookId, 1);
		}

		[TestMethod]
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

		[TestMethod]
		[ExpectedException(typeof(ForginKeyConstraintException))]
		public void AddChildWithoutParent()
		{
			var image = new Image();
			_images.Add(image);
		}

		[TestMethod]
		[ExpectedException(typeof(ForginKeyConstraintException))]
		public void ParentWithChild()
		{
			var book = new Book();
			_books.Add(book);

			var image = new Image();
			image.IdBook = book.BookId;

			_books.Remove(book);
			_images.Add(image);
		}
	}
}
