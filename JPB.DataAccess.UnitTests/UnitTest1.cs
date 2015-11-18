//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using JPB.DataAccess.Manager;
//using UnitTestProject1;

//namespace JPB.DataAccess.UnitTests
//{
//    [TestClass]
//    public class QueryBuilderTest
//    {
//        static DbAccessLayer expectWrapper;

//        [TestInitialize]
//        public async void InitTest()
//        {
//            if (expectWrapper != null)
//                return;

//            var dbType = DbAccessType.MsSql;
//            var dbname = "testDB";

//            expectWrapper = new DbAccessLayer(dbType, ConnectionString);
//            Assert.AreEqual(expectWrapper.DbAccessType, dbType);

//            var checkDatabase = expectWrapper.CheckDatabase();
//            Assert.IsTrue(checkDatabase);

//            checkDatabase = await expectWrapper.CheckDatabaseAsync();
//            Assert.IsTrue(checkDatabase);

//            var redesginDatabase = string.Format(
//                "IF EXISTS (select * from sys.databases where name=\'{0}\') DROP DATABASE {0}",
//                dbname);

//            expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(redesginDatabase));
//            expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand(string.Format("CREATE DATABASE {0}", dbname)));

//            expectWrapper = new DbAccessLayer(dbType, string.Format(CheckWrapperBaseTests.ConnectionString + "Initial Catalog={0};", dbname));

//            expectWrapper.ExecuteGenericCommand(
//                expectWrapper.Database.CreateCommand(
//                    string.Format(
//                        "CREATE TABLE {0} ( {1} BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL, {2} NVARCHAR(MAX));",
//                        UsersMeta.UserTable, UsersMeta.UserIDCol, UsersMeta.UserNameCol)));

//            expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand("CREATE PROC TestProcA " +
//                                                                                     "AS BEGIN " +
//                                                                                     "SELECT * FROM Users " +
//                                                                                     "END"));

//            expectWrapper.ExecuteGenericCommand(expectWrapper.Database.CreateCommand("CREATE PROC TestProcB @bigThen INT " +
//                                                                          "AS BEGIN " +
//                                                                          "SELECT * FROM Users us WHERE @bigThen > us.User_ID " +
//                                                                          "END "));

//        }

//        [TestMethod]
//        public void QueryBuilderTesta()
//        {
//            var query = expectWrapper.Query();
//            Assert.IsNotNull(query);    
//        }
//    }
//}
