using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.Helper.LocalDb.Constraints;
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
		private LocalDbReposetory<Image> _images;

		[SetUp]
		public void TestInit()
		{
			using (new DatabaseScope())
			{
				_images = new LocalDbReposetory<Image>(new DbConfig(), new LocalDbConstraint("TestConstraint", s =>
				{
					var item = s as Image;
					return item.IdBook < 0 && item.IdBook > 10;
				}));
				Assert.IsFalse(_images.ReposetoryCreated);
			}

			Assert.IsTrue(_images.ReposetoryCreated);
		}

		[Test]
		public void AddChildWithoutParent()
		{
			var image = new Image();
			image.IdBook = 20;
			Assert.That(() => _images.Add(image), Throws.Exception.TypeOf<ConstraintException>());
		}
	}
}
