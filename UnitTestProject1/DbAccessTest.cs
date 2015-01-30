using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.AdoWrapper.MsSql;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MySql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class DbAccessTest
    {
        public string testName = "TestDD";
        public static DbAccessLayer AccessLayer { get; set; }

        public static void Main()
        {
            var dbAccessTest = new DbAccessTest();
            try
            {
                //dbAccessTest.MySQlTest();
                dbAccessTest.MySQlTest();
                CleanUp();

                dbAccessTest.ACheckInserts();
                dbAccessTest.BCheckSelects();
            }
            finally
            {
                CleanUp();
            }
        }

        [ClassCleanup]
        public static void CleanUp()
        {
            AccessLayer.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users"));
        }

        public void MySQlTest()
        {

            AccessLayer = new DbAccessLayer(new MsSql(""));

            AccessLayer.CheckDatabase();

            AccessLayer.ExecuteGenericCommand(AccessLayer.Database.CreateCommand("CREATE TABLE Users (" +
                                                                                 " User_ID BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL," +
                                                                                 " UserName NVARCHAR(MAX)," +
                                                                                 ");"));

            AccessLayer.ExecuteGenericCommand(AccessLayer.Database.CreateCommand("CREATE TABLE Images (" +
                                                                     " Image_ID BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL," +
                                                                     " Content NVARCHAR(MAX)," +
                                                                     ");"));
        }

        public void MsSQlTest()
        {
            var accessLayer = new DbAccessLayer(new MsSql("Data Source=(localdb)\\Projects;Integrated Security=True;"));
            accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("IF EXISTS (select * from sys.databases where name='TestDB')" +
                                                                                 " DROP DATABASE TestDB"));
            accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("CREATE DATABASE TestDB"));

            accessLayer = new DbAccessLayer(new MsSql("Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;"));
            accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("CREATE TABLE Users (" +
                                                                                 " User_ID BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL," +
                                                                                 " UserName NVARCHAR(MAX)," +
                                                                                 ");"));

            accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("CREATE TABLE Images (" +
                                                                     " Image_ID BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL," +
                                                                     " Content NVARCHAR(MAX)," +
                                                                     ");"));

            AccessLayer = new DbAccessLayer(new MsSql("Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;"));
        }

        [TestMethod]
        public void ACheckInserts()
        {
            var user = new User();
            user.Name = testName;
            AccessLayer.Insert(user);
            user.Name += "_1";
            AccessLayer.InsertRange(new List<User> {user});
            user.Name += "_2";

            var img = new Image();
            img.Text = "BLA";
            img = AccessLayer.InsertWithSelect(img);

            ConsolePropertyGrid.RenderList(new List<Image>() { img });

            Console.ReadKey();
            
            User updatedUser = AccessLayer.InsertWithSelect(user);

            AccessLayer.Update(updatedUser);
        }

        [TestMethod]
        public void BCheckSelects()
        {
            List<User> @select = AccessLayer.Select<User>("test");

            ConsolePropertyGrid.RenderList(@select);
            Console.ReadKey();

            Assert.AreEqual(@select.Count, 3);

            var lastID = (long) AccessLayer.Database.Run(s => s.GetSkalar("SELECT TOP 1 User_ID FROM Users"));
            long count = (int) AccessLayer.Database.Run(s => s.GetSkalar("SELECT COUNT(*) FROM Users"));

            var user = AccessLayer.Select<User>(lastID);

            Assert.AreEqual(user.Name, testName);

            List<User> users = AccessLayer.SelectNative<User>("SELECT * FROM Users");
            ConsolePropertyGrid.RenderList(users);
            Console.ReadKey();


            Assert.AreEqual(users.Count, 3);

            User list =
                AccessLayer.SelectNative<User>("SELECT * FROM Users us WHERE us.User_ID = @test", new {test = lastID})
                    .FirstOrDefault();
            Assert.AreEqual(list.UserId, lastID);

            List<User> selectWhere = AccessLayer.SelectWhere<User>("AS s WHERE s.User_ID != 0");

            ConsolePropertyGrid.RenderList(selectWhere);
            Console.ReadKey();

            Assert.AreEqual(count, selectWhere.Count);

            User @where =
                AccessLayer.SelectWhere<User>("AS s WHERE s.User_ID = @testVar", new {testVar = lastID})
                    .FirstOrDefault();
            Assert.AreEqual(@where.UserId, lastID);
        }

        [TestMethod]
        public void CCheckUpdate()
        {
            List<User> @select = AccessLayer.Select<User>();

            User[] enumerable = @select.Take(3).ToArray();
            var users = new List<User>();

            foreach (User user in enumerable)
            {
                user.Name = user.Name + "_new";
                users.Add(new User {UserId = user.UserId});
            }

            foreach (User user in enumerable)
            {
                AccessLayer.Update(user);
            }

            foreach (User user in users)
            {
                AccessLayer.RefreshKeepObject(user);
            }
        }

        [TestMethod]
        public void DCheckAttributeSelect()
        {
            List<PocoUsers> @select = AccessLayer.Select<PocoUsers>();
            Assert.AreEqual(@select.Count, 3);
        }
    }
}