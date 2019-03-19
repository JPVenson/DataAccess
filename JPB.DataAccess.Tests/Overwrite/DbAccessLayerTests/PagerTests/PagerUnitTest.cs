#region

using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.DbAccessLayerTests.PagerTests
{
	public class PagerUnitTest : DatabaseBaseTest
	{
		/// <inheritdoc />
		public PagerUnitTest(DbAccessType type, bool asyncExecution, bool syncronised) : base(type,
			asyncExecution, syncronised)
		{
		}

		[Test]
		public void PagerCall()
		{
			var testUsers = DataMigrationHelper.AddUsers(250, DbAccess);

			var refSelect =
				int.Parse(DbAccess.Database.Run(
					s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.TableName))).ToString());
			Assert.That(testUsers.Length, Is.EqualTo(refSelect));

			using (var pager = DbAccess.Query().Select.Table<Users>().ForPagedResult(1, 25))
			{
				Assert.That(pager, Is.Not.Null);

				pager.CurrentPage = 1;
				pager.PageSize = 25;

				Assert.That(pager.MaxPage, Is.EqualTo(Math.Ceiling((int)refSelect / 25D)));

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

			using (var pager = DbAccess.Query().Select.Table<Users>().Where.Column(f => f.UserID).Is
				.EqualsTo(testUsers[0]).ForPagedResult(1, 1))
			{
				Assert.That(pager, Is.Not.Null);

				pager.CurrentPage = 1;
				pager.PageSize = 25;

				Assert.That(pager.MaxPage, Is.EqualTo(1));

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