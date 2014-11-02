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

namespace UnitTestProject1
{
    [TestClass]
    public class PerformanceTest
    {
        //public static void Main()
        //{
        //    new PerformanceTest().TestMethod1();
        //}

        public PerformanceTest()
        {
            writer = new StreamWriter(output, false);
            watch = new Stopwatch();
        }

        private StreamWriter writer;
        Stopwatch watch;
        private const string output = "output.xml";

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

            //accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("CREATE TABLE Users (" +
            //                                                                     " User_ID BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL," +
            //                                                                     " UserName NVARCHAR(MAX)," +
            //                                                                     ");"));

            //accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("CREATE TABLE Images (" +
            //                                                         " Image_ID BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL," +
            //                                                         " Content NVARCHAR(MAX)," +
            //                                                         ");"));

            var itemsList = new List<User>();

            var count = 500;

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

            writer.WriteLine("Test POCO serialization over {0} items", count);
            TraceAction(() =>
            {
                var items = accessLayer.Select<User>();
                Assert.AreEqual(items.Count, count);
            }, "Test | Selection + " + count + " + Entrys done in !{0}! ms");

            writer.WriteLine();
            writer.WriteLine("Test POCO serialization over {0} items with static Select Statement", count);
            TraceAction(() =>
            {
                var items = accessLayer.SelectNative(typeof(User), "SELECT * FROM Users");
                Assert.AreEqual(items.Count, count);
            }, "Test | Selection + " + count + " + Entrys done in !{0}! ms");

            writer.WriteLine();
            writer.WriteLine("Test POCO serialization over {0} items with static Select Statement and Static Factory", count);
            TraceAction(() =>
            {
                var items = accessLayer.SelectNative(typeof(UserImpl), "SELECT * FROM Users");
                Assert.AreEqual(items.Count, count);
            }, "Test | Selection + " + count + " + Entrys done in !{0}! ms");

            writer.WriteLine();
            writer.WriteLine("POCO serialization over {0} items with static Select Statement and Static Factory", count);
            TraceAction(() =>
            {
                var entitiesList = accessLayer.Database.GetEntitiesList("SELECT * FROM Users", e => new UserImpl(e), true).ToArray();
                Assert.AreEqual(entitiesList.Length, count);
            }, "Test |  Selection + " + count + " + Entrys done in !{0}! ms");

            //accessLayer.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users;DELETE FROM Images;"));


            writer.Close();
            Console.WriteLine(File.ReadAllText(output));
            Console.ReadLine();
        }
    }
}
