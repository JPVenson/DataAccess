#region

using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using JPB.DataAccess.Tests.DbAccessLayerTests;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;

#endregion

namespace JPB.DataAccess.Tests.PagerTests
{
	[TestFixture(DbAccessType.MsSql)]
	[TestFixture(DbAccessType.SqLite)]
	public class PagerUnitTest : DatabaseBaseTest
	{
		public PagerUnitTest(DbAccessType type) : base(type)
		{
		}

		[Test]
		public void PagerCall()
		{
			var testUsers = DataMigrationHelper.AddUsers(250, DbAccess);

			var refSelect =
					DbAccess.Database.Run(
					s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.TableName)));
			Assert.That(testUsers.Length, Is.EqualTo(refSelect));

			using (var pager = DbAccess.Database.CreatePager<Users>())
			{
				Assert.That(pager, Is.Not.Null);

				pager.CurrentPage = 1;
				pager.PageSize = 25;

				Assert.That(pager.MaxPage, Is.EqualTo(0));

				#region CheckEvents

				var triggeredNewPageLoaded = false;
				var triggeredNewPageLoading = false;

				pager.NewPageLoaded += () => triggeredNewPageLoaded = true;
				pager.NewPageLoading += () => triggeredNewPageLoading = true;

				pager.LoadPage(DbAccess);

				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(triggeredNewPageLoaded, Is.False);
				Assert.That(triggeredNewPageLoading, Is.False);

				pager.RaiseEvents = true;
				pager.LoadPage(DbAccess);

				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(triggeredNewPageLoaded);
				Assert.That(triggeredNewPageLoading);

				#endregion

				#region CheckPage Size

				var oldPageSize = pager.PageSize;
				var newPageSize = 20;
				Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(oldPageSize));

				pager.PageSize = newPageSize;
				Assert.That(pager.PageSize, Is.EqualTo(newPageSize));

				pager.LoadPage(DbAccess);
				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(newPageSize));

				#endregion
			}
		}

		[Test]
		public void PagerConditionalCall()
		{
			var testUsers = DataMigrationHelper.AddUsers(250, DbAccess);

			var refSelect =
					DbAccess.Database.Run(
					s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.TableName)));
			Assert.That(testUsers.Length, Is.EqualTo(refSelect));

			using (var pager = DbAccess.Database.CreatePager<Users>())
			{
				pager.AppendedComands.Add(
				DbAccess.Database.CreateCommand(string.Format("WHERE User_ID = {0}", testUsers[0])));

				Assert.That(pager, Is.Not.Null);

				pager.CurrentPage = 1;
				pager.PageSize = 25;

				Assert.That(pager.MaxPage, Is.EqualTo(0));

				#region CheckEvents

				var triggeredNewPageLoaded = false;
				var triggeredNewPageLoading = false;

				pager.NewPageLoaded += () => triggeredNewPageLoaded = true;
				pager.NewPageLoading += () => triggeredNewPageLoading = true;

				pager.LoadPage(DbAccess);

				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(triggeredNewPageLoaded, Is.False);
				Assert.That(triggeredNewPageLoading, Is.False);

				pager.RaiseEvents = true;
				pager.LoadPage(DbAccess);

				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(triggeredNewPageLoaded);
				Assert.That(triggeredNewPageLoading);

				#endregion

				#region CheckPage Size

				var oldPageSize = pager.PageSize;
				var newPageSize = 20;
				Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(1));

				pager.PageSize = newPageSize;
				Assert.That(pager.PageSize, Is.EqualTo(newPageSize));

				pager.LoadPage(DbAccess);
				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(1));

				#endregion
			}
		}
	}
}