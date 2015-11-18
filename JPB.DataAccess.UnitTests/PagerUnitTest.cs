using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JPB.DataAccess.Manager;
using UnitTestProject1;
using System.Collections.Generic;

namespace JPB.DataAccess.UnitTests
{
    [TestClass]
    public class PagerUnitTest
    {

        static DbAccessLayer expectWrapper;

        [TestInitialize]
        public async void InitTest()
        {
            if (expectWrapper != null)
                return;

            var dbType = DbAccessType.MsSql;
            var dbname = "testDB";

            expectWrapper = new DbAccessLayer(dbType, CheckWrapperBaseTests.SConnectionString);
            Assert.AreEqual(expectWrapper.DbAccessType, dbType);

            var checkDatabase = expectWrapper.CheckDatabase();
            Assert.IsTrue(checkDatabase);

            checkDatabase = await expectWrapper.CheckDatabaseAsync();
            Assert.IsTrue(checkDatabase);

            var redesginDatabase = string.Format(
                "IF EXISTS (select * from sys.databases where name=\'{0}\') DROP DATABASE {0}",
                dbname);

            expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(redesginDatabase));
            expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(string.Format("CREATE DATABASE {0}", dbname)));

            expectWrapper = new DbAccessLayer(dbType, string.Format(CheckWrapperBaseTests.SConnectionString + "Initial Catalog={0};", dbname));

            expectWrapper.ExecuteGenericCommand(
                expectWrapper.Database.CreateCommand(
                    string.Format(
                        "CREATE TABLE {0} ( {1} BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL, {2} NVARCHAR(MAX));",
                        UsersMeta.UserTable, UsersMeta.UserIDCol, UsersMeta.UserNameCol)));

            expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand("CREATE PROC TestProcA " +
                                                                                     "AS BEGIN " +
                                                                                     "SELECT * FROM Users " +
                                                                                     "END"));

            expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand("CREATE PROC TestProcB @bigThen INT " +
                                                                          "AS BEGIN " +
                                                                          "SELECT * FROM Users us WHERE @bigThen > us.User_ID " +
                                                                          "END "));

        }


        [TestMethod]
        public void BasicTest()
        {
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
            
            var pager = expectWrapper.Database.CreatePager<Users>();
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
            
            #endregion
        }
    }
}
