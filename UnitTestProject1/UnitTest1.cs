using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.AdoWrapper.MsSql;
using JPB.DataAccess.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using testing;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        //private static void Main()
        //{
        //    new UnitTest1().MsSQlTest();
        //}

        public UnitTest1()
        {
            writer = new StreamWriter("output.xml", false);
            watch = new Stopwatch();
        }

        private StreamWriter writer;
        Stopwatch watch;

        private void TraceAction(Action action, string message)
        {
            watch.Reset();

            watch.Start();

            action();

            watch.Stop();

            writer.WriteLine(message, watch.ElapsedMilliseconds);

            watch.Reset();
        }

        [TestMethod]
        public void TestMethod1()
        {
            //Clear old entrys
            var accessLayer = new DbAccessLayer(new MsSql("Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;"));
            accessLayer.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users;DELETE FROM Images;"));

            var itemsList = new List<User>();

            var count = 5000;

            for (int i = 0; i < count; i++)
            {
                itemsList.Add(new User() { Name = "testUser_" + i});
            }

            //first try normal Inserting
            
            writer.WriteLine("Test Managed Insert Range");
            TraceAction(() =>
            {
                accessLayer.InsertRange(itemsList);
            }, "Test | adding + " + count + " + Entrys done in !{0}! ms");

            writer.WriteLine("Test UnManaged Insert Range");
            TraceAction(() =>
            {
                foreach (var user in itemsList)
                {
                    accessLayer.Database.Run(s =>
                    {
                        var dbCommand = s.CreateCommand("INSERT INTO Users (UserName) VALUES (@1)") as SqlCommand;
                        dbCommand.Parameters.AddWithValue("@1", user.Name);

                        s.ExecuteNonQuery(dbCommand);
                    });
                }
            }, "Test | adding + " + count + " + Entrys done in !{0}! ms");


            accessLayer.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users;DELETE FROM Images;"));
            accessLayer.InsertRange(itemsList);

            writer.WriteLine("Test Managed Select Range");
            TraceAction(() =>
            {
                var items = accessLayer.Select<User>();
                Assert.AreEqual(items.Count, count);
            }, "Test | Selection + " + count + " + Entrys done in !{0}! ms");

            writer.WriteLine("Test Managed/Unmanged Select Range");
            TraceAction(() =>
            {
                var items = accessLayer.SelectNative(typeof(UserImpl), "SELECT * FROM Users");
                Assert.AreEqual(items.Count, count);
            }, "Test | Selection + " + count + " + Entrys done in !{0}! ms");

            writer.WriteLine("Test Unmanaged Select Range");
            TraceAction(() =>
            {
                var entitiesList = accessLayer.Database.GetEntitiesList("SELECT * FROM Users", e => new UserImpl(e), true).ToArray();
                Assert.AreEqual(entitiesList.Length, count);

            }, "Test |  Selection + " + count + " + Entrys done in !{0}! ms");

            //accessLayer.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users;DELETE FROM Images;"));

            writer.Close();
        }
    }
}
