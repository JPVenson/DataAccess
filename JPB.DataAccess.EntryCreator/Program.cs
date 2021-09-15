/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using CommandLine;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.EntityCreator.MsSql;
using JPB.DataAccess.EntityCreator.SqLite;
using JPB.DataAccess.Manager;
using JPB.DataAccess.SqLite;
using WinConsole = System.Console;

namespace JPB.DataAccess.EntityCreator
{
	public class Program
	{
		public static AutoConsole AutoConsole { get; set; }

		[STAThread]
		private static void Main(string[] args)
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

			var connectionString = "";
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
						Console.WriteLine(
							"The current Entity Creator version ({0}) is not equals the version ({1}) you have provided.",
							version, AutoConsole.Options.Version);
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

			WinConsole.WriteLine("Mode: (MsSQL, SqLite)");

			IDatabaseStructure structure = null;

			var mode = AutoConsole.GetNextOption().ToLower();
			if (mode == "mssql")
			{
				WinConsole.WriteLine(
				@"Enter Connection string or type \explore to search for a server");
				if (Clipboard.ContainsText() && AutoConsole.Options == null)
				{
					var maybeConnection = Clipboard.GetText(TextDataFormat.Text);
					var strings = maybeConnection.Split(';');
					var any = strings.Any(s =>
						s.ToLower().Contains("data source=") || s.ToLower().Contains("initial catalog="));
					if (any)
					{
						WinConsole.WriteLine("Use clipboard content? [(y|Enter*) | no]");
						var WinConsoleKeyInfo = Console.ReadKey();
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
				{
					do
					{
						connectionString = AutoConsole.GetNextOption();
						if (connectionString == @"\explore")
						{
							var instance = SqlDataSourceEnumerator.Instance;
							WinConsole.WriteLine("Search for data Sources in current network");

							var table = instance.GetDataSources();
							WinConsole.WriteLine("Row count {0}", table.Rows.Count);

							foreach (var column in table.Columns.Cast<DataColumn>())
							{
								WinConsole.Write(column.ColumnName + "|");
							}

							for (var i = 0; i < table.Rows.Count; i++)
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
				}

				if (connectionString.StartsWith("file:\\\\"))
				{
					//structure = new DacpacDatabaseStructure(connectionString.Replace("file:\\\\", ""));
				}
				else
				{
					var dbAccessLayer = new DbAccessLayer(DbAccessType.MsSql, connectionString);
					structure = new DatabaseMsSqlStructure(dbAccessLayer);
					var databaseName = dbAccessLayer.Database.DatabaseName;
					if (string.IsNullOrEmpty(databaseName))
					{
						throw new Exception("Database not exists. Maybe wrong Connection or no Selected Database?");
					}
				}
			}
			else if (mode == "sqlite")
			{
				WinConsole.WriteLine("Enter Connection string:");
				connectionString = AutoConsole.GetNextOption();
				SqLiteInteroptWrapper.EnsureSqLiteInteropt();
				//Data Source=H:\Code\JPB.InhousePlayback\JPB.InhousePlayback\Server\Playback.50.db;
				structure = new DatabaseSqLiteStructure(new DbAccessLayer(new SqLiteStrategy(connectionString)));
			}

			try
			{
				new ConsoleEntityCreator(options.IncludeInVsProject, structure)
					.CreateEntrys(connectionString, outputDirectory, string.Empty);
			}
			catch (Exception e)
			{
				WinConsole.WriteLine("Error while executing the MsSQLEntity Creator:");
				WinConsole.WriteLine(e.ToString());
				WinConsole.WriteLine("Press any key to stop the application");
				Console.ReadLine();
				//Thread.Sleep(5000);

				return;
			}

			if (AutoConsole.Options == null && options.InputFile != null)
			{
				AutoConsole.SaveStorage(options.InputFile);
			}
		}

		public class Options
		{
			[Option('c', "commands", Required = false, HelpText = "The XML file of all commands")]
			public string InputFile { get; set; }

			[Option('v', "include-vs-project", Required = false, HelpText = "Search for a single .csproj file and update its content")]
			public bool IncludeInVsProject { get; set; }

			[Option('a', "argument", Required = false, HelpText = "A set of Variables that are replaced within the Auto Console arguments. Syntax: Name:Value")]
			public IEnumerable<string> Variables { get; set; }

			[Option('t', "template", HelpText = "The Morestachio template to be used for generation")]
			public string Template { get; set; }
		}
	}
}