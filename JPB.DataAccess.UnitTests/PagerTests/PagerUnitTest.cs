using System;
using JPB.DataAccess.DebuggerHelper;
using JPB.DataAccess.UnitTests.TestModels;
using JPB.DataAccess.UnitTests.TestModels.CheckWrapperBaseTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JPB.DataAccess.Manager;
using System.Collections.Generic;

namespace JPB.DataAccess.UnitTests
{
	[TestClass]
	public class PagerUnitTest
	{
		DbAccessLayer expectWrapper;

		[TestInitialize]
		public void Init()
		{
			expectWrapper = new Manager().GetWrapper();
		}

		[TestMethod]
		public void PagerCall()
		{
		//this test might be fail as the cleanup can produce a lock exception. Run this test as a standalone

			expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

			var upperCountTestUsers = 100;
			var testUers = new List<Users>();

			var insGuid = Guid.NewGuid().ToString();

			for (int i = 0; i < upperCountTestUsers; i++)
			{
				testUers.Add(new Users() { UserName = insGuid });
			}

			expectWrapper.InsertRange(testUers);

			var refSelect = expectWrapper.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.UserTable)));
			Assert.AreEqual(testUers.Count, refSelect);

			using (var pager = expectWrapper.Database.CreatePager<Users>())
			{
				Assert.IsNotNull(pager);

				#region CheckEvents
				var triggeredNewPageLoaded = false;
				var triggeredNewPageLoading = false;

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

				var oldPageSize = pager.PageSize;
				var newPageSize = 20;
				Assert.AreEqual(pager.CurrentPageItems.Count, oldPageSize);

				pager.PageSize = newPageSize;
				Assert.AreEqual(pager.PageSize, newPageSize);

				pager.LoadPage(expectWrapper);
				Assert.AreEqual(pager.CurrentPageItems.Count, newPageSize);
			}


				#endregion
		}
	}
}
