/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using JPB.DataAccess.EntityCreator.Compiler;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using System.Xml.Serialization;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	public class MsSqlCreator : IEntryCreator
	{
		public static DbAccessLayer Manager;
		List<TableInfoModel> _tableNames;

		public string TargetDir { get; set; }
		public bool WithAutoCtor { get; set; }
		public bool GenerateForgeinKeyDeclarations { get; set; }

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
				Console.WriteLine("Database not accessible. Maybe wrong Connection or no Selected Database?");
				return;
			}
			var databaseName = string.IsNullOrEmpty(Manager.Database.DatabaseName) ? database : Manager.Database.DatabaseName;
			if (string.IsNullOrEmpty(databaseName))
			{
				Console.WriteLine("Database not exists. Maybe wrong Connection or no Selected Database?");
				return;
			}
			Console.WriteLine("Connection OK ... Reading Server Version ...");

			SqlVersion = Manager.RunPrimetivSelect<string>("SELECT SERVERPROPERTY('productversion')").FirstOrDefault();

			Console.WriteLine("Server version is {0}", SqlVersion);

			Console.WriteLine("Reading Tables from {0} ...", databaseName);

			_tableNames = Manager.Select<TableInformations>().Select(s => new TableInfoModel(s, databaseName)).ToList();
			_views = Manager.Select<ViewInformation>().Select(s => new TableInfoModel(s, databaseName)).ToList();
			_storedProcs = Manager.Select<StoredProcedureInformation>().Select(s => new StoredPrcInfoModel(s)).ToList();

			Console.WriteLine("Found {0} Tables, {1} Views, {2} Procedures ... select a Table to see Options or start an Action", _tableNames.Count, _views.Count, _storedProcs.Count);
			RenderMenu();          
		}

		private readonly string[] usings = new[]
		{
			"JPB.DataAccess.ModelsAnotations",
			"System.Collections.Generic",
			"System"
		};

		private void RenderMenu()
		{
			Console.WriteLine("Tables:");
			int i = 0;
			for (; i < _tableNames.Count; i++)
			{
				Console.WriteLine("{0} \t {1}", i, _tableNames[i].Info.TableName);
			}

			Console.WriteLine("Views:");
			int j = i;
			for (; j < _views.Count + i; j++)
			{
				Console.WriteLine("{0} \t {1}", j, _views[j - i].Info.TableName);
			}

			Console.WriteLine("Procedures:");
			int k = j;
			for (; k < _storedProcs.Count + j; k++)
			{
				Console.WriteLine("{0} \t {1}", k, _storedProcs[k - j].Parameter.TableName);
			}

			Console.WriteLine("Actions: ");

			Console.WriteLine(@"\compile       Starts the Compiling of all Tables");
			Console.WriteLine(@"\ns            Defines a default Namespace");
			Console.WriteLine(@"\fkGen         Generates ForgeinKeyDeclarations");
			Console.WriteLine(@"\withautoctor  Generates Loader Constructors");
			Console.WriteLine(@"\autoGenNames  Defines all names after a common naming convention");
			Console.WriteLine(@"\add           Adds elements to existing cs classes");
			Console.WriteLine(@"    Options: ");
			Console.WriteLine(@"            \ctor [\r]   Adds a loader Constructor to all classes and if \r is set existing ctors that impliments IDataReader will be overwritten");
			Console.WriteLine(@"\exit          Stops the execution of the program");
			RenderMenuAction();
		}

		private void RenderMenuAction()
		{
			var readLine = Program.AutoConsole.GetNextOption();
			if (string.IsNullOrEmpty(readLine))
			{
				Console.Clear();
				RenderMenu();
				return;
			}

			var input = readLine.ToLower();
			int result;
			var hasSelectTable = int.TryParse(input, out result);
			if (hasSelectTable)
			{
				if (result > _tableNames.Count || result < 0)
				{
					Console.Clear();
					Console.WriteLine("Unvalid number");
					RenderMenu();
					return;
				}

				var selectedTable = _tableNames.ElementAt(result);
				Console.Clear();
				RenderTableMenu(selectedTable);
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
						RenderCompiler();
						break;
					case @"\withautoctor":
						RenderAutoCtor();
						break;
					case @"\add":
						if (split.Length >= 2)
						{
							switch (split[1])
							{
								case @"\ctor":
									var replaceExisting = false;
									if (split.Length >= 3)
									{
										replaceExisting = split[3] == "\r";
									}
									RenderCtorCompiler(replaceExisting);
									break;
							}
						}
						else
						{
							RenderCtorCompiler(false);
						}
						break;
					case @"\exit":
						return;

					default:
						RenderMenuAction();
						break;
				}
			}
		}

		private void SetForgeinKeyDeclarationCreation()
		{
			GenerateForgeinKeyDeclarations = !GenerateForgeinKeyDeclarations;
			Console.WriteLine("Auto ForgeinKey Declaration Creation is {0}", GenerateForgeinKeyDeclarations ? "set" : "unset");
			RenderMenu();
		}

		private void RenderAutoCtor()
		{
			WithAutoCtor = !WithAutoCtor;
			Console.WriteLine("Auto Ctor is {0}", WithAutoCtor ? "set" : "unset");
			RenderMenu();
		}

		private void SetNamespace()
		{
			Console.WriteLine("Enter your DefaultNamespace");
			var ns = Program.AutoConsole.GetNextOption();
			foreach (var item in _tableNames.Concat(_views))
			{
				item.NewNamespace = ns;
			}
			RenderMenu();
		}

		private void RenderCtorCompiler(bool replaceExisting)
		{
			var files = Directory.GetFiles(TargetDir, "*.cs");

			var provider = CodeDomProvider.CreateProvider("CSharp");
			foreach (var file in files)
			{
				var loadFromFile = ClassCompiler.LoadFromFile(file);

				bool createCtor = false;
				var ctor = loadFromFile.Members.Where(s => s is CodeConstructor).Cast<CodeConstructor>().Where(s => s != null).ToArray();

				if (ctor.Any())
				{
					var fullNameOfObjectFactory = typeof(ObjectFactoryMethodAttribute).FullName;
					var ctorWithIdataRecord =
						ctor.FirstOrDefault(
							s =>
								s.CustomAttributes.Cast<CodeAttributeDeclaration>()
									.Any(e => e.Name == fullNameOfObjectFactory)
								&& s.Parameters.Count == 1
								&& s.Parameters.Cast<CodeParameterDeclarationExpression>().Any(e => Type.GetType(e.Type.BaseType) == typeof(IDataRecord)));

					if (ctorWithIdataRecord != null)
					{
						if (replaceExisting)
						{
							loadFromFile.MembersFromBase.Remove(ctorWithIdataRecord);
						}
						else
						{
							continue;
						}
					}
					else
					{
						createCtor = true;
					}
				}
				else
				{
					createCtor = true;
				}

				if (createCtor)
				{
					var propertys =
						loadFromFile.Members.Cast<CodeMemberProperty>()
							.Where(s => s != null)
							.ToArray()
							.Select(s => new Tuple<string, Type>(s.Name, Type.GetType(s.Type.BaseType)));

					var generateTypeConstructor = new ClassCompiler("", "").GenerateTypeConstructor(propertys.ToDictionary(f =>
					{
						var property =
							loadFromFile.Members.Cast<CodeMemberProperty>().FirstOrDefault(e => e.Name == f.Item1);

						var codeAttributeDeclaration = property.CustomAttributes.Cast<CodeAttributeDeclaration>()
							.FirstOrDefault(e => e.AttributeType.BaseType == typeof(ForModelAttribute).FullName);
						if (codeAttributeDeclaration != null)
						{
							var firstOrDefault =
								codeAttributeDeclaration.Arguments.Cast<CodeAttributeDeclaration>()
									.FirstOrDefault();
							if (firstOrDefault != null)
							{
								var codeAttributeArgument = firstOrDefault
									.Arguments.Cast<CodeAttributeArgument>()
									.FirstOrDefault();
								if (codeAttributeArgument != null)
								{
									var codePrimitiveExpression = codeAttributeArgument
										.Value as CodePrimitiveExpression;
									if (codePrimitiveExpression != null)
										return
											codePrimitiveExpression.Value.ToString();
								}
							}
						}
						return f.Item1;
					}, f =>
					{
						return f;
					}));

					loadFromFile.MembersFromBase.Insert(0, generateTypeConstructor);
				}
			}
		}


		private void AutoAlignNames()
		{
			Console.WriteLine("Auto rename Columns after common cs usage");
			Console.WriteLine();
			foreach (var tableInfoModel in _tableNames)
			{
				var tableName = tableInfoModel.Info.TableName;
				Console.WriteLine("Check Table: {0}", tableName);
				var newName = CheckOrAlterName(tableName);
				if (newName != tableName)
				{
					Console.WriteLine("Alter Table'{0}' to '{1}'", tableName, newName);
					tableInfoModel.NewTableName = newName;
				}

				Console.WriteLine();

				foreach (var columnInfo in tableInfoModel.ColumnInfos)
				{
					var columnName = columnInfo.ColumnInfo.ColumnName;
					Console.WriteLine("\tCheck Column: {0}", columnName);
					var newColumnName = CheckOrAlterName(columnName);
					if (newColumnName != columnName)
					{
						Console.WriteLine("\tAlter Column'{0}' to '{1}'", columnName, newColumnName);
						columnInfo.NewColumnName = newColumnName;
					}
				}
			}

			Console.WriteLine("Renaming is done");
			RenderMenu();
		}

		private static readonly char[] unvalid = { '_', ' ' };

		private string CheckOrAlterName(string name)
		{
			var newName = name;

			foreach (var unvalidPart in unvalid)
			{
				if (newName.Contains(unvalidPart))
				{
					var indexOfElement = newName.IndexOf(unvalidPart.ToString(CultureInfo.InvariantCulture), System.StringComparison.Ordinal) + 1;
					if (indexOfElement < newName.Length)
					{
						var elementAt = newName.ElementAt(indexOfElement);
						if (!unvalid.Contains(elementAt))
						{
							var remove = newName.Remove(indexOfElement, 1);
							newName = remove.Insert(indexOfElement, elementAt.ToString(CultureInfo.InvariantCulture).ToUpper());
						}
					}
					newName = newName.Replace(unvalidPart.ToString(CultureInfo.InvariantCulture), string.Empty);
				}
			}

			return newName;
		}

		private void RenderTableMenu(TableInfoModel selectedTable)
		{
			Console.WriteLine("Actions:");

			Console.WriteLine(@"\ForModelAttribute   [\c] [ColumnName] [[NewName] | [\d]]  ");
			Console.WriteLine(@"        Adds a ForModelAttribute to a Property or class with \c. deletes it with \d");
			Console.WriteLine("Example:");
			Console.WriteLine(@"        \ForModelAttribute \c NewTableName");
			Console.WriteLine("             Adding a ForModelAttribute Attribute to the generated class");
			Console.WriteLine(@"        \ForModelAttribute ID_Column NewName");
			Console.WriteLine("             Adding a ForModelAttribute Attribute to the generated Property with the value NewName");
			Console.WriteLine();
			Console.WriteLine(@"\Exclude    [Value [true | false]]");
			Console.WriteLine("         Exclude this table from the Process");
			Console.WriteLine(@"\Fallack    [true | false]]");
			Console.WriteLine("         Should create a LoadNotImplimentedDynamic Property for Not Loaded fieds");
			Console.WriteLine(@"\Createloader    [true | false]]");
			Console.WriteLine("         Should create a Dataloader that loads the Propertys from the IDataRecord");
			Console.WriteLine(@"\CreateSelect   [true | false]]");
			Console.WriteLine("         Should create a Attribute with a Select Statement");
			Console.WriteLine(@"\stats");
			Console.WriteLine("         Shows all avalible data from this table");
			Console.WriteLine(@"\back");
			Console.WriteLine("         Go back to Main Menu");

			var readLine = Program.AutoConsole.GetNextOption();
			if (string.IsNullOrEmpty(readLine))
			{
				RenderMenu();
				return;
			}

			var parts = readLine.Split(' ').ToArray();
			Console.Clear();

			if (parts.Any())
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
								Console.WriteLine("Renamed from {0} to {1}", selectedTable.Info.TableName, selectedTable.NewTableName);
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
										Console.WriteLine("Renamed from {0} to {1}", oldName, deleteOrNewName);
									}
									else
									{
										Console.WriteLine("There is no Column that is named like {0}", deleteOrNewName);
									}
								}
								else
								{
									if (columnToRename != null)
									{
										columnToRename.NewColumnName = string.Empty;
										Console.WriteLine("Removed the Renaming from {0} to {1}", deleteOrNewName, parts[1]);
									}
									else
									{
										Console.WriteLine("There is no Column that is named like {0}", deleteOrNewName);
									}
								}
							}
						}
						else
						{
							Console.Clear();
							Console.WriteLine("Unvalid Input expected was [ColumnName] [NewName] ");
							Console.WriteLine();
						}
						break;
					case @"\exclude":
						if (parts.Length == 2)
						{
							bool result;
							Boolean.TryParse(parts[1], out result);
							selectedTable.Exclude = result;
						}
						else
						{
							Console.Clear();
							Console.WriteLine("Unvalid Input expected was true | false ");
							Console.WriteLine();
						}
						break;
					case @"\fallack":
						if (parts.Length == 2)
						{
							bool result;
							Boolean.TryParse(parts[1], out result);
							selectedTable.CreateFallbackProperty = result;
						}
						else
						{
							Console.Clear();
							Console.WriteLine("Unvalid Input expected was  true | false ");
							Console.WriteLine();
						}
						break;

					case @"\Createloader":
						if (parts.Length == 2)
						{
							bool result;
							Boolean.TryParse(parts[1], out result);
							selectedTable.CreateDataRecordLoader = result;
						}
						else
						{
							Console.Clear();
							Console.WriteLine("Unvalid Input expected was  true | false ");
							Console.WriteLine();
						}
						break;
					case @"\CreateSelect":
						if (parts.Length == 2)
						{
							bool result;
							Boolean.TryParse(parts[1], out result);
							selectedTable.CreateSelectFactory = result;
						}
						else
						{
							Console.Clear();
							Console.WriteLine("Unvalid Input expected was  true | false ");
							Console.WriteLine();
						}
						break;

					case @"\stats":
						Console.WriteLine("Name =                       {0}", selectedTable.Info.TableName);
						Console.WriteLine("Cs class Name =              {0}", selectedTable.GetClassName());
						Console.WriteLine("Exclude =                    {0}", selectedTable.Exclude);
						Console.WriteLine("Create Fallback Property =   {0}", selectedTable.CreateFallbackProperty);
						Console.WriteLine("\t Create Select Factory =   {0}", selectedTable.CreateSelectFactory);
						Console.WriteLine("\t Create Dataloader =       {0}", selectedTable.CreateDataRecordLoader);
						Console.WriteLine("Columns");
						foreach (var columnInfo in selectedTable.ColumnInfos)
						{
							Console.WriteLine("--------------------------------------------------------");
							Console.WriteLine("\t Name =                    {0}", columnInfo.ColumnInfo.ColumnName);
							Console.WriteLine("\t Is Primary Key =          {0}", columnInfo.PrimaryKey);
							Console.WriteLine("\t Cs Property Name =        {0}", columnInfo.GetPropertyName());
							Console.WriteLine("\t Position From Top =       {0}", columnInfo.ColumnInfo.PositionFromTop);
							Console.WriteLine("\t Nullable =                {0}", columnInfo.ColumnInfo.Nullable);
							Console.WriteLine("\t Cs Type =                 {0}", columnInfo.ColumnInfo.TargetType.Name);
							Console.WriteLine("\t forgeinKey Type =         {0}", columnInfo.ForgeinKeyDeclarations);
						}
						break;

					case @"\back":
						Console.Clear();
						RenderMenu();
						return;
					default:
						Console.Clear();
						RenderTableMenu(selectedTable);
						break;
				}
			
			RenderTableMenu(selectedTable);
		}

		private void RenderCompiler()
		{
			Console.Clear();
			Console.WriteLine("Start compiling with selected options");

			var classes = new List<ClassCompiler>();

			foreach (var tableInfoModel in _tableNames.Concat(_views))
			{
				if (tableInfoModel.Exclude)
					continue;

				var targetCsName = tableInfoModel.GetClassName();

				var compiler = new ClassCompiler(TargetDir, targetCsName);
				compiler.Namespace = tableInfoModel.NewNamespace;
				compiler.TargetName = targetCsName;
				classes.Add(compiler);

				if (tableInfoModel.CreateSelectFactory || WithAutoCtor)
				{
					compiler.GenerateTypeConstructorBasedOnElements(tableInfoModel.ColumnInfos);
				}

				foreach (var columInfoModel in tableInfoModel.ColumnInfos)
				{
					var codeMemberProperty = compiler.AddProperty(columInfoModel);
					if (columInfoModel.PrimaryKey)
					{
						codeMemberProperty.CustomAttributes.Add(
							new CodeAttributeDeclaration(typeof(PrimaryKeyAttribute).Name));
					}
					if (columInfoModel.ForgeinKeyDeclarations != null)
					{
						var isRefTypeKnown = _tableNames.FirstOrDefault(s => s.Info.TableName == columInfoModel.ForgeinKeyDeclarations.TableName);

						if(isRefTypeKnown == null)
						{
							codeMemberProperty.CustomAttributes.Add(
	new CodeAttributeDeclaration(typeof(ForeignKeyDeclarationAttribute).Name,
	new CodeAttributeArgument(new CodePrimitiveExpression(columInfoModel.ForgeinKeyDeclarations.TargetColumn)),
	new CodeAttributeArgument(new CodePrimitiveExpression(columInfoModel.ForgeinKeyDeclarations.TableName))));
						}
						else
						{
							codeMemberProperty.CustomAttributes.Add(
	new CodeAttributeDeclaration(typeof(ForeignKeyDeclarationAttribute).Name,
	new CodeAttributeArgument(new CodePrimitiveExpression(columInfoModel.ForgeinKeyDeclarations.TargetColumn)),
	new CodeAttributeArgument(new CodeTypeOfExpression(isRefTypeKnown.GetClassName()))));
						}
					}

					if (columInfoModel.ColumnInfo.TargetType2 == "Timestamp")
					{
						codeMemberProperty.CustomAttributes.Add(
						   new CodeAttributeDeclaration(typeof(RowVersionAttribute).Name));
					}
				}

				if (tableInfoModel.CreateFallbackProperty)
				{
					compiler.AddFallbackProperty();
				}
			}

			foreach (var proc in _storedProcs)
			{
				if (proc.Exclude)
					continue;

				var targetCsName = proc.GetClassName();
				var generatedClass = new ClassCompiler(TargetDir, targetCsName, true);
				classes.Add(generatedClass);

				generatedClass.TargetName = proc.NewTableName;
				if (proc.Parameter.ParamaterSpParams != null)
					foreach (var spParamter in proc.Parameter.ParamaterSpParams)
					{
						var targetType = DbTypeToCsType.GetClrType(spParamter.Type);
						var spcName = spParamter.Parameter.Replace("@","");
						generatedClass.AddProperty(spcName, targetType);
					}
			}

			foreach (var codeTypeDeclaration in classes)
			{
				Console.WriteLine("Create {0}", codeTypeDeclaration.Name);
				codeTypeDeclaration.CompileClass();
			}

			Console.WriteLine("Created all files");            
		}

		public string SqlVersion { get; set; }

		public bool Is2000
		{
			get
			{
				return _is2000;
			}

			set
			{
				_is2000 = value;
			}
		}

		public bool Is2014
		{
			get
			{
				return _is2014;
			}

			set
			{
				_is2014 = value;
			}
		}

		private bool _is2000;
		private bool _is2014;
		private List<TableInfoModel> _views;
		private List<StoredPrcInfoModel> _storedProcs;

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