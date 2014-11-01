using System;
using System.Text;
using JPB.DataAccess.AdoWrapper.MsSql;
using JPB.DataAccess.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTestLoadXml
    {
        //public static void Main()
        //{
        //    new UnitTestLoadXml().TestMethod1();
        //}

        [TestMethod]
        public void TestMethod1()
        {
            var ConsolePropertyGrid = new ConsolePropertyGrid();
            ConsolePropertyGrid.Target = typeof(PagerTest.TestPagerTest);
            var tableName = "LoadXmlTest";
            var tableName2 = "LoadXmlTester";


            var accessLayer = new DbAccessLayer(new MsSql("Data Source=(localdb)\\Projects;Integrated Security=True;"));
            //accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("IF EXISTS (select * from sys.databases where name='TestDB')" +
            //                                                                     " DROP DATABASE TestDB"));
            //accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("CREATE DATABASE TestDB"));

            accessLayer = new DbAccessLayer(new MsSql("Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;"));

            //accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("CREATE TABLE " + tableName + " (" +
            //                                                                     " ID_test BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL," +
            //                                                                     " PropA NVARCHAR(MAX)," +
            //                                                                     " PropB NVARCHAR(MAX)" +
            //                                                                     ");"));
            //accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("CREATE TABLE " + tableName2 + " (" +
            //                                                                     " ID_test BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL," +
            //                                                                     " PropA NVARCHAR(MAX)," +
            //                                                                     " PropB NVARCHAR(MAX)" +
            //                                                                     ");"));
            //accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("DELETE FROM " + tableName));


            //Console.WriteLine("Insert 100 Rows");
            //for (int i = 0; i < 5; i++)
            //{
            //    Console.WriteLine("Row " + i);
            //    accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("INSERT INTO " + tableName + " VALUES ('Rand_' + CONVERT(NVARCHAR(MAX),RAND()), 'Rand2_' + CONVERT(NVARCHAR(MAX), NEWID()));"));
            //}

            //Console.WriteLine("Insert 100 Rows");
            //for (int i = 0; i < 5; i++)
            //{
            //    Console.WriteLine("Row " + i);
            //    accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("INSERT INTO " + tableName2 + " VALUES ('Rand_' + CONVERT(NVARCHAR(MAX),RAND()), 'Rand2_' + CONVERT(NVARCHAR(MAX), NEWID()));"));
            //}

            var loadEntrys = accessLayer.Select<LoadXmlTest>();

            //foreach (var item in loadEntrys)
            //{
            //    Console.WriteLine(item.IdTest);

            //    foreach (var loadXmlTest in item.Self)
            //    {
            //        Console.WriteLine("\t" + loadXmlTest.IdTest);

            //        foreach (var xmlTest in loadXmlTest.Self)
            //        {
            //            Console.WriteLine("\t\t" + xmlTest.IdTest);
            //        }
            //    }
            //}

            Console.ReadKey();
        }
    }
}
