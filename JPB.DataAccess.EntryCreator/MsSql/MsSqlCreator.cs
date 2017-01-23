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
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using System.Xml.Serialization;
using JPB.DataAccess.EntityCreator.Core;
using JPB.DataAccess.EntityCreator.Core.Compiler;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.DataAccess.EntityCreator.Core.Poco;

namespace JPB.DataAccess.EntityCreator.MsSql
{
    public class MsSqlCreator : IMsSqlCreator
    {
        public DbAccessLayer Manager;
        public IEnumerable<ITableInfoModel> Tables { get; set; }
        public IEnumerable<Dictionary<int, string>> Enums { get; private set; }
        public IEnumerable<ITableInfoModel> Views { get; set; }
        public IEnumerable<IStoredPrcInfoModel> StoredProcs { get; private set; }

        public string TargetDir { get; set; }
        public bool GenerateConstructor { get; set; }
        public bool GenerateForgeinKeyDeclarations { get; set; }
        public bool GenerateCompilerHeader { get; set; }
        public bool GenerateConfigMethod { get; set; }
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
            Console.WriteLine("Connection OK ... Reading Server Version ...");

            SqlVersion = Manager.RunPrimetivSelect<string>("SELECT SERVERPROPERTY('productversion')").FirstOrDefault();

            Console.WriteLine("Server version is {0}", SqlVersion);

            Console.WriteLine("Reading Tables from {0} ...", databaseName);

            Tables = Manager.Select<TableInformations>().Select(s => new TableInfoModel(s, databaseName, Manager)).ToList();
            Views = Manager.Select<ViewInformation>().Select(s => new TableInfoModel(s, databaseName, Manager)).ToList();
            StoredProcs = Manager.Select<StoredProcedureInformation>().Select(s => new StoredPrcInfoModel(s)).ToList();

            Console.WriteLine("Found {0} Tables, {1} Views, {2} Procedures ... select a Table to see Options or start an Action", Tables.Count(), Views.Count(), StoredProcs.Count());
            Enums = new List<Dictionary<int, string>>();
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
            for (; i < Tables.Count(); i++)
            {
                Console.WriteLine("{0} \t {1}", i, Tables.ToArray()[i].Info.TableName);
            }

            Console.WriteLine("Views:");
            int j = i;
            for (; j < Views.Count() + i; j++)
            {
                Console.WriteLine("{0} \t {1}", j, Views.ToArray()[j - i].Info.TableName);
            }

            Console.WriteLine("Procedures:");
            int k = j;
            for (; k < StoredProcs.Count() + j; k++)
            {
                Console.WriteLine("{0} \t {1}", k, StoredProcs.ToArray()[k - j].Parameter.TableName);
            }

            Console.WriteLine("Actions: ");

            Console.WriteLine(@"[Name | Number]");
            Console.WriteLine("		Edit table");
            Console.WriteLine(@"\compile");
            Console.WriteLine("		Starts the Compiling of all Tables");
            Console.WriteLine(@"\ns");
            Console.WriteLine("		Defines a default Namespace");
            Console.WriteLine(@"\fkGen");
            Console.WriteLine("		Generates ForgeinKeyDeclarations");
            Console.WriteLine(@"\addConfigMethod");
            Console.WriteLine("		Moves all attributes from Propertys and Methods into a single ConfigMethod");
            Console.WriteLine(@"\withAutoCtor");
            Console.WriteLine("		Generates Loader Constructors");
            Console.WriteLine(@"\autoGenNames");
            Console.WriteLine("		Defines all names after a common naming convention");
            Console.WriteLine(@"\addCompilerHeader	");
            Console.WriteLine("		Adds a Timestamp and a created user on each POCO");
            Console.WriteLine(@"\exit");
            Console.WriteLine("		Stops the execution of the program");
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
                    Console.WriteLine("Unvalid number");
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
            GenerateCompilerHeader = !this.GenerateCompilerHeader;
            Console.WriteLine("Compiler header is {0}", GenerateCompilerHeader ? "set" : "unset");
            RenderMenuAction();
        }

        private void SetConfigMethod()
        {
            GenerateConfigMethod = !this.GenerateConfigMethod;
            Console.WriteLine("Compiler header is {0}", GenerateConfigMethod ? "set" : "unset");
            RenderMenuAction();
        }

        private void SetForgeinKeyDeclarationCreation()
        {
            GenerateForgeinKeyDeclarations = !GenerateForgeinKeyDeclarations;
            Console.WriteLine("Auto ForgeinKey Declaration Creation is {0}", GenerateForgeinKeyDeclarations ? "set" : "unset");
            RenderMenuAction();
        }

        private void SetRenderAutoCtor()
        {
            GenerateConstructor = !GenerateConstructor;
            Console.WriteLine("Auto Ctor is {0}", GenerateConstructor ? "set" : "unset");
            RenderMenuAction();
        }

        private void SetNamespace()
        {
            Console.WriteLine("Enter your DefaultNamespace");
            var ns = Program.AutoConsole.GetNextOption();
            Namespace = ns;
            RenderMenuAction();
        }

        private void RenderCtorCompiler(bool replaceExisting)
        {
            Console.WriteLine("UNSUPPORTED");
            Console.WriteLine("UNSUPPORTED");
            Console.WriteLine("UNSUPPORTED");


            //var files = Directory.GetFiles(TargetDir, "*.cs");

            //var provider = CodeDomProvider.CreateProvider("CSharp");
            //foreach (var file in files)
            //{
            //	var loadFromFile = ClassCompiler.LoadFromFile(file);

            //	bool createCtor = false;
            //	var ctor = loadFromFile.Members.Where(s => s is CodeConstructor).Cast<CodeConstructor>().Where(s => s != null).ToArray();

            //	if (ctor.Any())
            //	{
            //		var fullNameOfObjectFactory = typeof(ObjectFactoryMethodAttribute).FullName;
            //		var ctorWithIdataRecord =
            //			ctor.FirstOrDefault(
            //				s =>
            //					s.CustomAttributes.Cast<CodeAttributeDeclaration>()
            //						.Any(e => e.Name == fullNameOfObjectFactory)
            //					&& s.Parameters.Count == 1
            //					&& s.Parameters.Cast<CodeParameterDeclarationExpression>().Any(e => Type.GetType(e.Type.BaseType) == typeof(IDataRecord)));

            //		if (ctorWithIdataRecord != null)
            //		{
            //			if (replaceExisting)
            //			{
            //				loadFromFile.MembersFromBase.Remove(ctorWithIdataRecord);
            //			}
            //			else
            //			{
            //				continue;
            //			}
            //		}
            //		else
            //		{
            //			createCtor = true;
            //		}
            //	}
            //	else
            //	{
            //		createCtor = true;
            //	}

            //	if (createCtor)
            //	{
            //		var propertys =
            //			loadFromFile.Members.Cast<CodeMemberProperty>()
            //				.Where(s => s != null)
            //				.ToArray()
            //				.Select(s => new Tuple<string, Type>(s.Name, Type.GetType(s.Type.BaseType)));

            //		var generateTypeConstructor = new ClassCompiler("", "").GenerateTypeConstructor(propertys.ToDictionary(f =>
            //		{
            //			var property =
            //				loadFromFile.Members.Cast<CodeMemberProperty>().FirstOrDefault(e => e.Name == f.Item1);

            //			var codeAttributeDeclaration = property.CustomAttributes.Cast<CodeAttributeDeclaration>()
            //				.FirstOrDefault(e => e.AttributeType.BaseType == typeof(ForModelAttribute).FullName);
            //			if (codeAttributeDeclaration != null)
            //			{
            //				var firstOrDefault =
            //					codeAttributeDeclaration.Arguments.Cast<CodeAttributeDeclaration>()
            //						.FirstOrDefault();
            //				if (firstOrDefault != null)
            //				{
            //					var codeAttributeArgument = firstOrDefault
            //						.Arguments.Cast<CodeAttributeArgument>()
            //						.FirstOrDefault();
            //					if (codeAttributeArgument != null)
            //					{
            //						var codePrimitiveExpression = codeAttributeArgument
            //							.Value as CodePrimitiveExpression;
            //						if (codePrimitiveExpression != null)
            //							return
            //								codePrimitiveExpression.Value.ToString();
            //					}
            //				}
            //			}
            //			return f.Item1;
            //		}, f =>
            //		{
            //			return f;
            //		}));

            //		loadFromFile.MembersFromBase.Insert(0, generateTypeConstructor);
            //	}
            //}
        }


        public void AutoAlignNames()
        {
            SharedMethods.AutoAlignNames(this.Tables);
            RenderMenuAction();
        }

        private void RenderTableMenu(ITableInfoModel selectedTable)
        {
            Console.WriteLine("Actions:");

            Console.WriteLine(@"\ForModel   [\c] [ColumnName] [[NewName] | [\d]]  ");
            Console.WriteLine(@"        Adds a ForModelAttribute to a Property or class with \c for class. deletes it with \d");
            Console.WriteLine("Example:");
            Console.WriteLine(@"        \ForModelAttribute \c NewTableName");
            Console.WriteLine("             Adding a ForModelAttribute Attribute to the generated class");
            Console.WriteLine(@"        \ForModelAttribute ID_Column NewName");
            Console.WriteLine("             Adding a ForModelAttribute Attribute to the generated Property with the value NewName");
            Console.WriteLine();
            Console.WriteLine(@"\Exclude			[true | false] [ColumnName]");
            Console.WriteLine("         Exclude this table from the Process");
            Console.WriteLine(@"\InsertIgnore		true | false] [ColumnName]");
            Console.WriteLine("         Exclude this column from inserts");
            Console.WriteLine(@"\Enum				[true | false] [ColumnName]");
            Console.WriteLine("         Marks this Column as an ENUM field on the Database. Experimental");
            Console.WriteLine(@"\Fallack			[true | false]]");
            Console.WriteLine("         Should create a LoadNotImplimentedDynamic Property for Not Loaded fieds");
            Console.WriteLine(@"\Createloader		[true | false]]");
            Console.WriteLine("         Should create a Dataloader that loads the Propertys from the IDataRecord");
            Console.WriteLine(@"\CreateSelect		[true | false]]");
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

                            Console.WriteLine("Unvalid Input expected was [ColumnName] [NewName] ");
                            Console.WriteLine();
                        }
                        break;
                    case @"\insertignore":
                        if (parts.Length == 3)
                        {
                            bool result;
                            Boolean.TryParse(parts[1], out result);

                            var column = selectedTable.ColumnInfos.FirstOrDefault(s => s.ColumnInfo.ColumnName == parts[2]);
                            if (column == null)
                            {

                                Console.WriteLine("Unvalid Input expected was  [ColumnName] ");
                                Console.WriteLine();
                                break;
                            }

                            column.InsertIgnore = result;
                        }
                        else
                        {

                            Console.WriteLine("Unvalid Input expected was  true | false ");
                            Console.WriteLine();
                        }
                        break;
                    case @"\enum":
                        if (parts.Length == 3)
                        {
                            bool result;
                            Boolean.TryParse(parts[1], out result);

                            var column = selectedTable.ColumnInfos.FirstOrDefault(s => s.ColumnInfo.ColumnName == parts[2]);
                            if (column == null)
                            {

                                Console.WriteLine("Unvalid Input expected was  [ColumnName] ");
                                Console.WriteLine();
                                break;
                            }

                            if (result == false)
                            {
                                column.EnumDeclaration = null;
                                break;
                            }

                            if (column.ForgeinKeyDeclarations == null)
                            {
                                Console.WriteLine("Declare the Enum:");
                                Console.WriteLine("Format: [Number] [Description]");
                                Console.WriteLine("Example: '1 Valid'");
                                Console.WriteLine("Type ok to submit");
                                Console.WriteLine("Type cancel to revert");

                                column.EnumDeclaration = new EnumDeclarationModel();

                                Console.WriteLine("Name of Enum:");
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
                                            Console.WriteLine("Added Enum member {0} = {1}", option[1], enumNumberResult);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Invalid Enum number Supplyed");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid Enum member Supplyed");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Enum is ForgeinKey.");
                                Console.WriteLine("Read data from Database to autogenerate Enum");
                                Console.WriteLine("Reading table: '{0}'", column.ForgeinKeyDeclarations.TableName);

                                var tableContent = Manager.Select<DynamicTableContentModel>(new object[] { column.ForgeinKeyDeclarations.TableName });

                                if (!tableContent.Any())
                                {
                                    Console.WriteLine("The Enum table '{0}' does not contain any data", column.ForgeinKeyDeclarations.TableName);
                                    break;
                                }

                                if (tableContent.First().DataHolder.Count > 2)
                                {
                                    Console.WriteLine("The Enum table '{0}' contains more then 2 columns", column.ForgeinKeyDeclarations.TableName);
                                    break;
                                }

                                if (!tableContent.Any(s => s.DataHolder.Any(f => f.Value is int)))
                                {
                                    Console.WriteLine("The Enum table '{0}' does not contains exactly one column of type int", column.ForgeinKeyDeclarations.TableName);
                                    break;
                                }

                                if (!tableContent.Any(s => s.DataHolder.Any(f => f.Value is string)))
                                {
                                    Console.WriteLine("The Enum table '{0}' does not contains exactly one column of type int", column.ForgeinKeyDeclarations.TableName);
                                    break;
                                }

                                column.EnumDeclaration = new EnumDeclarationModel();
                                column.EnumDeclaration.Name = column.ForgeinKeyDeclarations.TableName + "LookupValues";

                                foreach (var item in tableContent)
                                {
                                    var pk = (int)item.DataHolder.FirstOrDefault(s => s.Value is int).Value;
                                    var value = (string)item.DataHolder.FirstOrDefault(s => s.Value is string).Value;
                                    column.EnumDeclaration.Values.Add(pk, value);
                                    Console.WriteLine("Adding Enum member '{0}' = '{1}'", value, pk);
                                }
                            }
                        }
                        else
                        {

                            Console.WriteLine("Unvalid Input expected was  true | false ");
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
                            if (parts.Length == 3)
                            {
                                bool result;
                                Boolean.TryParse(parts[1], out result);

                                var column = selectedTable.ColumnInfos.FirstOrDefault(s => s.ColumnInfo.ColumnName == parts[2]);
                                if (column == null)
                                {

                                    Console.WriteLine("Unvalid Input expected was  [ColumnName] ");
                                    Console.WriteLine();
                                    break;
                                }

                                column.Exclude = result;
                            }
                            else
                            {

                                Console.WriteLine("Unvalid Input expected was  true | false ");
                                Console.WriteLine();
                            }
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

                            Console.WriteLine("Unvalid Input expected was  true | false ");
                            Console.WriteLine();
                        }
                        break;

                    case @"\createloader":
                        if (parts.Length == 2)
                        {
                            bool result;
                            Boolean.TryParse(parts[1], out result);
                            selectedTable.CreateDataRecordLoader = result;
                        }
                        else
                        {

                            Console.WriteLine("Unvalid Input expected was  true | false ");
                            Console.WriteLine();
                        }
                        break;
                    case @"\createSelect":
                        if (parts.Length == 2)
                        {
                            bool result;
                            Boolean.TryParse(parts[1], out result);
                            selectedTable.CreateSelectFactory = result;
                        }
                        else
                        {

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
                            Console.WriteLine("\t Is Enum Type =		    {0}", columnInfo.EnumDeclaration != null);
                        }
                        break;

                    case @"\back":
                        RenderMenu();
                        return;
                    default:
                        break;
                }

            RenderTableMenu(selectedTable);
        }

        public void Compile()
        {
            Console.WriteLine("Start compiling with selected options");

            var elements = Tables.Concat(Views);

            //foreach (var item in elements.Where(f => f.ColumnInfos.Any(e => e.EnumDeclaration != null)))
            //{
            //	foreach (var column in item.ColumnInfos)
            //	{
            //		if (column.EnumDeclaration != null)
            //		{
            //			var targetCsName = column.EnumDeclaration.Name;
            //			var compiler = new EnumCompiler(TargetDir, targetCsName);
            //			compiler.CompileHeader = this.GenerateCompilerHeader;
            //			compiler.Namespace = Namespace;
            //			compiler.GenerateConfigMethod = GenerateConfigMethod;
            //			foreach (var enumMember in column.EnumDeclaration.Values)
            //			{
            //				compiler.Add(new CodeMemberField()
            //				{
            //					Name = enumMember.Value,
            //					InitExpression = new CodePrimitiveExpression(enumMember.Key)
            //				});
            //			}

            //			compiler.Compile(new List<ColumInfoModel>());
            //		}
            //	}
            //}

            foreach (var tableInfoModel in elements)
            {
                SharedMethods.CompileTable(tableInfoModel, this);
            }

            foreach (var proc in StoredProcs)
            {
                if (proc.Exclude)
                    continue;

                var targetCsName = proc.GetClassName();
                var compiler = new ProcedureCompiler(TargetDir, targetCsName);
                compiler.CompileHeader = this.GenerateCompilerHeader;
                compiler.Namespace = Namespace;
                compiler.GenerateConfigMethod = GenerateConfigMethod;
                compiler.TableName = proc.NewTableName;
                if (proc.Parameter.ParamaterSpParams != null)
                    foreach (var spParamter in proc.Parameter.ParamaterSpParams)
                    {
                        var targetType = DbTypeToCsType.GetClrType(spParamter.Type);
                        var spcName = spParamter.Parameter.Replace("@", "");
                        compiler.AddProperty(spcName, targetType);
                    }

                Console.WriteLine("Compile Procedure {0}", compiler.Name);
                compiler.Compile(new List<ColumInfoModel>());
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