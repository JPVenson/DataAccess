using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Helper.LocalDb.Constraints;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;
using JPB.DataAccess.Helper.LocalDb.Constraints.Defaults;
using JPB.DataAccess.Helper.LocalDb.Scopes;
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
	public class LocalDbWithConstraintsTest
	{

		public LocalDbReposetory<Image> TestInit(IEnumerable<ILocalDbCheckConstraint> checks,
			IEnumerable<ILocalDbUniqueConstraint> unique,
			IEnumerable<ILocalDbDefaultConstraint> defaults)
		{
			LocalDbReposetory<Image> images;
			using (new DatabaseScope())
			{
				images = new LocalDbReposetory<Image>(new DbConfig());
				if (checks != null)
					foreach (var localDbCheckConstraint in checks)
					{
						images.Constraints.Check.Add(localDbCheckConstraint);
					}
				if (unique != null)
					foreach (var localDbCheckConstraint in unique)
					{
						images.Constraints.Unique.Add(localDbCheckConstraint);
					}
				if (defaults != null)
					foreach (var localDbCheckConstraint in defaults)
					{
						images.Constraints.Default.Add(localDbCheckConstraint);
					}
			}
			return images;
		}

		[Test]
		public void AddCheckConstraint()
		{
			var images = TestInit(new[]{new LocalDbCheckConstraint("TestConstraint", s =>
			{
				var item = s as Image;
				return item.IdBook > 0 && item.IdBook < 10;
			})}, null, null);

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
				images.Constraints.Check.Add(new LocalDbCheckConstraint("TestConstraint", s =>
				{
					var item = s as Image;
					return item.IdBook > 0 && item.IdBook < 10;
				}));
			}, Throws.Exception.TypeOf<InvalidOperationException>());
		}


		[Test]
		public void AddUniqueConstraint()
		{
			var images = TestInit(null, new[] { new LocalDbUniqueConstraint("BookId is Unique", s => ((Image)s).IdBook), }, null);

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

			Assert.That(() =>
			{
				images.Constraints.Unique.Add(new LocalDbUniqueConstraint("BookId is Unique", s => ((Image)s).IdBook));
			}, Throws.Exception.TypeOf<InvalidOperationException>());
		}

		[Test]
		public void AddDefaultConstraint()
		{
			var images = TestInit(null, null, new[]
			{
				new LocalDbDefaultConstraint("DefaultConstraint", 666, (source, constVal) => (source as Image).IdBook = (int)constVal),
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
				new LocalDbDefaultConstraint("DefaultConstraint", 666, (source, constVal) => (source as Image).IdBook = (int)constVal));
			}, Throws.Exception.TypeOf<InvalidOperationException>());
		}

	}
}
