/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
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
	public struct f
	{

	}

	public class Program
	{
		public static f X()
		{
			return default(f);
		}

		public static AutoConsole AutoConsole { get; set; }
		[STAThread]
		static void Main(string[] args)
		{
			var prog = new Program();

			string outputDir = "";
			string connectionString = "";

			if (args.Count() == 1)
			{
				if (File.Exists(args[0]))
					AutoConsole = new AutoConsole(args[0]);
			}

			Console.WriteLine("Enter output dir");

			outputDir = AutoConsole.GetNextOption();
			if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
			{
				Console.WriteLine("Invalid Directory ...");
				Console.ReadKey();
				return;
			}
			Console.WriteLine(@"Enter Connection string or type \explore to search for a server [ToBeSupported]");
			if (Clipboard.ContainsText() && AutoConsole.Options == null)
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
						AutoConsole.SetNextOption(connectionString);

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
					connectionString = AutoConsole.GetNextOption();
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
			if (AutoConsole.Options == null)
				AutoConsole.SaveStorage(args[0]);

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
