using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.AdoWrapper.MsSql;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Pager.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using testing;

namespace UnitTestProject1
{
    [TestClass]
    public class PagerTest
    {
        public static void Main()
        {
            var test = new PagerTest();
            test.TestMethod1();
        }

        [ForModel("PagerTest")]
        public class TestPagerTest
        {
            [PrimaryKey]
            public long ID_test { get; set; }

            public string PropA { get; set; }
            public string PropB { get; set; }
        }

        public IUnGenericDataPager DataPager { get; set; }

      
        public class TypeWrapper
        {
            public TypeWrapper(Type type)
            {
                GlobId++;
                ID = GlobId;
                Type = type.FullName;
            }

            private static int GlobId { get; set; }

            public int ID { get; set; }
            public string Type { get; private set; }
        }

        public List<TypeWrapper> types = new List<TypeWrapper>();

        [TestMethod]
        public void TestMethod1()
        {
            ConsolePropertyGrid = new ConsolePropertyGrid();
            ConsolePropertyGrid.Target = typeof(TestPagerTest);
            types.Add(new TypeWrapper(ConsolePropertyGrid.Target));
            types.Add(new TypeWrapper(typeof(User)) );

            var accessLayer = new DbAccessLayer(new MsSql("Data Source=(localdb)\\Projects;Integrated Security=True;"));
            accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("IF EXISTS (select * from sys.databases where name='TestDB')" +
                                                                                 " DROP DATABASE TestDB"));
            accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("CREATE DATABASE TestDB"));

            accessLayer = new DbAccessLayer(new MsSql("Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;"));
            accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("CREATE TABLE PagerTest (" +
                                                                                 " ID_test BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL," +
                                                                                 " PropA NVARCHAR(MAX)," +
                                                                                 " PropB NVARCHAR(MAX)" +
                                                                                 ");"));

            Console.WriteLine("Insert 100 Rows");
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine("Row " + i);
                accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("INSERT INTO PagerTest VALUES ('Rand_' + CONVERT(NVARCHAR(MAX),RAND()), 'Rand2_' + CONVERT(NVARCHAR(MAX), NEWID()));"));
            }

            Console.WriteLine("Insert 100 Rows");
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine("Row " + i + 100);
                accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("INSERT INTO PagerTest VALUES ('Rand_' + CONVERT(NVARCHAR(MAX),RAND()), 'Rand2_' + CONVERT(NVARCHAR(MAX), NEWID()));"));
            }
            Console.WriteLine("Insert 100 Rows");

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine("Row " + i + 200);
                accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("INSERT INTO PagerTest VALUES ('Rand_' + CONVERT(NVARCHAR(MAX),RAND()), 'Rand2_' + CONVERT(NVARCHAR(MAX), NEWID()));"));
            }
            Console.Clear();
            Console.WriteLine("Inserting done");
            DataPager = accessLayer.Database.CreateUntypedPager();
            DataPager.TargetType = typeof(TestPagerTest);
            DataPager.CurrentPage = 1;
            DataPager.LoadPage(accessLayer);
            ConsolePropertyGrid.SourceList = DataPager.CurrentPageItems.ToList();
            var input = "";
            ConsolePropertyGrid.RenderGrid();

            while (input != "exit")
            {
                var consoleKeyInfo = Console.ReadKey();
                input = consoleKeyInfo.KeyChar.ToString();

                if (input == "n")
                {
                    if (DataPager.CurrentPageItems.Count < DataPager.PageSize)
                    {
                        DataPager.LoadPage(accessLayer);
                        ConsolePropertyGrid.SourceList = DataPager.CurrentPageItems.ToList();
                        ConsolePropertyGrid.ExtraInfos.Append("last page reached ...");
                        ConsolePropertyGrid.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
                        ConsolePropertyGrid.RenderGrid();
                        continue;
                    }

                    DataPager.CurrentPage++;
                    DataPager.LoadPage(accessLayer);
                    ConsolePropertyGrid.SourceList = DataPager.CurrentPageItems.ToList();
                    if (DataPager.CurrentPageItems.Count == 0)
                    {
                        DataPager.CurrentPage--;
                        DataPager.LoadPage(accessLayer);
                        ConsolePropertyGrid.SourceList = DataPager.CurrentPageItems.ToList();
                        ConsolePropertyGrid.ExtraInfos.Append("last page reached ...");
                    }
                    ConsolePropertyGrid.ExtraInfos.Append("Page: " + DataPager.CurrentPage);

                    ConsolePropertyGrid.RenderGrid();
                    continue;
                }
                if (input == "m")
                {
                    if (DataPager.CurrentPage == 0)
                    {
                        ConsolePropertyGrid.ExtraInfos.AppendLine("First page reached ...");
                        ConsolePropertyGrid.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
                    }
                    else
                    {
                        DataPager.CurrentPage--;
                        DataPager.LoadPage(accessLayer);
                        ConsolePropertyGrid.SourceList = DataPager.CurrentPageItems.ToList();
                        ConsolePropertyGrid.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
                    }

                    ConsolePropertyGrid.RenderGrid();
                    continue;
                }

                if (input == @"?")
                {
                    Console.Clear();
                    Console.WriteLine("Any input will be converted to lower chars");
                    Console.WriteLine("n - Page + 1");
                    Console.WriteLine("m - Page - 1");
                    Console.WriteLine("a [Number] - Page size = Number");
                    Console.WriteLine("c [Number] - Change type to type with number = Number");
                    Console.WriteLine("c - get types with number");
                    Console.WriteLine("[Number] - Page = Number");
                    continue;
                }

                input = input + Console.ReadLine();

                if (input.StartsWith("a"))
                {
                    var nr = input.Substring(1, input.Length - 1);
                    nr = nr.Trim();

                    int pageSize;

                    var npageSize = int.TryParse(nr, out pageSize);
                    if (npageSize)
                    {
                        DataPager.PageSize = pageSize;
                        DataPager.LoadPage(accessLayer);
                        ConsolePropertyGrid.SourceList = DataPager.CurrentPageItems.ToList();
                        ConsolePropertyGrid.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
                    }
                }
                else if (input.StartsWith("c"))
                {
                    var nr = input.Substring(1, input.Length - 1);
                    nr = nr.Trim();

                    int pageSize;

                    var npageSize = int.TryParse(nr, out pageSize);
                    if (npageSize)
                    {
                        try
                        {
                            ConsolePropertyGrid.Target = Type.GetType(this.types[pageSize].Type);
                            DataPager.TargetType = ConsolePropertyGrid.Target;
                            ConsolePropertyGrid.SourceList.Clear();
                            DataPager.CurrentPage = 0;
                            DataPager.LoadPage(accessLayer);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    else
                    {
                        var objects = ConsolePropertyGrid.SourceList.ToArray();
                        ConsolePropertyGrid.SourceList.Clear();
                        var oldTarget = ConsolePropertyGrid.Target;
                        ConsolePropertyGrid.Target = typeof(TypeWrapper);
                        foreach (var typeWrapper in this.types)
                        {
                            ConsolePropertyGrid.SourceList.Add(typeWrapper);
                        }

                        ConsolePropertyGrid.RenderGrid();
                        ConsolePropertyGrid.SourceList = objects.ToList();
                        ConsolePropertyGrid.Target = oldTarget;
                        continue;
                    }
                }
                else
                {
                    int pageNumer;

                    var tryParse = int.TryParse(input, out pageNumer);

                    if (tryParse)
                    {
                        DataPager.CurrentPage = pageNumer;
                        DataPager.LoadPage(accessLayer);
                        ConsolePropertyGrid.SourceList = DataPager.CurrentPageItems.ToList();
                        ConsolePropertyGrid.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
                    }
                }
                ConsolePropertyGrid.RenderGrid();
            }
        }


        public ConsolePropertyGrid ConsolePropertyGrid { get; set; }
    }
}
