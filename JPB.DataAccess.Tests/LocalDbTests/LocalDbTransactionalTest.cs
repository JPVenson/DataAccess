using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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
	public class LocalDbTransactionalTest
	{
		public LocalDbTransactionalTest()
		{
			
		}

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
			Assert.That(() =>
			{
				using (var transaction  = new TransactionScope())
				{
					Assert.That(() => _images.Add(image), Throws.Nothing);
					transaction.Complete();
				}
			}, Throws.Exception.TypeOf<ForginKeyConstraintException>());
			
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
			Assert.That(() =>
			{
				using (var transaction = new TransactionScope())
				{
					var book = new Book();
					_books.Add(book);

					var image = new Image();
					image.IdBook = book.BookId;

					_books.Remove(book);
					Assert.That(() => _images.Add(image), Throws.Nothing);
					transaction.Complete();
				}
			}, Throws.Exception.TypeOf<ForginKeyConstraintException>());

		}

		[Test]
		public void AddMultibeItems()
		{
			Assert.That(() =>
			{
				using (var transaction = new TransactionScope())
				{
					var image = new Image();
					_images.Add(image);
					var book = new Book();
					_books.Add(book);
					image.IdBook = book.BookId;

					Assert.AreNotEqual(book.BookId, 0);
					Assert.AreNotEqual(image.ImageId, 0);

					Assert.AreNotEqual(_books.Count, 0);
					Assert.AreNotEqual(_images.Count, 0);
					transaction.Complete();
				}
			}, Throws.Nothing);

		}

		[Test]
		public void IdentityInsert()
		{
			Assert.That(() =>
			{
				using (var transaction = new TransactionScope())
				{
					using (new IdentityInsertScope())
					{
						var image = new Image();
						image.ImageId = 10;
						_images.Add(image);
						var book = new Book();
						_books.Add(book);
						image.IdBook = book.BookId;

						Assert.That(book.BookId, Is.EqualTo(0));
						Assert.That(image.ImageId, Is.EqualTo(10));

						Assert.AreNotEqual(_books.Count, 0);
						Assert.AreNotEqual(_images.Count, 0);
						transaction.Complete();
					}
				}
			}, Throws.Nothing);
		}

		[Test]
		public void IdentityInsertAutoSetUninit()
		{
			Assert.That(() =>
			{
				using (var transaction = new TransactionScope())
				{
					using (new IdentityInsertScope(true))
					{
						var image = new Image();
						image.ImageId = 10;
						_images.Add(image);
						var book = new Book();
						_books.Add(book);
						image.IdBook = book.BookId;

						Assert.That(book.BookId, Is.EqualTo(1));
						Assert.That(image.ImageId, Is.EqualTo(10));

						Assert.AreNotEqual(_books.Count, 0);
						Assert.AreNotEqual(_images.Count, 0);
						transaction.Complete();
					}
				}
			}, Throws.Nothing);
		}
	}
}