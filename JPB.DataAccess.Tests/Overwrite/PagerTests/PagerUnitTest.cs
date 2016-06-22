using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;

namespace JPB.DataAccess.Tests.PagerTests
#if MsSql
.MsSQL
#endif

#if SqLite
.SqLite
#endif
{
	[TestFixture]
	public class PagerUnitTest
	{
		private DbAccessLayer expectWrapper;

		[SetUp]
		public void Init()
		{
			expectWrapper = new Manager().GetWrapper();
		}

		[Test]
		[Category("MsSQL")]
#if SqLite
		[Ignore("MsSQL only")]
#endif
		public void PagerCall()
		{
			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);

			int upperCountTestUsers = 100;
			var testUers = new List<Users>();

			string insGuid = Guid.NewGuid().ToString();

			for (int i = 0; i < upperCountTestUsers; i++)
			{
				testUers.Add(new Users { UserName = insGuid });
			}

			expectWrapper.InsertRange(testUers);

			object refSelect =
				expectWrapper.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.TableName)));
			Assert.That(testUers.Count, Is.EqualTo(refSelect));

			using (IDataPager<Users> pager = expectWrapper.Database.CreatePager<Users>())
			{
				Assert.That(pager, Is.Not.Null);

				pager.CurrentPage = 1;
				pager.PageSize = 25;

				Assert.That(pager.MaxPage, Is.EqualTo(0));

				#region CheckEvents

				bool triggeredNewPageLoaded = false;
				bool triggeredNewPageLoading = false;

				pager.NewPageLoaded += () => triggeredNewPageLoaded = true;
				pager.NewPageLoading += () => triggeredNewPageLoading = true;

				pager.LoadPage(expectWrapper);

				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(triggeredNewPageLoaded, Is.False);
				Assert.That(triggeredNewPageLoading, Is.False);

				pager.RaiseEvents = true;
				pager.LoadPage(expectWrapper);

				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(triggeredNewPageLoaded);
				Assert.That(triggeredNewPageLoading);

				#endregion

				#region CheckPage Size

				int oldPageSize = pager.PageSize;
				int newPageSize = 20;
				Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(oldPageSize));

				pager.PageSize = newPageSize;
				Assert.That(pager.PageSize, Is.EqualTo(newPageSize));

				pager.LoadPage(expectWrapper);
				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(newPageSize));

				#endregion
			}

		}

		[Test]
		[Category("MsSQL")]
#if SqLite
		[Ignore("MsSQL only")]
#endif
		public void PagerConditionalCall()
		{
			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
			expectWrapper.ExecuteGenericCommand(string.Format("TRUNCATE TABLE {0} ", UsersMeta.TableName), null);

			int upperCountTestUsers = 100;
			var testUers = new List<Users>();

			string insGuid = Guid.NewGuid().ToString();

			for (int i = 0; i < upperCountTestUsers; i++)
			{
				testUers.Add(new Users { UserName = insGuid });
			}

			expectWrapper.InsertRange(testUers);

			object refSelect =
				expectWrapper.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.TableName)));
			Assert.That(testUers.Count, Is.EqualTo(refSelect));

			using (IDataPager<Users> pager = expectWrapper.Database.CreatePager<Users>())
			{
				pager.AppendedComands.Add(expectWrapper.Database.CreateCommand("WHERE User_ID = 1"));

				Assert.That(pager, Is.Not.Null);

				pager.CurrentPage = 1;
				pager.PageSize = 25;

				Assert.That(pager.MaxPage, Is.EqualTo(0));

				#region CheckEvents

				bool triggeredNewPageLoaded = false;
				bool triggeredNewPageLoading = false;

				pager.NewPageLoaded += () => triggeredNewPageLoaded = true;
				pager.NewPageLoading += () => triggeredNewPageLoading = true;

				pager.LoadPage(expectWrapper);

				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(triggeredNewPageLoaded, Is.False);
				Assert.That(triggeredNewPageLoading, Is.False);

				pager.RaiseEvents = true;
				pager.LoadPage(expectWrapper);

				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(triggeredNewPageLoaded);
				Assert.That(triggeredNewPageLoading);

				#endregion

				#region CheckPage Size

				int oldPageSize = pager.PageSize;
				int newPageSize = 20;
				Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(1));

				pager.PageSize = newPageSize;
				Assert.That(pager.PageSize, Is.EqualTo(newPageSize));

				pager.LoadPage(expectWrapper);
				Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
				Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(1));

				#endregion
			}

		}
	}
}