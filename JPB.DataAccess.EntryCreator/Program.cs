using System;
using System.Data;
using System.Data.Sql;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using JPB.DataAccess.EntityCreator.MsSql;
using System.Xml.Serialization;
using System.Collections.Generic;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using TextDataFormat = System.Windows.TextDataFormat;

namespace JPB.DataAccess.EntityCreator
{
    public class Program
    {
        public static Options Options { get; set; }
        private static List<string> _op = new List<string>();
        static int index = 0;
        public static string GetNextOption()
        {
            if (Options == null)
                return SetNextOption();
            if (Options.Actions.Length > index)
                return Options.Actions[index++];
            return Console.ReadLine();
        }

        public static string SetNextOption()
        {
            var action = Console.ReadLine();
            _op.Add(action);
            return action;
        }

        public static void SetNextOption(string op)
        {
            _op.Add(op);
        }

        [STAThread]
        static void Main(string[] args)
        {
            var prog = new Program();

            string outputDir = "";
            string connectionString = "";

            if (args.Count() == 1)
            {
                if (File.Exists(args[0]))
                    using (var fs = new FileStream(args[0], FileMode.Open))
                    {
                        var serilizer = new XmlSerializer(typeof(Options));
                        Options = serilizer.Deserialize(fs) as Options;
                    }
            }

            Console.WriteLine("Enter output dir");

            outputDir = GetNextOption();
            if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
            {
                Console.WriteLine("Invalid Directory ...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine(@"Enter Connection string or type \explore to search for a server [ToBeSupported]");
            if (Clipboard.ContainsText() && Options == null)
            {
                var maybeConnection = Clipboard.GetText(TextDataFormat.Text);
                var strings = maybeConnection.Split(';');
                var any = strings.Any(s => s.ToLower().Contains("data source=") || s.ToLower().Contains("initial catalog="));
                if (any)
                {
                    Console.WriteLine("Use clipboard content?");
                    var consoleKeyInfo = Console.ReadKey();
                    if (char.ToLower(consoleKeyInfo.KeyChar) == 'y')
                    {
                        connectionString = maybeConnection;
                        SetNextOption(connectionString);

                    }
                    else
                    {
                        connectionString = string.Empty;
                    }
                }
                else
                {
                    connectionString = string.Empty;
                }
            }
            else
            {
                connectionString = string.Empty;
            }
            if (string.IsNullOrEmpty(connectionString))
                do
                {
                    connectionString = GetNextOption();
                    if (connectionString == @"\explore")
                    {
                        SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
                        Console.WriteLine("Search for data Sources in current network");

                        var table = instance.GetDataSources();
                        Console.WriteLine("Row count {0}", table.Rows.Count);

                        foreach (var column in table.Columns.Cast<DataColumn>())
                        {
                            Console.Write(column.ColumnName + "|");
                        }

                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            var row = table.Rows[i];

                            Console.Write("o {0} |", i);

                            foreach (System.Data.DataColumn col in table.Columns)
                            {
                                Console.Write(" {0} = {1} |", col.ColumnName, row[col]);
                            }
                            Console.WriteLine("============================");
                        }
                        Console.WriteLine();
                    }
                } while (string.IsNullOrEmpty(connectionString) || connectionString.ToLower().Contains(@"\explore"));


            new MsSqlCreator().CreateEntrys(connectionString, outputDir, string.Empty);
            if (Options == null)
                using (var fs = new FileStream("out.xml", FileMode.OpenOrCreate))
                {
                    var serilizer = new XmlSerializer(typeof(Options));

                    serilizer.Serialize(fs, new Options() { Actions = _op.ToArray() });
                }
        }

        private void RenderHelp()
        {
            Console.WriteLine("Creates cs Entrys based on a Database ( currently MsSQL only )");
            Console.WriteLine();
            Console.WriteLine("Usage: [/out] [/con] ");
            Console.WriteLine();
            Console.WriteLine("\t /out     A existing Directory where all cs classes are written to");
            Console.WriteLine("\t /con       Specifys the Connection property to a existing Database");
        }
    }
}
