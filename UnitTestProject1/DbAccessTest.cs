using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using testing;

namespace UnitTestProject1
{
    [TestClass]
    public class DbAccessTest
    {
        public static DbAccessLayer AccessLayer { get; set; }

        [ClassCleanup]
        public static void CleanUp()
        {
            AccessLayer.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users"));
        }

        [TestInitialize]
        public void TestMethod1()
        {
            AccessLayer = new DbAccessLayer(DbTypes.MsSql, "Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;");
        }

        public string testName = "TestDD";

        [TestMethod]        
        public void ACheckInserts()
        {
            var user = new User();
            user.Name = testName;
            AccessLayer.Insert(user);
            user.Name += "_1";
            AccessLayer.InsertRange(new List<User> { user });
            user.Name += "_2";

            var img = new Image();
            img.Text = "BLA";
            img = AccessLayer.InsertWithSelect(img);

            user.ID_Image = img.Id;

            var updatedUser = AccessLayer.InsertWithSelect(user);

            updatedUser.ID_Image = img.Id;
            AccessLayer.Update(updatedUser);
        }

        [TestMethod]
        public void BCheckSelects()
        {
            var @select = AccessLayer.Select<User>();
            Assert.AreEqual(@select.Count, 3);

            long lastID = (long)AccessLayer.Database.Run(s => s.GetSkalar("SELECT TOP 1 User_ID FROM Users"));
            long count = (int)AccessLayer.Database.Run(s => s.GetSkalar("SELECT COUNT(*) FROM Users"));

            var user = AccessLayer.Select<User>(lastID);
            Assert.AreEqual(user.Name, testName);

            var users = AccessLayer.SelectNative<User>("SELECT * FROM Users");
            Assert.AreEqual(users.Count, 3);

            var list = AccessLayer.SelectNative<User>("SELECT * FROM Users us WHERE us.User_ID = @test", new { test = lastID }).FirstOrDefault();
            Assert.AreEqual(list.UserId, lastID);

            var selectWhere = AccessLayer.SelectWhere<User>("AS s WHERE s.User_ID != 0");
            Assert.AreEqual(count, selectWhere.Count);

            var @where = AccessLayer.SelectWhere<User>("AS s WHERE s.User_ID = @testVar", new { testVar = lastID }).FirstOrDefault();
            Assert.AreEqual(@where.UserId, lastID);
        }

        [TestMethod]
        public void CCheckUpdate()
        {
            var @select = AccessLayer.Select<User>();

            var enumerable = @select.Take(3).ToArray();
            var users = new List<User>();

            foreach (var user in enumerable)
            {
                user.Name = user.Name + "_new";
                users.Add(new User() { UserId = user.UserId });
            }

            foreach (var user in enumerable)
            {
                AccessLayer.Update(user);
            }

            foreach (var user in users)
            {
                AccessLayer.RefreshKeepObject(user);
            }
        }

        [TestMethod()]
        public void DCheckAttributeSelect()
        {
            var @select = AccessLayer.Select<PocoUsers>();
            Assert.AreEqual(@select.Count, 3);
        }

    }
}
