

using JPB.Console.Helper.Grid;

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JPB.DataAccess.EntityCreator.Core;
using JPB.DataAccess.EntityCreator.Core.Compiler;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.DataAccess.EntityCreator.Core.Poco;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;

#endregion
using WinConsole = System.Console;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	public class MsSqlCreator : IMsSqlCreator
	{
		private readonly string[] usings =
		{
			"JPB.DataAccess.ModelsAnotations",
			"System.Collections.Generic",
			"System"
		};

		private bool _is2000;
		private bool _is2014;
		public DbAccessLayer Manager;

		public bool Is2000
		{
			get { return _is2000; }

			set { _is2000 = value; }
		}

		public bool Is2014
		{
			get { return _is2014; }

			set { _is2014 = value; }
		}

		public IEnumerable<ISharedInterface> SharedInterfaces { get; set; }
		public IEnumerable<ITableInfoModel> Tables { get; set; }
		public IEnumerable<Dictionary<int, string>> Enums { get; private set; }
		public IEnumerable<ITableInfoModel> Views { get; set; }
		public IEnumerable<IStoredPrcInfoModel> StoredProcs { get; private set; }

		public string TargetDir { get; set; }
		public bool GenerateConstructor { get; set; }
		public bool GenerateForgeinKeyDeclarations { get; set; }
		public bool GenerateCompilerHeader { get; set; }
		public bool GenerateConfigMethod { get; set; }

		/// <inheritdoc />
		public bool SplitByType { get; set; }
		public string Namespace { get; set; }

		public void CreateEntrys(string connection, string outputPath, string database)
		{
			TargetDir = outputPath;
			Manager = new DbAccessLayer(DbAccessType.MsSql, connection);
			bool checkDatabase;
			try
			{
				checkDatabase = Manager.CheckDatabase();
			}
			catch (Exception)
			{
				checkDatabase = false;
			}

			if (!checkDatabase)
			{
				throw new Exception("Database not accessible. Maybe wrong Connection or no Selected Database?");
			}
			var databaseName = string.IsNullOrEmpty(Manager.Database.DatabaseName) ? database : Manager.Database.DatabaseName;
			if (string.IsNullOrEmpty(databaseName))
			{
				throw new Exception("Database not exists. Maybe wrong Connection or no Selected Database?");
			}
			WinConsole.WriteLine("Connection OK ... Reading Server Version ...");

			SqlVersion = Manager.RunPrimetivSelect<string>("SELECT SERVERPROPERTY('productversion')").FirstOrDefault();

			WinConsole.WriteLine("Server version is {0}", SqlVersion);

			WinConsole.WriteLine("Reading Tables from {0} ...", databaseName);

			Tables = Manager.Select<TableInformations>().Select(s => new TableInfoModel(s, databaseName, Manager)).ToList();
			Views = Manager.Select<ViewInformation>().Select(s => new TableInfoModel(s, databaseName, Manager)).ToList();
			StoredProcs = Manager.Select<StoredProcedureInformation>().Select(s => new StoredPrcInfoModel(s)).ToList();

			WinConsole.WriteLine(
			"Found {0} Tables, {1} Views, {2} Procedures ... select a Table to see Options or start an Action", Tables.Count(),
			Views.Count(), StoredProcs.Count());
			Enums = new List<Dictionary<int, string>>();
			RenderMenu();
		}

		public bool WrapNullables { get; set; }

		public void Compile()
		{
			WinConsole.WriteLine("Start compiling with selected options");
			WinConsole.WriteLine("Please define a output Directory");
			TargetDir = Program.AutoConsole.GetNextOption();
			if (TargetDir == "temp")
			{
				TargetDir = Path.GetTempPath();
			}

			if (string.IsNullOrEmpty(TargetDir) || !Directory.Exists(TargetDir))
			{
				WinConsole.WriteLine("Invalid Directory ...");
				return;
			}

			var elements = Tables.Concat(Views);

			foreach (var tableInfoModel in elements)
			{
				SharedMethods.CompileTable(tableInfoModel, this);
			}

			foreach (var proc in StoredProcs)
			{
				if (proc.Exclude)
				{
					continue;
				}

				var targetCsName = proc.GetClassName();
				var compiler = new ProcedureCompiler(TargetDir, targetCsName);
				compiler.CompileHeader = GenerateCompilerHeader;
				compiler.Namespace = Namespace;
				compiler.GenerateConfigMethod = GenerateConfigMethod;
				compiler.TableName = proc.NewTableName;
				if (proc.Parameter.ParamaterSpParams != null)
				{
					foreach (var spParamter in proc.Parameter.ParamaterSpParams)
					{
						var targetType = DbTypeToCsType.GetClrType(spParamter.Type);
						var spcName = spParamter.Parameter.Replace("@", "");
						compiler.AddProperty(spcName, targetType);
					}
				}

				WinConsole.WriteLine("Compile Procedure {0}", compiler.Name);
				compiler.Compile(new List<ColumInfoModel>(), SplitByType);
			}

			WinConsole.WriteLine("Created all files");
			RenderMenuAction();
		}

		public string SqlVersion { get; set; }

		private void RenderMenu()
		{
			var ddbWinConsole = new StringBuilderInterlaced();
			ddbWinConsole.AppendLine("Tables: ");
			var i = 0;
			ddbWinConsole.Up();

			for (; i < Tables.Count(); i++)
			{
				ddbWinConsole.AppendInterlacedLine("{0} \t {1}", i, Tables.ToArray()[i].Info.TableName);
			}

			ddbWinConsole.Down();
			ddbWinConsole.AppendInterlacedLine("Views:");
			ddbWinConsole.Up();
			var j = i;
			for (; j < Views.Count() + i; j++)
			{
				ddbWinConsole.AppendInterlaced(j.ToString(), ConsoleColor.Gray);
				ddbWinConsole.AppendLine("\t {1}", j, Views.ToArray()[j - i].Info.TableName);
			}

			ddbWinConsole.Down();
			ddbWinConsole.AppendInterlacedLine("Procedures:");
			ddbWinConsole.Up();

			var k = j;
			for (; k < StoredProcs.Count() + j; k++)
			{
				ddbWinConsole.AppendInterlaced(k.ToString(), ConsoleColor.Gray);
				ddbWinConsole.AppendLine("\t {1}", j, StoredProcs.ToArray()[k - j].Parameter.TableName);
			}

			ddbWinConsole.Down();
			ddbWinConsole.AppendInterlacedLine("Actions: ");
			ddbWinConsole.Up();

			ddbWinConsole
				.AppendInterlacedLine(@"[Name | Number]", ConsoleColor.Blue)
				.Up()
				.AppendInterlacedLine("Edit table")
				.Down()
				.AppendInterlacedLine(@"\compile", ConsoleColor.Blue)
				.Up()
				.AppendInterlacedLine("Starts the Compiling of all Tables")
				.Down()
				.AppendInterlacedLine(@"\ns", ConsoleColor.Blue)
				.Up()
				.AppendInterlacedLine("Defines a default Namespace")
				.Down()
				.AppendInterlacedLine(@"\fkGen", ConsoleColor.Blue)
				.Up()
				.AppendInterlacedLine("Generates ForgeinKeyDeclarations")
				.Down()
				.AppendInterlacedLine(@"\addConfigMethod", ConsoleColor.Blue)
				.Up()
				.AppendInterlacedLine("Moves all attributes from Propertys and Methods into a single ConfigMethod")
				.Down()
				.AppendInterlacedLine(@"\withAutoCtor", ConsoleColor.Blue)
				.Up()
				.AppendInterlacedLine("Generates Loader Constructors")
				.Down()
				.AppendInterlacedLine(@"\autoGenNames", ConsoleColor.Blue)
				.Up()
				.AppendInterlacedLine("Defines all names after a common naming convention")
				.Down()
				.AppendInterlacedLine(@"\addCompilerHeader", ConsoleColor.Blue)
				.Up()
				.AppendInterlacedLine("Adds a Timestamp and a created user on each POCO")
				.Down()
				.AppendInterlacedLine(@"\exit", ConsoleColor.Blue)
				.Up()
				.AppendInterlacedLine("Stops the execution of the program");

			ddbWinConsole.WriteToConsole(true);
			RenderMenuAction();
		}

		private void RenderMenuAction()
		{
			var readLine = Program.AutoConsole.GetNextOption();
			if (string.IsNullOrEmpty(readLine))
			{
				RenderMenu();
				return;
			}

			var input = readLine.ToLower();
			int result;
			var hasSelectTable = int.TryParse(input, out result);
			if (hasSelectTable)
			{
				if (result > Tables.Count() || result < 0)
				{
					WinConsole.WriteLine("Unvalid number");
					RenderMenu();
					return;
				}

				RenderTableMenu(Tables.ElementAt(result));
			}

			var tableName = Tables.FirstOrDefault(s => s.GetClassName() == readLine);

			if (tableName != null)
			{
				RenderTableMenu(tableName);
			}
			else
			{
				var split = input.Split(' ');
				switch (split[0])
				{
					case @"\autogennames":
						AutoAlignNames();
						break;
					case @"\ns":
						SetNamespace();
						break;
					case @"\fkgen":
						SetForgeinKeyDeclarationCreation();
						break;
					case @"\compile":
						Compile();
						break;
					case @"\withautoctor":
						SetRenderAutoCtor();
						break;
					case @"\addconfigmethod":
						SetConfigMethod();
						break;
					case @"\addcompilerheader":
						SetCompilerHeader();
						break;
					case @"\exit":
						return;

					default:
						RenderMenuAction();
						break;
				}
			}
		}

		private void SetCompilerHeader()
		{
			GenerateCompilerHeader = !GenerateCompilerHeader;
			WinConsole.WriteLine("Compiler header is {0}", GenerateCompilerHeader ? "set" : "unset");
			RenderMenuAction();
		}

		private void SetConfigMethod()
		{
			GenerateConfigMethod = !GenerateConfigMethod;
			WinConsole.WriteLine("Compiler header is {0}", GenerateConfigMethod ? "set" : "unset");
			RenderMenuAction();
		}

		private void SetForgeinKeyDeclarationCreation()
		{
			GenerateForgeinKeyDeclarations = !GenerateForgeinKeyDeclarations;
			WinConsole.WriteLine("Auto ForgeinKey Declaration Creation is {0}", GenerateForgeinKeyDeclarations ? "set" : "unset");
			RenderMenuAction();
		}

		private void SetRenderAutoCtor()
		{
			GenerateConstructor = !GenerateConstructor;
			WinConsole.WriteLine("Auto Ctor is {0}", GenerateConstructor ? "set" : "unset");
			RenderMenuAction();
		}

		private void SetNamespace()
		{
			WinConsole.WriteLine("Enter your DefaultNamespace");
			var ns = Program.AutoConsole.GetNextOption();
			Namespace = ns;
			RenderMenuAction();
		}

		public void AutoAlignNames()
		{
			SharedMethods.AutoAlignNames(Tables);
			RenderMenuAction();
		}

		private void RenderTableMenu(ITableInfoModel selectedTable)
		{
			WinConsole.WriteLine("Actions:");

			WinConsole.WriteLine(@"\ForModel   [\c] [ColumnName] [[NewName] | [\d]]  ");
			WinConsole.WriteLine(@"        Adds a ForModelAttribute to a Property or class with \c for class. deletes it with \d");
			WinConsole.WriteLine("Example:");
			WinConsole.WriteLine(@"        \ForModelAttribute \c NewTableName");
			WinConsole.WriteLine("             Adding a ForModelAttribute Attribute to the generated class");
			WinConsole.WriteLine(@"        \ForModelAttribute ID_Column NewName");
			WinConsole.WriteLine(
			"             Adding a ForModelAttribute Attribute to the generated Property with the value NewName");
			WinConsole.WriteLine();
			WinConsole.WriteLine(@"\Exclude			[true | false] [ColumnName]");
			WinConsole.WriteLine("         Exclude this table from the Process");
			WinConsole.WriteLine(@"\InsertIgnore		true | false] [ColumnName]");
			WinConsole.WriteLine("         Exclude this column from inserts");
			WinConsole.WriteLine(@"\Enum				[true | false] [ColumnName]");
			WinConsole.WriteLine("         Marks this Column as an ENUM field on the Database. Experimental");
			WinConsole.WriteLine(@"\Fallack			[true | false]]");
			WinConsole.WriteLine("         Should create a LoadNotImplimentedDynamic Property for Not Loaded fields");
			WinConsole.WriteLine(@"\Createloader		[true | false]]");
			WinConsole.WriteLine("         Should create a Dataloader that loads the Properties from the IDataRecord");
			WinConsole.WriteLine(@"\CreateSelect		[true | false]]");
			WinConsole.WriteLine("         Should create a Attribute with a Select Statement");
			WinConsole.WriteLine(@"\stats");
			WinConsole.WriteLine("         Shows all avalible data from this table");
			WinConsole.WriteLine(@"\back");
			WinConsole.WriteLine("         Go back to Main Menu");

			var readLine = Program.AutoConsole.GetNextOption();
			if (string.IsNullOrEmpty(readLine))
			{
				RenderMenu();
				return;
			}

			var parts = readLine.Split(' ').ToArray();

			if (parts.Any())
			{
				switch (parts[0].ToLower())
				{
					case @"\formodel":
						if (parts.Length == 3)
						{
							var deleteOrNewName = parts[2];

							if (parts[1] == @"\c")
							{
								if (deleteOrNewName == @"\d")
								{
									selectedTable.NewTableName = string.Empty;
								}

								else
								{
									selectedTable.NewTableName = parts[2];
								}
								WinConsole.WriteLine("Renamed from {0} to {1}", selectedTable.Info.TableName, selectedTable.NewTableName);
							}
							else
							{
								var oldName = parts[1];
								var columnToRename = selectedTable.ColumnInfos.FirstOrDefault(s => s.ColumnInfo.ColumnName == oldName);
								if (deleteOrNewName != @"\d")
								{
									if (columnToRename != null)
									{
										columnToRename.NewColumnName = deleteOrNewName;
										WinConsole.WriteLine("Renamed from {0} to {1}", oldName, deleteOrNewName);
									}
									else
									{
										WinConsole.WriteLine("There is no Column that is named like {0}", deleteOrNewName);
									}
								}
								else
								{
									if (columnToRename != null)
									{
										columnToRename.NewColumnName = string.Empty;
										WinConsole.WriteLine("Removed the Renaming from {0} to {1}", deleteOrNewName, parts[1]);
									}
									else
									{
										WinConsole.WriteLine("There is no Column that is named like {0}", deleteOrNewName);
									}
								}
							}
						}
						else
						{
							WinConsole.WriteLine("Unvalid Input expected was [ColumnName] [NewName] ");
							WinConsole.WriteLine();
						}
						break;
					case @"\insertignore":
						if (parts.Length == 3)
						{
							bool result;
							bool.TryParse(parts[1], out result);

							var column = selectedTable.ColumnInfos.FirstOrDefault(s => s.ColumnInfo.ColumnName == parts[2]);
							if (column == null)
							{
								WinConsole.WriteLine("Unvalid Input expected was  [ColumnName] ");
								WinConsole.WriteLine();
								break;
							}

							column.InsertIgnore = result;
						}
						else
						{
							WinConsole.WriteLine("Unvalid Input expected was  true | false ");
							WinConsole.WriteLine();
						}
						break;
					case @"\enum":
						if (parts.Length == 3)
						{
							bool result;
							bool.TryParse(parts[1], out result);

							var column = selectedTable.ColumnInfos.FirstOrDefault(s => s.ColumnInfo.ColumnName == parts[2]);
							if (column == null)
							{
								WinConsole.WriteLine("Unvalid Input expected was  [ColumnName] ");
								WinConsole.WriteLine();
								break;
							}

							if (result == false)
							{
								column.EnumDeclaration = null;
								break;
							}

							if (column.ForgeinKeyDeclarations == null)
							{
								WinConsole.WriteLine("Declare the Enum:");
								WinConsole.WriteLine("Format: [Number] [Description]");
								WinConsole.WriteLine("Example: '1 Valid'");
								WinConsole.WriteLine("Type ok to submit");
								WinConsole.WriteLine("Type cancel to revert");

								column.EnumDeclaration = new EnumDeclarationModel();

								WinConsole.WriteLine("Name of Enum:");
								column.EnumDeclaration.Name = Program.AutoConsole.GetNextOption();

								while (true)
								{
									var inp = Program.AutoConsole.GetNextOption();
									if (inp.ToLower() == "ok")
									{
										break;
									}
									if (inp.ToLower() == "cancel")
									{
										column.EnumDeclaration = null;
									}

									var option = inp.Split(' ');
									if (option.Length == 2)
									{
										var enumNumber = option[0];

										int enumNumberResult;
										if (int.TryParse(enumNumber, out enumNumberResult))
										{
											column.EnumDeclaration.Values.Add(enumNumberResult, option[1]);
											WinConsole.WriteLine("Added Enum member {0} = {1}", option[1], enumNumberResult);
										}
										else
										{
											WinConsole.WriteLine("Invalid Enum number Supplyed");
										}
									}
									else
									{
										WinConsole.WriteLine("Invalid Enum member Supplyed");
									}
								}
							}
							else
							{
								WinConsole.WriteLine("Enum is ForgeinKey.");
								WinConsole.WriteLine("Read data from Database to autogenerate Enum");
								WinConsole.WriteLine("Reading table: '{0}'", column.ForgeinKeyDeclarations.TableName);

								var tableContent =
										Manager.Select<DynamicTableContentModel>(new object[] { column.ForgeinKeyDeclarations.TableName });

								if (!tableContent.Any())
								{
									WinConsole.WriteLine("The Enum table '{0}' does not contain any data", column.ForgeinKeyDeclarations.TableName);
									break;
								}

								if (tableContent.First().DataHolder.Count > 2)
								{
									WinConsole.WriteLine("The Enum table '{0}' contains more then 2 columns", column.ForgeinKeyDeclarations.TableName);
									break;
								}

								if (!tableContent.Any(s => s.DataHolder.Any(f => f.Value is int)))
								{
									WinConsole.WriteLine("The Enum table '{0}' does not contains exactly one column of type int",
									column.ForgeinKeyDeclarations.TableName);
									break;
								}

								if (!tableContent.Any(s => s.DataHolder.Any(f => f.Value is string)))
								{
									WinConsole.WriteLine("The Enum table '{0}' does not contains exactly one column of type int",
									column.ForgeinKeyDeclarations.TableName);
									break;
								}

								column.EnumDeclaration = new EnumDeclarationModel();
								column.EnumDeclaration.Name = column.ForgeinKeyDeclarations.TableName + "LookupValues";

								foreach (var item in tableContent)
								{
									var pk = (int)item.DataHolder.FirstOrDefault(s => s.Value is int).Value;
									var value = (string)item.DataHolder.FirstOrDefault(s => s.Value is string).Value;
									column.EnumDeclaration.Values.Add(pk, value);
									WinConsole.WriteLine("Adding Enum member '{0}' = '{1}'", value, pk);
								}
							}
						}
						else
						{
							WinConsole.WriteLine("Unvalid Input expected was  true | false ");
							WinConsole.WriteLine();
						}
						break;
					case @"\exclude":
						if (parts.Length == 2)
						{
							bool result;
							bool.TryParse(parts[1], out result);
							selectedTable.Exclude = result;
						}
						else
						{
							if (parts.Length == 3)
							{
								bool result;
								bool.TryParse(parts[1], out result);

								var column = selectedTable.ColumnInfos.FirstOrDefault(s => s.ColumnInfo.ColumnName == parts[2]);
								if (column == null)
								{
									WinConsole.WriteLine("Unvalid Input expected was  [ColumnName] ");
									WinConsole.WriteLine();
									break;
								}

								column.Exclude = result;
							}
							else
							{
								WinConsole.WriteLine("Unvalid Input expected was  true | false ");
								WinConsole.WriteLine();
							}
						}
						break;
					case @"\fallack":
						if (parts.Length == 2)
						{
							bool result;
							bool.TryParse(parts[1], out result);
							selectedTable.CreateFallbackProperty = result;
						}
						else
						{
							WinConsole.WriteLine("Unvalid Input expected was  true | false ");
							WinConsole.WriteLine();
						}
						break;

					case @"\createloader":
						if (parts.Length == 2)
						{
							bool result;
							bool.TryParse(parts[1], out result);
							selectedTable.CreateDataRecordLoader = result;
						}
						else
						{
							WinConsole.WriteLine("Unvalid Input expected was  true | false ");
							WinConsole.WriteLine();
						}
						break;
					case @"\createSelect":
						if (parts.Length == 2)
						{
							bool result;
							bool.TryParse(parts[1], out result);
							selectedTable.CreateSelectFactory = result;
						}
						else
						{
							WinConsole.WriteLine("Unvalid Input expected was  true | false ");
							WinConsole.WriteLine();
						}
						break;

					case @"\stats":
						WinConsole.WriteLine("Name =                       {0}", selectedTable.Info.TableName);
						WinConsole.WriteLine("Cs class Name =              {0}", selectedTable.GetClassName());
						WinConsole.WriteLine("Exclude =                    {0}", selectedTable.Exclude);
						WinConsole.WriteLine("Create Fallback Property =   {0}", selectedTable.CreateFallbackProperty);
						WinConsole.WriteLine("\t Create Select Factory =   {0}", selectedTable.CreateSelectFactory);
						WinConsole.WriteLine("\t Create Dataloader =       {0}", selectedTable.CreateDataRecordLoader);
						WinConsole.WriteLine("Columns");
						foreach (var columnInfo in selectedTable.ColumnInfos)
						{
							WinConsole.WriteLine("--------------------------------------------------------");
							WinConsole.WriteLine("\t Name =						{0}", columnInfo.ColumnInfo.ColumnName);
							WinConsole.WriteLine("\t Is Primary Key =			{0}", columnInfo.PrimaryKey);
							WinConsole.WriteLine("\t Cs Property Name =			{0}", columnInfo.GetPropertyName());
							WinConsole.WriteLine("\t Position From Top =		{0}", columnInfo.ColumnInfo.PositionFromTop);
							WinConsole.WriteLine("\t Nullable =					{0}", columnInfo.ColumnInfo.Nullable);
							WinConsole.WriteLine("\t Cs Type =					{0}", columnInfo.ColumnInfo.TargetType.Name);
							WinConsole.WriteLine("\t forgeinKey Type =			{0}", columnInfo.ForgeinKeyDeclarations);
							WinConsole.WriteLine("\t Is Enum Type =				{0}", columnInfo.EnumDeclaration != null);
						}
						break;

					case @"\back":
						RenderMenu();
						return;
					default:
						break;
				}
			}

			RenderTableMenu(selectedTable);
		}

		private void CheckVersion()
		{
			var versionParts = SqlVersion.Split('.');
			var major = int.Parse(versionParts[0]);
			var minor = int.Parse(versionParts[1]);
			var build = int.Parse(versionParts[2]);
			var revision = int.Parse(versionParts[3]);

			if (major > 11)
			{
				Is2000 = false;
				Is2014 = true;
			}
			else if (major == 11)
			{
				if (minor > 0)
				{
					Is2014 = true;
				}
				else if (minor == 0)
				{
					if (build > 2100)
					{
						Is2014 = true;
					}
					else if (build == 2100)
					{
						if (revision >= 60)
						{
							Is2014 = true;
						}
					}
				}
			}
			else
			{
				Is2014 = false;
			}
		}
	}
}

//var pathToFile = Path.Combine(TargetDir, targetCsName, ".cs");
//          var csCreator = new StringBuilder();

//          foreach (var @using in usings)
//          {
//              csCreator.AppendLine("using " + @using + ";");
//          }

//          csCreator.AppendLine();

//          csCreator.AppendLine("namespace JPB.DataAccess.EntryCreator.AutoGenerated");
//          //opening namespace
//          csCreator.AppendLine("{");

//          csCreator.AppendLine();

//          if (!string.IsNullOrEmpty(tableInfoModel.NewTableName))
//          {
//              csCreator.Append("[ForModelAttribute(");
//              csCreator.Append(tableInfoModel.Info.TableName);
//              csCreator.AppendLine(")");
//          }

//          csCreator.Append("public class ");
//          csCreator.AppendLine(targetCsName);
//          //Opening class
//          csCreator.AppendLine("{");

//          var bytes = Encoding.Unicode.GetBytes(csCreator.ToString());
//          using (var fs = new FileStream(pathToFile,FileMode.CreateNew))
//          {
//              var writeAsync = fs.WriteAsync(bytes, 0, bytes.Length);
//          }