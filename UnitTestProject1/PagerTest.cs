using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using JPB.DataAccess.AdoWrapper.MsSql;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Pager.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            [IgnoreReflection]
            public string PropB12 { get; set; }
            [IgnoreReflection]
            public string PropB3 { get; set; }
            [IgnoreReflection]
            public string PropB12313 { get; set; }
        }

        public IDataPager DataPager { get; set; }

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

        public void TestMethod1()
        {

            var contoler = new ConsoleGridControler();
            contoler.ConsoleGrid.Target = typeof(TestPagerTest);
            contoler.ConsoleGrid.RenderSum = true;
            contoler.ConsoleGrid.RenderRowNumber = true;
            contoler.ConsoleGrid.ObserveList = false;
            contoler.ConsoleGrid.RenderTypeName = false;
            contoler.ConsoleGrid.ClearConsole = false;
            contoler.ConsoleGrid.SourceList = new ObservableCollection<object>();

            types.Add(new TypeWrapper(contoler.ConsoleGrid.Target));
            types.Add(new TypeWrapper(typeof(User)));

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

            //Console.WriteLine("Insert 100 Rows");
            //for (int i = 0; i < 100; i++)
            //{
            //    Console.WriteLine("Row " + i + 100);
            //    accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("INSERT INTO PagerTest VALUES ('Rand_' + CONVERT(NVARCHAR(MAX),RAND()), 'Rand2_' + CONVERT(NVARCHAR(MAX), NEWID()));"));
            //}
            //Console.WriteLine("Insert 100 Rows");

            //for (int i = 0; i < 100; i++)
            //{
            //    Console.WriteLine("Row " + i + 200);
            //    accessLayer.ExecuteGenericCommand(accessLayer.Database.CreateCommand("INSERT INTO PagerTest VALUES ('Rand_' + CONVERT(NVARCHAR(MAX),RAND()), 'Rand2_' + CONVERT(NVARCHAR(MAX), NEWID()));"));
            //}

            Console.Clear();
            Console.WriteLine("Inserting done");
            DataPager = accessLayer.Database.CreatePager<TestPagerTest>();
            DataPager.CurrentPage = 1;
            DataPager.PageSize = 15;
            DataPager.LoadPage(accessLayer); 
            foreach (var source in DataPager.CurrentPageItems)
            {
                contoler.ConsoleGrid.SourceList.Add(source);
            }
            var input = "";

            contoler.Commands.Add(new DelegateCommand("exit", s =>
            {
                contoler.StopDispatcherLoop = true;
            }));

            contoler.Commands.Add(new DelegateCommand("n", s =>
            {
                if (DataPager.CurrentPageItems.Cast<object>().Count<object>() < DataPager.PageSize)
                {
                    DataPager.LoadPage(accessLayer);

                    contoler.ConsoleGrid.SourceList.Clear();
                    foreach (var source in DataPager.CurrentPageItems)
                    {
                        contoler.ConsoleGrid.SourceList.Add(source);
                    }
                    contoler.ConsoleGrid.ExtraInfos.Append("last page reached ...");
                    contoler.ConsoleGrid.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
                    return true;
                }

                DataPager.CurrentPage++;
                DataPager.LoadPage(accessLayer);
                var items = DataPager.CurrentPageItems.Cast<object>().ToArray();
                contoler.ConsoleGrid.ClearConsole = contoler.ConsoleGrid.SourceList.Count != items.Length;
                contoler.ConsoleGrid.SourceList.Clear();
                foreach (var source in items)
                {
                    contoler.ConsoleGrid.SourceList.Add(source);
                }
                if (!items.Any())
                {
                    DataPager.CurrentPage--;
                    DataPager.LoadPage(accessLayer); 
                    contoler.ConsoleGrid.SourceList.Clear();
                    foreach (var source in items)
                    {
                        contoler.ConsoleGrid.SourceList.Add(source);
                    }
                    contoler.ConsoleGrid.ExtraInfos.Append("last page reached ...");
                }
                contoler.ConsoleGrid.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
                return true;
            }));

            contoler.Commands.Add(new DelegateCommand("m", s =>
            {
                if (DataPager.CurrentPage == 0)
                {
                    contoler.ConsoleGrid.ExtraInfos.Append("First page reached ...");
                    contoler.ConsoleGrid.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
                }
                else
                {
                    DataPager.CurrentPage--;
                    DataPager.LoadPage(accessLayer);
                    var items = DataPager.CurrentPageItems.Cast<object>().ToArray<object>();
                    contoler.ConsoleGrid.ClearConsole = contoler.ConsoleGrid.SourceList.Count != items.Length;
                    contoler.ConsoleGrid.SourceList.Clear();
                    foreach (var source in items)
                    {
                        contoler.ConsoleGrid.SourceList.Add(source);
                    }
                    contoler.ConsoleGrid.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
                }
            }));

            contoler.Run();

            //while (input != "exit")
            //{
            //    var consoleKeyInfo = Console.ReadKey();
            //    input = consoleKeyInfo.KeyChar.ToString();

            //    if (input == "n")
            //    {
            //        if (DataPager.CurrentPageItems.Cast<object>().Count<object>() < DataPager.PageSize)
            //        {
            //            DataPager.LoadPage(accessLayer);

            //            GridControl.SourceList.Clear();
            //            foreach (var source in DataPager.CurrentPageItems)
            //            {
            //                GridControl.SourceList.Add(source);
            //            }
            //            GridControl.ExtraInfos.Append("last page reached ...");
            //            GridControl.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
            //            GridControl.RenderGrid();
            //            continue;
            //        }

            //        DataPager.CurrentPage++;
            //        DataPager.LoadPage(accessLayer); GridControl.SourceList.Clear();
            //        foreach (var source in DataPager.CurrentPageItems)
            //        {
            //            GridControl.SourceList.Add(source);
            //        }
            //        if (!DataPager.CurrentPageItems.Cast<object>().Any())
            //        {
            //            DataPager.CurrentPage--;
            //            DataPager.LoadPage(accessLayer); GridControl.SourceList.Clear();
            //            foreach (var source in DataPager.CurrentPageItems)
            //            {
            //                GridControl.SourceList.Add(source);
            //            }
            //            GridControl.ExtraInfos.Append("last page reached ...");
            //        }
            //        GridControl.ExtraInfos.Append("Page: " + DataPager.CurrentPage);

            //        GridControl.RenderGrid();
            //        continue;
            //    }
            //    if (input == "m")
            //    {
            //        if (DataPager.CurrentPage == 0)
            //        {
            //            GridControl.ExtraInfos.Append("First page reached ...");
            //            GridControl.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
            //        }
            //        else
            //        {
            //            DataPager.CurrentPage--;
            //            DataPager.LoadPage(accessLayer);
            //            GridControl.SourceList.Clear();
            //            foreach (var source in DataPager.CurrentPageItems)
            //            {
            //                GridControl.SourceList.Add(source);
            //            }
            //            GridControl.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
            //        }

            //        GridControl.RenderGrid();
            //        continue;
            //    }

            //    if (input == @"?")
            //    {
            //        Console.Clear();
            //        Console.WriteLine("Any input will be converted to lower chars");
            //        Console.WriteLine("n - Page + 1");
            //        Console.WriteLine("m - Page - 1");
            //        Console.WriteLine("a [Number] - Page size = Number");
            //        Console.WriteLine("c [Number] - Change type to type with number = Number");
            //        Console.WriteLine("c - get types with number");
            //        Console.WriteLine("[Number] - Page = Number");
            //        continue;
            //    }

            //    input = input + Console.ReadLine();

            //    if (input.StartsWith("a"))
            //    {
            //        var nr = input.Substring(1, input.Length - 1);
            //        nr = nr.Trim();

            //        int pageSize;

            //        var npageSize = int.TryParse(nr, out pageSize);
            //        if (npageSize)
            //        {
            //            DataPager.PageSize = pageSize;
            //            DataPager.LoadPage(accessLayer);
            //            GridControl.SourceList.Clear();
            //            foreach (var source in DataPager.CurrentPageItems)
            //            {
            //                GridControl.SourceList.Add(source);
            //            }
            //            GridControl.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
            //        }
            //    }
            //    else if (input.StartsWith("c"))
            //    {
            //        var nr = input.Substring(1, input.Length - 1);
            //        nr = nr.Trim();

            //        int pageSize;

            //        var npageSize = int.TryParse(nr, out pageSize);
            //        if (npageSize)
            //        {
            //            try
            //            {
            //                GridControl.Target = Type.GetType(this.types[pageSize].Type);
            //                GridControl.SourceList.Clear();
            //                DataPager.CurrentPage = 0;
            //                DataPager.LoadPage(accessLayer);
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine(ex);
            //            }
            //        }
            //        else
            //        {
            //            var objects = GridControl.SourceList.ToArray();
            //            GridControl.SourceList.Clear();
            //            var oldTarget = GridControl.Target;
            //            GridControl.Target = typeof(TypeWrapper);
            //            foreach (var typeWrapper in this.types)
            //            {
            //                GridControl.SourceList.Add(typeWrapper);
            //            }

            //            GridControl.RenderGrid();
            //            GridControl.SourceList.Clear();
            //            foreach (var o in objects)
            //            {
            //                GridControl.SourceList.Add(o);
            //            }
            //            GridControl.Target = oldTarget;
            //            continue;
            //        }
            //    }
            //    else
            //    {
            //        int pageNumer;

            //        var tryParse = int.TryParse(input, out pageNumer);

            //        if (tryParse)
            //        {
            //            DataPager.CurrentPage = pageNumer;
            //            DataPager.LoadPage(accessLayer); 
            //            GridControl.SourceList.Clear();
            //            foreach (var source in DataPager.CurrentPageItems)
            //            {
            //                GridControl.SourceList.Add(source);
            //            }
            //            GridControl.ExtraInfos.Append("Page: " + DataPager.CurrentPage);
            //        }
            //    }
            //    GridControl.RenderGrid();
            //}
        }
    }
}
