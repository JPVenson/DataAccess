using System.Transactions;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Helper.LocalDb.Constraints;
using JPB.DataAccess.Helper.LocalDb.Scopes;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;

namespace JPB.DataAccess.Tests.LocalDbTests
{
    [TestFixture]
    public class LocalDbTransactionalTest
    {
        [SetUp]
        public void TestInit()
        {
            using (new DatabaseScope())
            {
                _books = new LocalDbRepository<Book>(new DbConfig());
                _images = new LocalDbRepository<Image>(new DbConfig());
                _imagesNullable = new LocalDbRepository<ImageNullable>(new DbConfig());
            }
        }

        private LocalDbRepository<Book> _books;
        private LocalDbRepository<Image> _images;
        private LocalDbRepository<ImageNullable> _imagesNullable;

        [Test]
        public void AddChildWithoutParent()
        {
            var image = new Image();
            Assert.That(() =>
            {
                using (var transaction = new TransactionScope())
                {
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
        public void AddParentWithChild()
        {
            var book = new Book();
            _books.Add(book);
            Assert.IsNotNull(book);
            Assert.AreNotEqual(book.BookId, 0);
            Assert.AreEqual(book.BookId, 1);
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
                        book.BookId = 0;
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
        public void ParentWithNullableChild()
        {
            Assert.That(() =>
            {
                using (var transaction = new TransactionScope())
                {
                    var book = new Book();
                    var image = new ImageNullable();

                    Assert.That(() => _books.Add(book), Throws.Nothing);
                    image.IdBook = book.BookId;
                    Assert.That(() => _imagesNullable.Add(image), Throws.Nothing);
                    transaction.Complete();
                }
            }, Throws.Nothing);
        }

        [Test]
        public void ParentWithNullableChildIsNull()
        {
            Assert.That(() =>
            {
                using (var transaction = new TransactionScope())
                {
                    var book = new Book();
                    var image = new ImageNullable();

                    Assert.That(() => _books.Add(book), Throws.Nothing);
                    image.IdBook = null;
                    Assert.That(() => _imagesNullable.Add(image), Throws.Nothing);
                    transaction.Complete();
                }
            }, Throws.Nothing);
        }

        [Test]
        public void ParentWithNullableChildIsNullWithPkInsert()
        {
            Assert.That(() =>
            {
                using (var transaction = new TransactionScope())
                {
                    using (new IdentityInsertScope())
                    {
                        var book = new Book();
                        var image = new ImageNullable();

                        Assert.That(() => _books.Add(book), Throws.Nothing);
                        image.IdBook = null;
                        Assert.That(() => _imagesNullable.Add(image), Throws.Nothing);
                        transaction.Complete();
                    }
                }
            }, Throws.Nothing);
        }

        [Test]
        public void ParentWithNullableChildWithPkInsert()
        {
            Assert.That(() =>
            {
                using (var transaction = new TransactionScope())
                {
                    using (new IdentityInsertScope())
                    {
                        var book = new Book();
                        var image = new ImageNullable();

                        Assert.That(() => _books.Add(book), Throws.Nothing);
                        image.IdBook = book.BookId;
                        Assert.That(() => _imagesNullable.Add(image), Throws.Nothing);
                        transaction.Complete();
                    }
                }
            }, Throws.Nothing);
        }
    }
}