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
using System.Reflection;
using System.Threading;
using JPB.Console.Helper.Grid;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using TextDataFormat = System.Windows.TextDataFormat;
using WinConsole = System.Console;

namespace JPB.DataAccess.EntityCreator
{
	public class Program
	{
		public static AutoConsole AutoConsole { get; set; }
		[STAThread]
		static void Main(string[] args)
		{
			var prog = new Program();

			var pathToCommandSet = "";
			string connectionString = "";

			if (args.Count() == 1)
			{
				pathToCommandSet = args[0];
				if (File.Exists(pathToCommandSet))
				{
					AutoConsole = new AutoConsole(pathToCommandSet);

					var version = Assembly.GetExecutingAssembly().GetName().Version;
					if (AutoConsole.Options.Version == null || new Version(AutoConsole.Options.Version) != version)
					{
						new StringBuilderInterlaced()
							.ForgroundColor(ConsoleColor.Yellow)
							.AppendLine("Warning")
							.AppendLine(string.Format("The current Entity Creator version ({0}) is not equals the version ({1}) you have provided.", version, AutoConsole.Options.Version))
							.AppendLine("There might be errors or unexpected Behavor")
							.WriteToConsole();
					}
				}
				else
				{
					AutoConsole = new AutoConsole(null);
				}
			}
			else
			{
				AutoConsole = new AutoConsole(null);
			}
			
			WinConsole.WriteLine(@"Enter Connection string or type \explore to search for a server [Only MSSQL supported]");
			if (Clipboard.ContainsText() && AutoConsole.Options == null)
			{
				var maybeConnection = Clipboard.GetText(TextDataFormat.Text);
				var strings = maybeConnection.Split(';');
				var any = strings.Any(s => s.ToLower().Contains("data source=") || s.ToLower().Contains("initial catalog="));
				if (any)
				{
					WinConsole.WriteLine("Use clipboard content? [(y|Enter*) | no]");
					var WinConsoleKeyInfo = System.Console.ReadKey();
					if (char.ToLower(WinConsoleKeyInfo.KeyChar) == 'y' || WinConsoleKeyInfo.Key == ConsoleKey.Enter)
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
						WinConsole.WriteLine("Search for data Sources in current network");

						var table = instance.GetDataSources();
						WinConsole.WriteLine("Row count {0}", table.Rows.Count);

						foreach (var column in table.Columns.Cast<DataColumn>())
						{
							WinConsole.Write(column.ColumnName + "|");
						}

						for (int i = 0; i < table.Rows.Count; i++)
						{
							var row = table.Rows[i];

							WinConsole.Write("o {0} |", i);

							foreach (DataColumn col in table.Columns)
							{
								WinConsole.Write(" {0} = {1} |", col.ColumnName, row[col]);
							}
							WinConsole.WriteLine("============================");
						}
						WinConsole.WriteLine();
					}
				} while (string.IsNullOrEmpty(connectionString) || connectionString.ToLower().Contains(@"\explore"));

			try
			{
				new MsSqlCreator().CreateEntrys(connectionString, "", string.Empty);
			}
			catch (Exception e)
			{
				if (AutoConsole.Options == null)
				{
					new StringBuilderInterlaced().
						AppendLine("Error while executing the MsSQLEntity Creator:")
						.Up()
						.AppendInterlacedLine(e.ToString(), ConsoleColor.Red)
						.Down()
						.AppendLine("Press any key to stop the application")
						.WriteToConsole();

					WinConsole.ReadKey(true);
				}
				else
				{
					new StringBuilderInterlaced().
						AppendLine("Error while executing the MsSQLEntity Creator:")
						.Up()
						.AppendInterlacedLine(e.ToString(), ConsoleColor.Red)
						.Down()
						.AppendLine("Application will shutdown in 5 seconds")
						.WriteToConsole();
					Thread.Sleep(5000);
				}

				return;
			}
			
			if (AutoConsole.Options == null && pathToCommandSet != null)
			{
				AutoConsole.SaveStorage(pathToCommandSet);
			}
		}
	}
}
