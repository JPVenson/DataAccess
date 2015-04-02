using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.QueryBuilder;
using JPB.DataAccess.UnitTests.TestModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestProject1;

namespace JPB.DataAccess.UnitTests
{
    [TestClass]
    public class CheckWrapperBaseTests
    {
        static DbAccessLayer expectWrapper;

        public const string ConnectionString = "Data Source=(localdb)\\ProjectsV12;Integrated Security=True;";

        [TestInitialize]
        public async void InitTest()
        {
            if (expectWrapper != null)
                return;

            var dbType = DbAccessType.MsSql;
            var dbname = "testDB";

            expectWrapper = new DbAccessLayer(dbType, ConnectionString);
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

            expectWrapper = new DbAccessLayer(dbType, string.Format(ConnectionString + "Initial Catalog={0};", dbname));

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
        public void ExecuteGenericCommand()
        {
            var resultSelect1 = expectWrapper.ExecuteGenericCommand("Select 10", null);
            Assert.AreEqual(resultSelect1, -1);

            resultSelect1 = expectWrapper.ExecuteGenericCommand("SELECT @test", new { test = 10 });
            Assert.AreEqual(resultSelect1, -1);

            resultSelect1 = expectWrapper.ExecuteGenericCommand("SELECT @test", new List<QueryParameter>() { new QueryParameter("test", 10) });
            Assert.AreEqual(resultSelect1, -1);
        }

        [TestMethod]
        public void InsertTest()
        {
            var insGuid = Guid.NewGuid().ToString();

            expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

            expectWrapper.Insert(new Users() { UserName = insGuid });
            var selectUsernameFromWhere = string.Format("SELECT UserName FROM {0}", UsersMeta.UserTable);
            var selectTest = expectWrapper.Database.Run(s => s.GetSkalar(selectUsernameFromWhere));

            Assert.IsNotNull(selectTest);
            Assert.AreEqual(selectTest, insGuid);
        }

        [TestMethod]
        public void InsertWithSelectTest()
        {
            var insGuid = Guid.NewGuid().ToString();

            expectWrapper.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.UserTable), null);

            var expectedUser = expectWrapper.InsertWithSelect(new Users() { UserName = insGuid });
            Assert.IsNotNull(expectedUser);
            Assert.AreEqual(expectedUser.UserName, insGuid);
            Assert.AreNotEqual(expectedUser.User_ID, default(long));
        }

        [TestMethod]
        public void ProcedureParamLessTest()
        {
            var expectedUser = expectWrapper.ExecuteProcedure<TestProcAParams, Users>(new TestProcAParams());

            Assert.IsNotNull(expectedUser);
            Assert.AreNotEqual(expectedUser.Count, 0);

            var refSelect = expectWrapper.Database.Run(s => s.GetSkalar(string.Format("SELECT COUNT (*) FROM {0}", UsersMeta.UserTable)));
            Assert.AreEqual(expectedUser.Count, refSelect);
        }

        [TestMethod]
        public void ProcedureParamTest()
        {
            const int procParamA = 5;

            var expectedUser =
                expectWrapper.ExecuteProcedure<TestProcBParams, Users>(new TestProcBParams()
                {
                    Number = procParamA
                });

            Assert.IsNotNull(expectedUser);
            Assert.AreNotEqual(expectedUser.Count, 0);

            var refSelect =
                expectWrapper.Database.Run(
                    s =>
                        s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0} us WHERE {1} > us.User_ID", UsersMeta.UserTable,
                            procParamA)));
            Assert.AreEqual(expectedUser.Count, refSelect);
        }


        [TestMethod]
        public void RangeInsertTest()
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
        }

        [TestMethod]
        public void InsertWithSelect()
        {
            var val = new Users { UserName = "test" };
            var refSelect = expectWrapper.InsertWithSelect(val);

            Assert.AreEqual(refSelect.UserName, val.UserName);
            Assert.AreNotEqual(refSelect.User_ID, val.User_ID);
        }

        [TestMethod]
        public void SelectBase()
        {
            var refSelect = expectWrapper.Select<Users>();
            Assert.IsTrue(refSelect.Count > 0);

            var testInsertName = Guid.NewGuid().ToString();
            var testUser = expectWrapper.InsertWithSelect(new Users() { UserName = testInsertName });
            Assert.IsNotNull(testUser);
            Assert.AreNotEqual(testUser.User_ID, default(long));

            var selTestUser = expectWrapper.Select<Users_PK_IDFM_FUNCSELECTFACWITHPARAM>(testUser.User_ID);
            Assert.AreEqual(selTestUser.UserName, testUser.UserName);
            Assert.AreEqual(selTestUser.UserId, testUser.User_ID);
        }


        [TestMethod]
        public void SelectNative()
        {
            var refSelect = expectWrapper.SelectNative<Users>(UsersMeta.SelectStatement);
            Assert.IsTrue(refSelect.Any());

            var anyId = refSelect.FirstOrDefault().User_ID;
            Assert.AreNotEqual(anyId, 0);

            refSelect =
                expectWrapper.SelectNative<Users>(UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA", new QueryParameter("paramA", anyId));
            Assert.IsTrue(refSelect.Count > 0);

            refSelect =
                expectWrapper.SelectNative<Users>(
                    UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA", new { paramA = anyId });
            Assert.IsTrue(refSelect.Count > 0);
        }

        [TestMethod]
        public void SelectPrimitivSelect()
        {
            var refSelect = expectWrapper.RunPrimetivSelect<long>(UsersMeta.SelectStatement);
            Assert.IsTrue(refSelect.Any());

            var anyId = refSelect.FirstOrDefault();
            Assert.AreNotEqual(anyId, 0);

            refSelect =
                expectWrapper.RunPrimetivSelect<long>(
                    UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA",
                    new QueryParameter("paramA", anyId));
            Assert.IsTrue(refSelect.Count > 0);

            refSelect =
                expectWrapper.RunPrimetivSelect<long>(
                    UsersMeta.SelectStatement + " WHERE " + UsersMeta.UserIDCol + " = @paramA", new { paramA = anyId });
            Assert.IsTrue(refSelect.Count > 0);
        }


        [TestMethod]
        public void SelectModelsSelect()
        {
            RangeInsertTest();
            var firstAvaibleUser = expectWrapper.Query().Select<Users>().Top(1).ForResult<Users>().First();

            var refSelect = expectWrapper.Select<Users_PK>(firstAvaibleUser.User_ID);
            Assert.IsNotNull(refSelect);

            var userSelectAlternatingProperty = expectWrapper.Select<Users_PK_IDFM>(firstAvaibleUser.User_ID);
            Assert.IsNotNull(userSelectAlternatingProperty);

            var userSelectStaticSel = expectWrapper.Select<Users_PK_IDFM_CLASSEL>(firstAvaibleUser.User_ID);
            Assert.IsNotNull(userSelectStaticSel);
        }

        [TestMethod]
        public void MarsTest()
        {
            RangeInsertTest();

            var baseQuery = expectWrapper.Query().Select(typeof(Users));
            var queryA = baseQuery.Compile();
            var queryB = baseQuery.Compile();
            Assert.IsNotNull(queryA);
            Assert.IsNotNull(queryB);

            var marsCommand = DbAccessLayer.MergeCommands(expectWrapper.Database, queryA, queryB, true);
            var returnValue = expectWrapper.ExecuteMARS(marsCommand, typeof(Users), typeof(Users));
            Assert.IsNotNull(returnValue);
            Assert.AreNotSame(returnValue.Count, 0);

            var queryAResult = returnValue.ElementAt(0);
            var queryBResult = returnValue.ElementAt(1);
            Assert.AreNotSame(queryAResult.Count, 0);
            Assert.AreEqual(queryAResult.Count, queryBResult.Count);

            var refCall = expectWrapper.Select<Users>();
            Assert.AreEqual(refCall.Count, queryAResult.Count);
        }

        [TestMethod]
        public void SyncCollectionTest()
        {
            RangeInsertTest();

            var dbCollection = expectWrapper.CreateDbCollection<Users_Col>();
            Assert.AreEqual(dbCollection.Count, 100);

            dbCollection.Add(new Users_Col());
            Assert.AreEqual(dbCollection.Count, 101);

            dbCollection.SaveChanges();
            var refAfterAdd = expectWrapper.Select<Users_Col>();
            Assert.AreEqual(refAfterAdd.Count, 101);

            dbCollection.Remove(dbCollection.First());
            Assert.AreEqual(dbCollection.Count, 100);

            dbCollection.SaveChanges();
            refAfterAdd = expectWrapper.Select<Users_Col>();
            Assert.AreEqual(refAfterAdd.Count, 100);
        }
    }
}
