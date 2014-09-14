using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MsSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using testing;

namespace UnitTestProject1
{
    [TestClass]
    public class DbAccessTest
    {
        public string testName = "TestDD";
        public static DbAccessLayer AccessLayer { get; set; }

        private static void Main()
        {
            var dbAccessTest = new DbAccessTest();
            try
            {
                dbAccessTest.TestMethod1();
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

        [TestInitialize]
        public void TestMethod1()
        {
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

            user.ID_Image = img.Id;

            User updatedUser = AccessLayer.InsertWithSelect(user);

            updatedUser.ID_Image = img.Id;
            AccessLayer.Update(updatedUser);
        }

        [TestMethod]
        public void BCheckSelects()
        {
            List<User> @select = AccessLayer.Select<User>("test");
            Assert.AreEqual(@select.Count, 3);

            var lastID = (long) AccessLayer.Database.Run(s => s.GetSkalar("SELECT TOP 1 User_ID FROM Users"));
            long count = (int) AccessLayer.Database.Run(s => s.GetSkalar("SELECT COUNT(*) FROM Users"));

            var user = AccessLayer.Select<User>(lastID);
            Assert.AreEqual(user.Name, testName);

            List<User> users = AccessLayer.SelectNative<User>("SELECT * FROM Users");
            Assert.AreEqual(users.Count, 3);

            User list =
                AccessLayer.SelectNative<User>("SELECT * FROM Users us WHERE us.User_ID = @test", new {test = lastID})
                    .FirstOrDefault();
            Assert.AreEqual(list.UserId, lastID);

            List<User> selectWhere = AccessLayer.SelectWhere<User>("AS s WHERE s.User_ID != 0");
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