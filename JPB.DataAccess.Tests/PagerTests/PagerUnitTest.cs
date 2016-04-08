using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;

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
			//this test might be fail as the cleanup can produce a lock exception. Run this test as a standalone

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			int upperCountTestUsers = 100;
			var testUers = new List<Users>();

			string insGuid = Guid.NewGuid().ToString();

			for (int i = 0; i < upperCountTestUsers; i++)
			{
				testUers.Add(new Users {UserName = insGuid});
			}

			expectWrapper.InsertRange(testUers);

			object refSelect =
				expectWrapper.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.UserTable)));
			Assert.AreEqual(testUers.Count, refSelect);

			using (IDataPager<Users> pager = expectWrapper.Database.CreatePager<Users>())
			{
				Assert.IsNotNull(pager);

				pager.CurrentPage = 1;
				pager.PageSize = 25;

				#region CheckEvents

				bool triggeredNewPageLoaded = false;
				bool triggeredNewPageLoading = false;

				pager.NewPageLoaded += () => triggeredNewPageLoaded = true;
				pager.NewPageLoading += () => triggeredNewPageLoading = true;

				pager.LoadPage(expectWrapper);

				Assert.IsFalse(triggeredNewPageLoaded);
				Assert.IsFalse(triggeredNewPageLoading);

				pager.RaiseEvents = true;
				pager.LoadPage(expectWrapper);

				Assert.IsTrue(triggeredNewPageLoaded);
				Assert.IsTrue(triggeredNewPageLoading);

				#endregion

				#region CheckPage Size

				int oldPageSize = pager.PageSize;
				int newPageSize = 20;
				Assert.AreEqual(pager.CurrentPageItems.Count, oldPageSize);

				pager.PageSize = newPageSize;
				Assert.AreEqual(pager.PageSize, newPageSize);

				pager.LoadPage(expectWrapper);
				Assert.AreEqual(pager.CurrentPageItems.Count, newPageSize);

				#endregion
			}

		}
	}
}