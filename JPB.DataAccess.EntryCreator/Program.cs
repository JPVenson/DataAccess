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
using System.Text;
using System.Threading;
using CommandLine;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using TextDataFormat = System.Windows.TextDataFormat;
using WinConsole = System.Console;

namespace JPB.DataAccess.EntityCreator
{
	public class Program
	{
		public class Options
		{
			[Option('c', "commands", Required = false, HelpText = "The XML file of all commands")]
			public string InputFile { get; set; }

			[Option('v', "include-vs-project", Required = false, HelpText = "Search for a single .csproj file and update its content")]
			public bool IncludeInVsProject { get; set; }

			[Option('a', "argument", Required = false, HelpText = "A set of Variables that are replaced within the Auto Console arguments. Syntax: Name:Value")]
			public IEnumerable<string> Variables { get; set; }
		}

		public static AutoConsole AutoConsole { get; set; }

		[STAThread]
		static void Main(string[] args)
		{
			var prog = new Program();

			Options options = null;
			var parserResult = Parser.Default.ParseArguments<Options>(args)				
				.WithParsed(f => { options = f; })
				.WithNotParsed(f =>
				{
					foreach (var error in f)
					{
						Console.WriteLine($"ERROR: {error}");
					}
				});
			if (options == null)
			{
				Thread.Sleep(5000);
				return;
			}
			string connectionString = "";
			string outputDirectory = null;
			if (!string.IsNullOrWhiteSpace(options.InputFile))
			{
				Console.WriteLine($"Specified command file: '{options.InputFile}'");
				if (File.Exists(options.InputFile))
				{
					AutoConsole = new AutoConsole(options.InputFile, options.Variables);

					var version = Assembly.GetExecutingAssembly().GetName().Version;
					if (AutoConsole.Options.Version == null || new Version(AutoConsole.Options.Version) != version)
					{
						Console.WriteLine("WARNING");
						Console.WriteLine(string.Format("The current Entity Creator version ({0}) is not equals the version ({1}) you have provided.", version, AutoConsole.Options.Version));
						Console.WriteLine("There might be errors or unexpected Behavor");
					}
				}
				else
				{
					Console.WriteLine("The commandfile does not exist");
					AutoConsole = new AutoConsole(null, new string[0]);
				}
			}
			else
			{
				AutoConsole = new AutoConsole(null, new string[0]);
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
				new MsSqlCreator(options.IncludeInVsProject)
					.CreateEntrys(connectionString, outputDirectory, string.Empty);
			}
			catch (Exception e)
			{
				WinConsole.WriteLine("Error while executing the MsSQLEntity Creator:");
				WinConsole.WriteLine(e.ToString());
				WinConsole.WriteLine("Press any key to stop the application");
				Thread.Sleep(5000);

				return;
			}
			
			if (AutoConsole.Options == null && options.InputFile != null)
			{
				AutoConsole.SaveStorage(options.InputFile);
			}
		}
	}
}
