#region

using JPB.DataAccess.Framework.Manager;
using JPB.DataAccess.Tests.Base;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests.PagerTests
{
	[TestFixture(DbAccessType.SqLite)]
	[TestFixture(DbAccessType.MsSql)]
	public class PagerConstraintTests : DatabaseBaseTest
	{

		[Test]
		public void CurrentPageBiggerOrEqualsOne()
		{
			var dataPager = DbAccess.Database.CreatePager<Users>();
			Assert.That(() => dataPager.CurrentPage, Is.GreaterThanOrEqualTo(1));
			Assert.That(() => dataPager.PageSize, Is.GreaterThanOrEqualTo(1));
			Assert.That(() => dataPager.CurrentPage = 0, Throws.Exception);
		}

		/// <inheritdoc />
		public PagerConstraintTests(DbAccessType type) : base(type, false, false)
		{
		}
	}
}