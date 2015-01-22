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
using JPB.DataAccess.QueryBuilder;

namespace UnitTestProject1
{
    public class PerformanceTest
    {
        public static void Main()
        {



            new PerformanceTest().TestMethod1();
        }

        public PerformanceTest()
        {
            Console.WriteLine("Hello World");
            FileStream fs = new FileStream(output, FileMode.Create);
            // First, save the standard output.
            //writer = new StreamWriter(fs);
            //Console.SetOut(writer);
            //Console.WriteLine("Hello file");


            //writer = new StreamWriter(output, false);
            //Console.WriteLine("Start");
            //_textWriter = Console.Out;
            //Console.SetOut(writer);
            //Console.WriteLine("Start");
            //writer.Flush();
            watch = new Stopwatch();
        }

        private StreamWriter writer;
        Stopwatch watch;
        //private TextWriter _textWriter;
        private const string output = "output.xml";

        private void TraceAction(Action action, string message)
        {
            watch.Reset();
            watch.Start();

            action();

            watch.Stop();

            Console.WriteLine(message, watch.ElapsedMilliseconds);

            watch.Reset();
        }

        public void TestMethod1()
        {
            //Clear old entrys
            var accessLayer = new DbAccessLayer(new MsSql("Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;"));
            //accessLayer.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users;DELETE FROM Images;"));

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
                itemsList.Add(new User() { Name = "testUser_" + i });
            }

            //Create the cache

            accessLayer.Select<User>();

            Console.WriteLine("Test Managed Insert Range");
            TraceAction(() =>
            {
                accessLayer.InsertRange(itemsList);
            }, "Test | adding + " + count + " + Entrys done in !{0}! ms");

            Console.WriteLine("Test UnManaged Insert Range");
            TraceAction(() =>
            {
                accessLayer.Database.RunInTransaction(s =>
                {
                    foreach (var user in itemsList)
                    {
                        var dbCommand = s.CreateCommand("INSERT INTO Users (UserName) VALUES (@1)") as SqlCommand;
                        dbCommand.Parameters.AddWithValue("@1", user.Name);
                        s.ExecuteNonQuery(dbCommand);
                    }
                });
            }, "Test | adding + " + count + " + Entrys done in !{0}! ms");

            accessLayer.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users;DELETE FROM Images;"));
            accessLayer.InsertRange(itemsList);

            Console.WriteLine("Test POCO serialization over {0} items", count);
            TraceAction(() =>
            {
                var items = accessLayer.Select<User>();
            }, "Test | Selection + " + count + " + Entrys done in !{0}! ms");

            Console.WriteLine();
            Console.WriteLine("Test POCO serialization over {0} items with static Select Statement", count);
            TraceAction(() =>
            {
                var items = accessLayer.SelectNative(typeof(User), "SELECT * FROM Users");
            }, "Test | Selection + " + count + " + Entrys done in !{0}! ms");

            Console.WriteLine();
            Console.WriteLine("Test POCO serialization over {0} items with static Select Statement and Static Factory", count);
            TraceAction(() =>
            {
                var items = accessLayer.SelectNative(typeof(UserImpl), "SELECT * FROM Users");
            }, "Test | Selection + " + count + " + Entrys done in !{0}! ms");

            Console.WriteLine();
            Console.WriteLine("POCO serialization over {0} items with static Select Statement and Static Factory", count);
            TraceAction(() =>
            {
                var entitiesList = accessLayer.Database.GetEntitiesList("SELECT * FROM Users", e => new UserImpl(e), true).ToArray();
            }, "Test |  Selection + " + count + " + Entrys done in !{0}! ms");

            //accessLayer.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users;DELETE FROM Images;"));
            //writer.Close();
            Console.ReadLine();

            foreach (var item in accessLayer.Query().Select(typeof(Image)))
            {

            }
        }
    }
}
