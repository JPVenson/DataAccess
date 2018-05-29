#region

using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.DbAccessLayerTests.PagerTests

{
	[TestFixture(DbAccessType.MsSql)]
	[TestFixture(DbAccessType.SqLite)]
	public class PagerConstraintTests : DatabaseBaseTest
	{
		public PagerConstraintTests(DbAccessType type) : base(type)
		{
		}

		[Test]
		public void CurrentPageBiggerOrEqualsOne()
		{
			var dataPager = DbAccess.Database.CreatePager<Users>();
			Assert.That(() => dataPager.CurrentPage, Is.GreaterThanOrEqualTo(1));
			Assert.That(() => dataPager.PageSize, Is.GreaterThanOrEqualTo(1));
			Assert.That(() => dataPager.CurrentPage = 0, Throws.Exception);
		}
	}
}