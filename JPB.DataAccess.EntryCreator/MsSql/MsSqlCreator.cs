using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using System.CodeDom;
using System.CodeDom.Compiler;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.EntryCreator.MsSql
{
    public class MsSqlCreator : IEntryCreator
    {
        public static DbAccessLayer Manager;
        List<TableInfoModel> _tableNames;

        public string TargetDir { get; set; }

        public void CreateEntrys(string connection, string outputPath, string database)
        {
            TargetDir = outputPath;
            Manager = new DbAccessLayer(DbTypes.MsSql, connection);
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
            Console.WriteLine(@"\autoGenNames  Defines all names after a common naming convention");
            Console.WriteLine(@"\add           Adds elements to existing cs classes");
            Console.WriteLine(@"    Options: ");
            Console.WriteLine(@"            \ctor [\r]   Adds a loader Constructor to all classes and if \r is set existing ctors that impliments IDataReader will be overwritten");
            RenderMenuAction();
        }

        private void RenderMenuAction()
        {
            var readLine = Console.ReadLine();
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
                    case @"\compile":
                        RenderCompiler();
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
                        break;
                }
            }

            Console.WriteLine("Done ... end of program");
            Console.ReadKey();
        }

        private void RenderCtorCompiler(bool replaceExisting)
        {
            var files = Directory.GetFiles(TargetDir, "*.cs");

            var provider = CodeDomProvider.CreateProvider("CSharp");
            foreach (var file in files)
            {
                var codeCompileUnit = provider.Parse(new StreamReader(file));

                foreach (var ns in codeCompileUnit.Namespaces.Cast<CodeNamespace>())
                {
                    foreach (var type in ns.Types.Cast<CodeTypeDeclaration>())
                    {
                        bool createCtor = false;
                        var ctor = type.Members.Cast<CodeTypeMember>().Where(s => s is CodeConstructor).Cast<CodeConstructor>().Where(s => s != null).ToArray();

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
                                    type.Members.Remove(ctorWithIdataRecord);
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
                                type.Members.Cast<CodeMemberProperty>()
                                    .Where(s => s != null)
                                    .ToArray()
                                    .Select(s => new Tuple<string, Type>(s.Name, Type.GetType(s.Type.BaseType)));

                            var generateTypeConstructor = GenerateTypeConstructor(propertys.ToDictionary(f =>
                            {
                                var property =
                                    type.Members.Cast<CodeMemberProperty>().FirstOrDefault(e => e.Name == f.Item1);

                                var codeAttributeDeclaration = property.CustomAttributes.Cast<CodeAttributeDeclaration>()
                                    .FirstOrDefault(e => e.AttributeType.BaseType == typeof(ForModel).FullName);
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

                            type.Members.Insert(0, generateTypeConstructor);
                        }
                    }
                }
            }
        }
        //        foreach (var columInfoModel in tableInfoModel.ColumnInfos)
        //{
        //    var codeIndexerExpression = new CodeIndexerExpression(new CodeVariableReferenceExpression("record"),
        //        new CodePrimitiveExpression(columInfoModel.ColumnInfo.ColumnName));
        //    var castExp = new CodeCastExpression(columInfoModel.ColumnInfo.TargetType, codeIndexerExpression);
        //    var setExpr = new CodeAssignStatement(new CodeVariableReferenceExpression(columInfoModel.GetPropertyName()), castExp);

        //    ctor.Statements.Add(setExpr);
        //}
        private CodeConstructor GenerateTypeConstructor(IDictionary<string, Tuple<string, Type>> propertyToDbColumn)
        {
            var ctor = new CodeConstructor();
            ctor.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(ObjectFactoryMethodAttribute).Name));
            ctor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IDataRecord).Name, "record"));
            ctor.Attributes = MemberAttributes.Public;

            foreach (var columInfoModel in propertyToDbColumn)
            {
                var codeIndexerExpression = new CodeIndexerExpression(new CodeVariableReferenceExpression("record"), new CodePrimitiveExpression(columInfoModel.Key));
                var castExp = new CodeCastExpression(columInfoModel.Value.Item2, codeIndexerExpression);
                var setExpr = new CodeAssignStatement(new CodeVariableReferenceExpression(columInfoModel.Value.Item1), castExp);

                ctor.Statements.Add(setExpr);
            }

            return ctor;
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
            Console.ReadKey();
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

            Console.WriteLine(@"\ForModel   [\c] [ColumnName] [[NewName] | [\d]]  ");
            Console.WriteLine(@"        Adds a ForModelAttribute to a Property or class with \c. deletes it with \d");
            Console.WriteLine("Example:");
            Console.WriteLine(@"        \ForModel \c NewTableName");
            Console.WriteLine("             Adding a ForModel Attribute to the generated class");
            Console.WriteLine(@"        \ForModel ID_Column NewName");
            Console.WriteLine("             Adding a ForModel Attribute to the generated Property with the value NewName");
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

            var readLine = Console.ReadLine();
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

            Console.ReadKey();
            Console.Clear();
            RenderTableMenu(selectedTable);
        }
        
        private void RenderCompiler()
        {
            Console.Clear();
            Console.WriteLine("Start compiling with selected options");

            var copyrightBuilder = new StringBuilder();

            copyrightBuilder.AppendLine("o--------------------------------o");
            copyrightBuilder.AppendLine("| Made by Jean - Pierre Bachmann |");
            copyrightBuilder.AppendLine("| Visit my Github page for more  |");
            copyrightBuilder.AppendLine("|              infos             |");
            copyrightBuilder.AppendLine("|  https://github.com/JPVenson   |");
            copyrightBuilder.AppendLine("|              Email:            |");
            copyrightBuilder.AppendLine("|  jean-pierre_bachmann@live.de  |");
            copyrightBuilder.AppendLine("o--------------------------------o");

            Console.WriteLine(copyrightBuilder.ToString());

            var provider = CodeDomProvider.CreateProvider("CSharp");

            var comments = copyrightBuilder.ToString().Split('\n').Select(s => new CodeCommentStatement(s)).Concat(new []
            {
                new CodeCommentStatement("Created by " + Environment.UserDomainName + @"\" + Environment.UserName),
                new CodeCommentStatement("Created at " + DateTime.Now.ToString("yyyy MMMM dd"))
            }).ToArray();

            var classes = new List<CodeTypeDeclaration>();


            foreach (var tableInfoModel in _tableNames.Concat(_views))
            {
                if (tableInfoModel.Exclude)
                    continue;

                var targetCsName = tableInfoModel.GetClassName();

                var generatedClass = new CodeTypeDeclaration(targetCsName);
                classes.Add(generatedClass);

                generatedClass.Comments.AddRange(comments);
                
                generatedClass.IsClass = true;

                var generatedCodeAttribute = new GeneratedCodeAttribute("MsSqlEntryCreator", "1.0.0.0");
                var codeAttrDecl = new CodeAttributeDeclaration(generatedCodeAttribute.GetType().Name,
                    new CodeAttributeArgument(
                    new CodePrimitiveExpression(generatedCodeAttribute.Tool)),
                        new CodeAttributeArgument(
                            new CodePrimitiveExpression(generatedCodeAttribute.Version)));

                generatedClass.CustomAttributes.Add(codeAttrDecl);
                if (!string.IsNullOrEmpty(tableInfoModel.NewTableName))
                {
                    var forModel = new ForModel(tableInfoModel.Info.TableName);
                    var codeAttributeDeclaration = new CodeAttributeDeclaration(forModel.GetType().Name,
                        new CodeAttributeArgument(new CodePrimitiveExpression(forModel.AlternatingName)));
                    generatedClass.CustomAttributes.Add(codeAttributeDeclaration);
                    generatedClass.Name = tableInfoModel.NewTableName;

                }
                else
                {
                    generatedClass.Name = tableInfoModel.Info.TableName;
                }

                if (tableInfoModel.CreateSelectFactory)
                {
                    var generateTypeConstructor = GenerateTypeConstructor(tableInfoModel.ColumnInfos.ToDictionary(s => s.ColumnInfo.ColumnName,
                        s => new Tuple<string, Type>(s.GetPropertyName(), s.ColumnInfo.TargetType)));
                    generatedClass.Members.Add(generateTypeConstructor);
                }

                var fields = new List<CodeMemberField>();
                var propertys = new List<CodeMemberProperty>();

                foreach (var columInfoModel in tableInfoModel.ColumnInfos)
                {
                    var property = new CodeMemberProperty();

                    property.Attributes = (MemberAttributes)24578; //Public Final
                    property.HasGet = true;
                    property.HasSet = true;

                    var targetType = columInfoModel.ColumnInfo.TargetType;
                    if (columInfoModel.ColumnInfo.Nullable)
                    {
                        var type = typeof(Nullable<>);
                        if (columInfoModel.ColumnInfo.TargetType.IsClass)
                        {
                            try
                            {
                                targetType = type.MakeGenericType(columInfoModel.ColumnInfo.TargetType);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    property.Type = new CodeTypeReference(targetType);
                    var codeAttributeDeclarationCollection = new CodeAttributeDeclarationCollection();
                    if (!string.IsNullOrEmpty(columInfoModel.NewColumnName))
                    {
                        var forModel = new ForModel(columInfoModel.ColumnInfo.ColumnName);
                        codeAttributeDeclarationCollection.Add(new CodeAttributeDeclaration(forModel.GetType().Name,
                            new CodeAttributeArgument(new CodePrimitiveExpression(forModel.AlternatingName))));
                        property.Name = columInfoModel.NewColumnName;
                    }
                    else
                    {
                        property.Name = columInfoModel.ColumnInfo.ColumnName;
                    }

                    var memberName = char.ToLower(property.Name[0]) + property.Name.Substring(1);
                    memberName = memberName.Insert(0, "_");

                    var field = new CodeMemberField()
                    {
                        Name = memberName,
                        Type = new CodeTypeReference(targetType),
                        Attributes = MemberAttributes.Private
                    };

                    property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName)));
                    property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName), new CodePropertySetValueReferenceExpression()));

                    if (columInfoModel.PrimaryKey)
                    {
                        codeAttributeDeclarationCollection.Add(
                            new CodeAttributeDeclaration(typeof(PrimaryKeyAttribute).Name));
                    }
                    if (columInfoModel.ColumnInfo.TargetType2 == SqlDbType.Timestamp)
                    {
                        codeAttributeDeclarationCollection.Add(
                           new CodeAttributeDeclaration(typeof(RowVersionAttribute).Name));
                    }
                    property.CustomAttributes = codeAttributeDeclarationCollection;
                    fields.Add(field);
                    propertys.Add(property);
                }

                if (tableInfoModel.CreateFallbackProperty)
                {
                    var property = new CodeMemberProperty();

                    property.HasGet = true;
                    property.HasSet = true;
                    property.Type = new CodeTypeReference(typeof(Dictionary<string, object>));
                    var codeAttributeDeclarationCollection = new CodeAttributeDeclarationCollection();
                    property.Name = "FallbackDictorary";
                    var memberName = "_fallbackDictorary";

                    var field = new CodeMemberField()
                    {
                        Name = memberName,
                        Type = new CodeTypeReference(typeof(Dictionary<string, object>)),
                        Attributes = MemberAttributes.Private
                    };

                    property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName)));
                    property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName), new CodePropertySetValueReferenceExpression()));
                    var fallbackAtt = new LoadNotImplimentedDynamicAttribute();
                    codeAttributeDeclarationCollection.Add(new CodeAttributeDeclaration(fallbackAtt.GetType().Name));
                    property.CustomAttributes = codeAttributeDeclarationCollection;
                    propertys.Add(property);
                    fields.Add(field);
                }

                foreach (var codeMemberField in fields)
                {
                    generatedClass.Members.Add(codeMemberField);
                }

                foreach (var codeMemberProperty in propertys)
                {
                    generatedClass.Members.Add(codeMemberProperty);
                }
              
                //if (tableInfoModel.CreateSelectFactory)
                //{
                //    provider.GenerateCodeFromType(generatedClass, writer, new CodeGeneratorOptions());
                //    var compileAssemblyFromDom = provider.CompileAssemblyFromDom(new CompilerParameters(), new CodeSnippetCompileUnit(writer.ToString()));

                //    var firstOrDefault = Assembly.LoadFile(compileAssemblyFromDom.PathToAssembly).DefinedTypes.FirstOrDefault();
                //    var selectAttribute = new SelectFactoryAttribute(DbAccessLayer.CreateSelect(firstOrDefault));

                //    generatedClass.CustomAttributes.Add(new CodeAttributeDeclaration(selectAttribute.GetType().Name,
                //        new CodeAttributeArgument(new CodePrimitiveExpression(selectAttribute.Query))));
                //}


            }

            var dynmaicResultClass = "DynamicStoredProcedureResultSet";

            //create a generic Result entry
            if (_storedProcs.Any())
            {
                var generatedClass = new CodeTypeDeclaration(dynmaicResultClass);
                classes.Add(generatedClass);

                generatedClass.IsClass = true;

                var generatedCodeAttribute = new GeneratedCodeAttribute("MsSqlEntryCreator", "1.0.0.0");
                var codeAttrDecl = new CodeAttributeDeclaration(generatedCodeAttribute.GetType().Name,
                    new CodeAttributeArgument(
                    new CodePrimitiveExpression(generatedCodeAttribute.Tool)),
                        new CodeAttributeArgument(
                            new CodePrimitiveExpression(generatedCodeAttribute.Version)));

                generatedClass.CustomAttributes.Add(codeAttrDecl);

                generatedClass.Comments.AddRange(comments);

                var property = new CodeMemberProperty();

                property.HasGet = true;
                property.HasSet = true;
                property.Type = new CodeTypeReference(typeof(Dictionary<string, object>));
                var codeAttributeDeclarationCollection = new CodeAttributeDeclarationCollection();
                property.Name = "FallbackDictorary";
                var memberName = "_fallbackDictorary";

                var field = new CodeMemberField()
                {
                    Name = memberName,
                    Type = new CodeTypeReference(typeof(Dictionary<string, object>)),
                    Attributes = MemberAttributes.Private
                };

                property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName)));
                property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName), new CodePropertySetValueReferenceExpression()));
                var fallbackAtt = new LoadNotImplimentedDynamicAttribute();
                codeAttributeDeclarationCollection.Add(new CodeAttributeDeclaration(fallbackAtt.GetType().Name));
                property.CustomAttributes = codeAttributeDeclarationCollection;

                generatedClass.Members.Add(property);
                generatedClass.Members.Add(field);
            }

            foreach (var proc in _storedProcs)
            {
                if (proc.Exclude)
                    continue;

                var targetCsName = proc.GetClassName();

                var generatedClass = new CodeTypeDeclaration(targetCsName);
                classes.Add(generatedClass);

                generatedClass.Comments.AddRange(comments);
                generatedClass.TypeParameters.Add(dynmaicResultClass);

                generatedClass.IsClass = true;

                //add Generated Code Attribute
                var generatedCodeAttribute = new GeneratedCodeAttribute("MsSqlEntryCreator", "1.0.0.0");
                var codeAttrDecl = new CodeAttributeDeclaration(generatedCodeAttribute.GetType().Name,
                    new CodeAttributeArgument(
                    new CodePrimitiveExpression(generatedCodeAttribute.Tool)),
                        new CodeAttributeArgument(
                            new CodePrimitiveExpression(generatedCodeAttribute.Version)));
                generatedClass.CustomAttributes.Add(codeAttrDecl);

                var spAttribute = new CodeAttributeDeclaration(typeof(StoredProcedureAttribute).Name);
                generatedClass.CustomAttributes.Add(spAttribute);

                if (!string.IsNullOrEmpty(proc.NewTableName))
                {
                    var forModel = new ForModel(proc.Parameter.TableName);
                    var codeAttributeDeclaration = new CodeAttributeDeclaration(forModel.GetType().Name,
                        new CodeAttributeArgument(new CodePrimitiveExpression(forModel.AlternatingName)));
                    generatedClass.CustomAttributes.Add(codeAttributeDeclaration);
                    generatedClass.Name = proc.NewTableName;

                }
                else
                {
                    generatedClass.Name = proc.Parameter.TableName;
                }

                var fields = new List<CodeMemberField>();
                var propertys = new List<CodeMemberProperty>();

                foreach (var spParamter in proc.Parameter.ParamaterSpParams)
                {
                    var property = new CodeMemberProperty();

                    property.Attributes = (MemberAttributes)24578; //Public Final
                    property.HasGet = true;
                    property.HasSet = true;

                    var targetType = DbTypeToCsType.GetClrType(spParamter.Type);

                    property.Type = new CodeTypeReference(targetType);
                    var codeAttributeDeclarationCollection = new CodeAttributeDeclarationCollection();
                    property.Name = spParamter.Parameter;

                    var memberName = char.ToLower(property.Name[0]) + property.Name.Substring(1);
                    memberName = memberName.Insert(0, "_");

                    var field = new CodeMemberField()
                    {
                        Name = memberName,
                        Type = new CodeTypeReference(targetType),
                        Attributes = MemberAttributes.Private
                    };

                    property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName)));
                    property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName), new CodePropertySetValueReferenceExpression()));

                    property.CustomAttributes = codeAttributeDeclarationCollection;
                    fields.Add(field);
                    propertys.Add(property);
                }

                foreach (var codeMemberField in fields)
                {
                    generatedClass.Members.Add(codeMemberField);
                }

                foreach (var codeMemberProperty in propertys)
                {
                    generatedClass.Members.Add(codeMemberProperty);
                }

                var writer = new StringWriter();

                //Create Caller
                var createFactoryMethod = new CodeMemberMethod();
                createFactoryMethod.Name = "Invoke" + proc.GetClassName();
                createFactoryMethod.ReturnType = new CodeTypeReference(typeof(QueryFactoryResult));
                createFactoryMethod.CustomAttributes.Add(
                    new CodeAttributeDeclaration(typeof(SelectFactoryMehtodAttribute).FullName));

                //Create the Params
                string query = "EXEC " + proc.Parameter.TableName;

                var nameOfListOfParamater = "paramaters";
                var listOfParams = new CodeObjectCreateExpression(typeof(List<IQueryParameter>));
                var listOfParamscreator = new CodeVariableDeclarationStatement(typeof(List<IQueryParameter>), nameOfListOfParamater, listOfParams);
                createFactoryMethod.Statements.Add(listOfParamscreator);

                for (int i = 0; i < proc.Parameter.ParamaterSpParams.Count(); i++)
                {
                    var variable = proc.Parameter.ParamaterSpParams.ElementAt(i);
                    var paramName = "param" + i;
                    query += " @" + paramName + " ";
                    var createParams = new CodeObjectCreateExpression(typeof(QueryParameter),
                        new CodePrimitiveExpression(paramName),
                        new CodeVariableReferenceExpression(variable.Parameter));
                    var addToList = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(nameOfListOfParamater),
                        "Add", createParams);

                    createFactoryMethod.Statements.Add(addToList);
                }

                //Finaly create the instance
                var createFactory = new CodeObjectCreateExpression(typeof(QueryFactoryResult),
                    new CodePrimitiveExpression(query),
                    new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(nameOfListOfParamater), "ToArray"));
                var queryFactoryVariable = new CodeMethodReturnStatement(createFactory);

                createFactoryMethod.Statements.Add(queryFactoryVariable);
                generatedClass.Members.Add(createFactoryMethod);
            }


            foreach (var codeTypeDeclaration in classes)
            {
                var targetFileName = Path.Combine(TargetDir, codeTypeDeclaration.Name + ".cs");
                if (File.Exists(targetFileName))
                    File.Delete(targetFileName);
                var fileStream = File.Create(targetFileName);

                using (fileStream)
                {
                    var writer = new StringWriter();
                    var cp = new CompilerParameters();
                    cp.ReferencedAssemblies.Add("System.dll");
                    cp.ReferencedAssemblies.Add("JPB.DataAccess.dll");
                    var compileUnit = new CodeCompileUnit();
                    var importNameSpace = new CodeNamespace("JPB.DataAccess.EntryCreator.AutoGeneratedEntrys");
                    importNameSpace.Imports.Add(new CodeNamespaceImport("System"));
                    importNameSpace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                    importNameSpace.Imports.Add(new CodeNamespaceImport("System.CodeDom.Compiler"));
                    importNameSpace.Imports.Add(new CodeNamespaceImport("System.Linq"));
                    importNameSpace.Imports.Add(new CodeNamespaceImport("System.Data"));
                    importNameSpace.Imports.Add(new CodeNamespaceImport("JPB.DataAccess.ModelsAnotations"));
                    importNameSpace.Types.Add(codeTypeDeclaration);
                    compileUnit.Namespaces.Add(importNameSpace);

                    provider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions()
                    {
                        BlankLinesBetweenMembers = true,
                        VerbatimOrder = true,
                        ElseOnClosing = true
                    });

                    Console.WriteLine("Generated class" + codeTypeDeclaration.Name);
                    var csResult = Encoding.Unicode.GetBytes(writer.ToString());
                    fileStream.Write(csResult, 0, csResult.Length);
                }
            }

            Console.WriteLine("Created all files");

            Console.ReadKey();
        }

        public string SqlVersion { get; set; }

        private bool Is2000;
        private bool Is2014;
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
//              csCreator.Append("[ForModel(");
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