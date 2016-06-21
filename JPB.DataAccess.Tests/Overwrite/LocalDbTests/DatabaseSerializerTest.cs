using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Helper.LocalDb.Scopes;
using JPB.DataAccess.Tests.Base;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;

namespace JPB.DataAccess.Tests.LocalDbTests
#if MsSql
.MsSQL
#endif

#if SqLite
.SqLite
#endif
{
	[TestFixture]
	public class DatabaseSerializerTest
	{
		[Test]
		public void WriteUsers()
		{
			LocalDbRepository<Users> users;
			using (new DatabaseScope())
			{
				users = new LocalDbRepository<Users>(new DbConfig());
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

		[Test]
		public void ReadUsers()
		{
			LocalDbRepository<Users> users;
			using (var scope = new DatabaseScope())
			{
				users = new LocalDbRepository<Users>(new DbConfig());

				scope.SetupDone += Scope_SetupDone1;
			}

			Assert.That(users.Count, Is.EqualTo(3));
			Assert.That(users.ElementAt(0), Is.Not.Null.And.Property("UserID").EqualTo(1));
			Assert.That(users.ElementAt(1), Is.Not.Null.And.Property("UserID").EqualTo(2));
			Assert.That(users.ElementAt(2), Is.Not.Null.And.Property("UserID").EqualTo(3));
		}

		private void Scope_SetupDone1(object sender, EventArgs e)
		{
			using (var memStream = new MemoryStream(Encoding.ASCII.GetBytes(DbLoaderResouces.UsersInDatabaseDump)))
			{
				new XmlSerializer(typeof(DataContent)).Deserialize(memStream);
			}
		}

		[Test]
		public void WriteBooksWithImages()
		{
			LocalDbRepository<Book> books;
			LocalDbRepository<Image> images;
			using (new DatabaseScope())
			{
				books = new LocalDbRepository<Book>(new DbConfig());
				images = new LocalDbRepository<Image>(new DbConfig());
				Assert.IsFalse(books.ReposetoryCreated);
				Assert.IsFalse(images.ReposetoryCreated);
			}

			for (int i = 0; i < 3; i++)
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
			}
		}

		[Test]
		public void ReadBooksWithImages()
		{
			LocalDbRepository<Book> books;
			LocalDbRepository<Image> images;
			using (var scope = new DatabaseScope())
			{
				books = new LocalDbRepository<Book>(new DbConfig());
				images = new LocalDbRepository<Image>(new DbConfig());

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

		private void Scope_SetupDone(object sender, EventArgs e)
		{
			using (var memStream = new MemoryStream(Encoding.ASCII.GetBytes(DbLoaderResouces.BooksWithImagesDatabaseDump)))
			{
				new XmlSerializer(typeof(DataContent)).Deserialize(memStream);
			}
		}
	}
}
