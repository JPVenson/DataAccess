#region

using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Helper.LocalDb.Constraints;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;
using JPB.DataAccess.Helper.LocalDb.Constraints.Defaults;
using JPB.DataAccess.Helper.LocalDb.Scopes;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.LocalDbTests

{
	[TestFixture]
	public class LocalDbWithConstraintsTest
	{
		public LocalDbRepository<Image> TestInit(IEnumerable<ILocalDbCheckConstraint<Image>> checks,
			IEnumerable<ILocalDbUniqueConstraint<Image>> unique,
			IEnumerable<ILocalDbDefaultConstraint<Image>> defaults)
		{
			LocalDbRepository<Image> images;
			using (new DatabaseScope())
			{
				images = new LocalDbRepository<Image>(new DbConfig(true));
				if (checks != null)
				{
					foreach (var localDbCheckConstraint in checks)
					{
						images.Constraints.Check.Add(localDbCheckConstraint);
					}
				}
				if (unique != null)
				{
					foreach (var localDbCheckConstraint in unique)
					{
						images.Constraints.Unique.Add(localDbCheckConstraint);
					}
				}
				if (defaults != null)
				{
					foreach (var localDbCheckConstraint in defaults)
					{
						images.Constraints.Default.Add(localDbCheckConstraint);
					}
				}
			}
			return images;
		}

		[Test]
		public void AddCheckConstraint()
		{
			var images = TestInit(new[]
			{
				new LocalDbCheckConstraint<Image>("TestConstraint", s =>
				{
					var item = s;
					return item.IdBook > 0 && item.IdBook < 10;
				})
			}, null, null);

			var image = new Image();
			image.IdBook = 20;
			Assert.That(() => images.Add(image), Throws.Exception.TypeOf<ConstraintException>());
			image.IdBook = 9;
			Assert.That(() => images.Add(image), Throws.Nothing);
			Assert.That(images.Count, Is.EqualTo(1));
		}

		[Test]
		public void AddCheckConstraintAfter()
		{
			var images = TestInit(null, null, null);

			Assert.That(() =>
			{
				images.Constraints.Check.Add(new LocalDbCheckConstraint<Image>("TestConstraint", s =>
				{
					var item = s;
					return item.IdBook > 0 && item.IdBook < 10;
				}));
			}, Throws.Exception.TypeOf<InvalidOperationException>());
		}

		[Test]
		public void AddDefaultConditionalConstraint()
		{
			var images = TestInit(null, null, new[]
			{
				new LocalDbDefaultConstraintEx<Image, int>(new DbConfig(true), "DefaultConstraint", () => 666,
					source => source.IdBook)
			});

			var image = new Image();
			image.IdBook = 20;
			Assert.That(() => images.Add(image), Throws.Nothing);
			Assert.That(() => image.IdBook, Is.EqualTo(20));

			var img2 = new Image();
			Assert.That(() => images.Add(img2), Throws.Nothing);
			Assert.That(() => img2.IdBook, Is.EqualTo(666));
		}

		[Test]
		public void AddDefaultConstraint()
		{
			var images = TestInit(null, null, new[]
			{
				new LocalDbDefaultConstraint<Image, int>("DefaultConstraint", 666,
					(source, constVal) => source.IdBook = constVal)
			});

			var image = new Image();
			image.IdBook = 20;
			Assert.That(() => images.Add(image), Throws.Nothing);
			Assert.That(() => image.IdBook, Is.EqualTo(666));
		}

		[Test]
		public void AddDefaultConstraintAfter()
		{
			var images = TestInit(null, null, null);

			Assert.That(() =>
			{
				images.Constraints.Default.Add(
					new LocalDbDefaultConstraint<Image, int>("DefaultConstraint", 666,
						(source, constVal) => source.IdBook = constVal));
			}, Throws.Exception.TypeOf<InvalidOperationException>());
		}

		[Test]
		public void AddObjectCheckConstraint()
		{
			var images = TestInit(new[]
			{
				new LocalDbCheckConstraint<object>("TestConstraint", s =>
				{
					var item = s as Image;
					return item.IdBook > 0 && item.IdBook < 10;
				})
			}, null, null);

			var image = new Image();
			image.IdBook = 20;
			Assert.That(() => images.Add(image), Throws.Exception.TypeOf<ConstraintException>());
			image.IdBook = 9;
			Assert.That(() => images.Add(image), Throws.Nothing);
			Assert.That(images.Count, Is.EqualTo(1));
		}


		[Test]
		public void AddUniqueConstraint()
		{
			var images = TestInit(null,
				new[] {new LocalDbUniqueConstraint<Image, int>("BookId is Unique", s => s.IdBook)}, null);

			var image = new Image();
			image.IdBook = 20;
			Assert.That(() => images.Add(image), Throws.Nothing);
			var sec = new Image();
			sec.IdBook = 20;

			Assert.That(() => images.Add(sec), Throws.Exception.TypeOf<ConstraintException>());
			Assert.That(images.Count, Is.EqualTo(1));
		}

		[Test]
		public void AddUniqueConstraintAfter()
		{
			var images = TestInit(null, null, null);

			Assert.That(
				() =>
				{
					images.Constraints.Unique.Add(new LocalDbUniqueConstraint<Image, int>("BookId is Unique",
						s => s.IdBook));
				}, Throws.Exception.TypeOf<InvalidOperationException>());
		}
	}
}