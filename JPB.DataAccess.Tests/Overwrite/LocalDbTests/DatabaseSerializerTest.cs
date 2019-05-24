#region

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Serialization;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Helper.LocalDb.Scopes;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.LocalDbTests
{
	[TestFixture]
	public class DatabaseSerializerTest
	{
		[SetUp]
		public void Setup()
		{
			AllTestContextHelper.TestSetup(null);
		}

		private string _booksWithImagesDatabaseDump;
		private string _usersDatabaseDump;


		private string BooksWithImagesDatabaseDump()
		{
			LocalDbRepository<Book> books;
			LocalDbRepository<Image> images;
			using (new DatabaseScope())
			{
				books = new LocalDbRepository<Book>(new DbConfig(true));
				images = new LocalDbRepository<Image>(new DbConfig(true));
				Assert.IsFalse(books.ReposetoryCreated);
				Assert.IsFalse(images.ReposetoryCreated);
			}

			for (var i = 0; i < 3; i++)
			{
				var book = new Book();
				books.Add(book);

				var image = new Image();
				image.IdBook = book.BookId;
				images.Add(image);
			}

			var serializableContent = books.Database.GetSerializableContent();
			var xmlSer = new XmlSerializer(typeof(DataContent));
			using (var memStream = new MemoryStream())
			{
				xmlSer.Serialize(memStream, serializableContent);
				var content = Encoding.ASCII.GetString(memStream.ToArray());
				Assert.That(content, Is.Not.Null.And.Not.Empty);
				return content;
			}
		}

		private string UsersDatabaseDump()
		{
			LocalDbRepository<Users> users;
			using (new DatabaseScope())
			{
				users = new LocalDbRepository<Users>(new DbConfig(true));
				Assert.IsFalse(users.ReposetoryCreated);
			}

			for (var i = 0; i < 3; i++)
			{
				var user = new Users();
				users.Add(user);
			}

			var serializableContent = users.Database.GetSerializableContent();
			var xmlSer = new XmlSerializer(typeof(DataContent));
			using (var memStream = new MemoryStream())
			{
				xmlSer.Serialize(memStream, serializableContent);
				var content = Encoding.ASCII.GetString(memStream.ToArray());
				Assert.That(content, Is.Not.Null.And.Not.Empty);
				return content;
			}
		}

		private void Scope_SetupDone1(object sender, EventArgs e)
		{
			using (var memStream = new MemoryStream(Encoding.ASCII.GetBytes(_usersDatabaseDump)))
			{
				new XmlSerializer(typeof(DataContent)).Deserialize(memStream);
			}
		}

		private void Scope_SetupDone(object sender, EventArgs e)
		{
			using (
				var memStream = new MemoryStream(Encoding.ASCII.GetBytes(_booksWithImagesDatabaseDump)))
			{
				new XmlSerializer(typeof(DataContent)).Deserialize(memStream);
			}
		}

		[Test, Ignore("Structure has changed, test must be updated")]
		public void ReadBooksWithImages()
		{
			_booksWithImagesDatabaseDump = BooksWithImagesDatabaseDump();

			LocalDbRepository<Book> books;
			LocalDbRepository<Image> images;
			using (var scope = new DatabaseScope())
			{
				var config = new DbConfig(true);
				books = new LocalDbRepository<Book>(config);
				images = new LocalDbRepository<Image>(config);
				scope.SetupDone += Scope_SetupDone;
			}

			Assert.That(books.Count, Is.EqualTo(3));
			Assert.That(images.Count, Is.EqualTo(3));
			CollectionAssert.AllItemsAreInstancesOfType(books, typeof(Book));
			CollectionAssert.AllItemsAreNotNull(books);
			CollectionAssert.AllItemsAreUnique(books);

			CollectionAssert.AllItemsAreInstancesOfType(images, typeof(Image));
			CollectionAssert.AllItemsAreNotNull(images);
			CollectionAssert.AllItemsAreUnique(images);
			Assert.That(books, Is.All.Property("BookId").Not.EqualTo(0).And.All.Property("BookName").Null);
			Assert.That(images, Is.All.Property("ImageId").Not.EqualTo(0)
				.And.All.Property("Text").Null
				.And.All.Property("IdBook").Not.EqualTo(0));
		}

		[Test]
		public void ReadUsers()
		{
			_usersDatabaseDump = UsersDatabaseDump();
			LocalDbRepository<Users> users;
			using (var scope = new DatabaseScope())
			{
				users = new LocalDbRepository<Users>(new DbConfig(true));

				scope.SetupDone += Scope_SetupDone1;
			}

			Assert.That(users.Count, Is.EqualTo(3));
			Assert.That(users.ElementAt(0), Is.Not.Null.And.Property("UserID").EqualTo(1));
			Assert.That(users.ElementAt(1), Is.Not.Null.And.Property("UserID").EqualTo(2));
			Assert.That(users.ElementAt(2), Is.Not.Null.And.Property("UserID").EqualTo(3));
		}

		[Test]
		public void TestInvalidReplicationScope_Nested()
		{
			Assert.That(() =>
			{
				using (var scope = new DatabaseScope())
				{
					using (var transactionScope = new TransactionScope())
					{
						using (var replicationScope = new ReplicationScope())
						{
							using (var replicationScope1 = new ReplicationScope())
							{
							}
						}
					}
				}
			}, Throws.Exception.TypeOf<InvalidOperationException>());
		}

		[Test]
		public void TestInvalidReplicationScope_WithoutTransaction()
		{
			Assert.That(() =>
			{
				using (var scope = new DatabaseScope())
				{
					using (var replicationScope = new ReplicationScope())
					{
					}
				}
			}, Throws.Exception.TypeOf<InvalidOperationException>());
		}

		[Test]
		public void WriteBooksWithImages()
		{
			LocalDbRepository<Book> books;
			LocalDbRepository<Image> images;
			using (new DatabaseScope())
			{
				books = new LocalDbRepository<Book>(new DbConfig(true));
				images = new LocalDbRepository<Image>(new DbConfig(true));
				Assert.IsFalse(books.ReposetoryCreated);
				Assert.IsFalse(images.ReposetoryCreated);
			}

			for (var i = 0; i < 3; i++)
			{
				var book = new Book();
				books.Add(book);

				var image = new Image();
				image.IdBook = book.BookId;
				images.Add(image);
			}

			var serializableContent = books.Database.GetSerializableContent();
			var xmlSer = new XmlSerializer(typeof(DataContent));
			using (var memStream = new MemoryStream())
			{
				xmlSer.Serialize(memStream, serializableContent);
				var content = Encoding.ASCII.GetString(memStream.ToArray());
				Assert.That(content, Is.Not.Null.And.Not.Empty);
				memStream.Seek(0, SeekOrigin.Begin);

				LocalDbRepository<Book> booksN;
				LocalDbRepository<Image> imagesN;
				using (new DatabaseScope())
				{
					booksN = new LocalDbRepository<Book>(new DbConfig(true));
					imagesN = new LocalDbRepository<Image>(new DbConfig(true));
					Assert.IsFalse(booksN.ReposetoryCreated);
					Assert.IsFalse(imagesN.ReposetoryCreated);
					using (new TransactionScope())
					{
						using (new ReplicationScope())
						{
							xmlSer.Deserialize(memStream);
						}
					}
				}
			}
		}

		[Test]
		public void WriteUsers()
		{
			LocalDbRepository<Users> users;
			using (new DatabaseScope())
			{
				users = new LocalDbRepository<Users>(new DbConfig(true));
			}

			users.Add(new Users());
			users.Add(new Users());
			users.Add(new Users());

			var serializableContent = users.Database.GetSerializableContent();
			var xmlSer = new XmlSerializer(typeof(DataContent));
			using (var memStream = new MemoryStream())
			{
				xmlSer.Serialize(memStream, serializableContent);
				var content = Encoding.ASCII.GetString(memStream.ToArray());
				Assert.That(content, Is.Not.Null.And.Not.Empty);
			}
		}
	}
}