using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using JPB.DataAccess.EntityCreator.MsSql;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.EntityCreator.Compiler
{
    public class ClassCompiler
    {
        static ClassCompiler()
        {
            Provider = CodeDomProvider.CreateProvider("CSharp");
        }
        
        public string TargetDir { get; private set; }
        public string TargetName { get; set; }
        public string TargetCsName { get; private set; }
        public bool CompileToPrc { get; private set; }

        private bool preCompiled = false;

        public IEnumerable<CodeTypeMember> Members
        {
            get { return _base.Members.Cast<CodeTypeMember>().Where(s => s != null); }
        }

        public CodeTypeMemberCollection MembersFromBase
        {
            get { return _base.Members; }
        }

        public string Name { get { return _base.Name; } }

        private CodeTypeDeclaration _base;

        static readonly CodeDomProvider Provider;

        public ClassCompiler(string targetDir, string targetCsName, bool compileToPrc = false)
        {
            _base = new CodeTypeDeclaration(targetCsName);
            TargetCsName = targetCsName;
            CompileToPrc = compileToPrc;
            TargetDir = targetDir;
        }

        public static ClassCompiler LoadFromFile(string path)
        {
            var codeCompileUnit = Provider.Parse(new StreamReader(path));

            var codeNamespace = codeCompileUnit.Namespaces.Cast<CodeNamespace>().FirstOrDefault();
            bool isPrc = false;

            var codeClass = codeNamespace.Types.Cast<CodeTypeDeclaration>().FirstOrDefault();

            if (codeClass != null)
            {
                isPrc = codeClass.CustomAttributes.Cast<CodeAttributeDeclaration>().FirstOrDefault(s => s.AttributeType.BaseType == typeof(StoredProcedureAttribute).FullName) != null;
            }

            var classCompiler = new ClassCompiler(Path.GetDirectoryName(path), Path.GetFileName(path), isPrc);
            classCompiler._base = codeClass;
            return classCompiler;
        }

        public const string GitURL = "https://github.com/JPVenson/DataAccess";
        public const string AttrbuteHeader = "MsSqlEntryCreator";

        public static StringBuilder CreateHeader()
        {
            var copyrightBuilder = new StringBuilder();

            copyrightBuilder.AppendLine("o--------------------------------o");
            copyrightBuilder.AppendLine("| Made by Jean - Pierre Bachmann |");
            copyrightBuilder.AppendLine("| Visit my Github page for more  |");
            copyrightBuilder.AppendLine("|              infos             |");
            copyrightBuilder.AppendLine("|  https://github.com/JPVenson/  |");
            copyrightBuilder.AppendLine("|            DataAccess          |");
            copyrightBuilder.AppendLine("|              Email:            |");
            copyrightBuilder.AppendLine("|  jean-pierre_bachmann@live.de  |");
            copyrightBuilder.AppendLine("o--------------------------------o");

            return copyrightBuilder;
        }

        public void CompileClass()
        {
            if (CompileToPrc && !preCompiled)
            {
                CompilePrc();
                return;
            }

            if (string.IsNullOrEmpty(TargetName))
            {
                TargetName = TargetCsName;
            }

            //Add Creation Infos
            var copyrightBuilder = CreateHeader();

            var comments = copyrightBuilder.ToString().Split('\n').Select(s => new CodeCommentStatement(s)).Concat(new[]
            {
                new CodeCommentStatement("Created by " + Environment.UserDomainName + @"\" + Environment.UserName),
                new CodeCommentStatement("Created at " + DateTime.Now.ToString("yyyy MMMM dd"))
            }).ToArray();

            //Create DOM class
            _base.Name = TargetName;

            //Add Copyright
            _base.Comments.AddRange(comments);
            
            //Write static members
            _base.IsClass = true;

            //Add Code Generated Attribute
            var generatedCodeAttribute = new GeneratedCodeAttribute(AttrbuteHeader, "1.0.0.1");
            var codeAttrDecl = new CodeAttributeDeclaration(generatedCodeAttribute.GetType().Name,
                new CodeAttributeArgument(
                new CodePrimitiveExpression(generatedCodeAttribute.Tool)),
                    new CodeAttributeArgument(
                        new CodePrimitiveExpression(generatedCodeAttribute.Version)));

            _base.CustomAttributes.Add(codeAttrDecl);
            //Add members

            if (!string.IsNullOrEmpty(TargetName))
            {
                var forModel = new ForModel(TargetName);
                var codeAttributeDeclaration = new CodeAttributeDeclaration(forModel.GetType().Name,
                    new CodeAttributeArgument(new CodePrimitiveExpression(forModel.AlternatingName)));
                _base.CustomAttributes.Add(codeAttributeDeclaration);
                _base.Name = TargetName;
            }

            var targetFileName = Path.Combine(TargetDir, _base.Name + ".cs");
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
                importNameSpace.Types.Add(_base);
                compileUnit.Namespaces.Add(importNameSpace);

                Provider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions()
                {
                    BlankLinesBetweenMembers = false,
                    VerbatimOrder = true,
                    ElseOnClosing = true
                });

                Console.WriteLine("Generated class" + _base.Name);
                var csResult = Encoding.Unicode.GetBytes(writer.ToString());
                fileStream.Write(csResult, 0, csResult.Length);
            }
        }

        public void CompilePrc()
        {
            if (CompileToPrc)
            {
                if (string.IsNullOrEmpty(TargetName))
                {
                    TargetName = TargetCsName;
                }

                var spAttribute = new CodeAttributeDeclaration(typeof(StoredProcedureAttribute).Name);
                _base.CustomAttributes.Add(spAttribute);

                if (_base.TypeParameters.Count == 0)
                {
                    //_base.TypeParameters.Add(new CodeTypeParameter(typeof ().FullName));
                }

                //Create Caller
                var createFactoryMethod = new CodeMemberMethod();
                createFactoryMethod.Name = "Invoke" + TargetName;
                createFactoryMethod.ReturnType = new CodeTypeReference(typeof(QueryFactoryResult));
                createFactoryMethod.CustomAttributes.Add(
                    new CodeAttributeDeclaration(typeof(SelectFactoryMehtodAttribute).FullName));

                //Create the Params
                string query = "EXEC " + TargetName;

                var nameOfListOfParamater = "paramaters";
                var listOfParams = new CodeObjectCreateExpression(typeof(List<IQueryParameter>));
                var listOfParamscreator = new CodeVariableDeclarationStatement(typeof(List<IQueryParameter>), nameOfListOfParamater, listOfParams);
                createFactoryMethod.Statements.Add(listOfParamscreator);
                var codeMemberProperties = _base.Members.Cast<CodeMemberProperty>().ToArray();

                for (int i = 0; i < codeMemberProperties.Count(); i++)
                {
                    var variable = codeMemberProperties.ElementAt(i);
                    var paramName = "param" + i;
                    query += " @" + paramName + " ";
                    var createParams = new CodeObjectCreateExpression(typeof(QueryParameter),
                        new CodePrimitiveExpression(paramName),
                        new CodeVariableReferenceExpression(variable.Name));
                    var addToList =
                        new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(nameOfListOfParamater),
                            "Add", createParams);

                    createFactoryMethod.Statements.Add(addToList);
                }

                //Finaly create the instance
                var createFactory = new CodeObjectCreateExpression(typeof(QueryFactoryResult),
                    new CodePrimitiveExpression(query),
                    new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(nameOfListOfParamater), "ToArray"));
                var queryFactoryVariable = new CodeMethodReturnStatement(createFactory);

                createFactoryMethod.Statements.Add(queryFactoryVariable);
                _base.Members.Add(createFactoryMethod);

                preCompiled = true;
            }
            preCompiled = true;

            CompileClass();
        }

        internal CodeMemberProperty AddFallbackProperty()
        {
            var codeMemberProperty = AddProperty("FallbackDictorary", typeof (Dictionary<string, object>));
            var fallbackAtt = new LoadNotImplimentedDynamicAttribute();
            codeMemberProperty.CustomAttributes.Add(new CodeAttributeDeclaration(fallbackAtt.GetType().Name));
            return codeMemberProperty;
        }

        internal CodeMemberProperty AddProperty(ColumInfoModel info)
        {
            var propertyName = info.GetPropertyName();
            var targetType = info.ColumnInfo.TargetType;
            var codeMemberProperty = AddProperty(propertyName, targetType);

            if (!string.IsNullOrEmpty(info.NewColumnName))
            {
                var forModel = new ForModel(info.ColumnInfo.ColumnName);
                codeMemberProperty.CustomAttributes.Add(new CodeAttributeDeclaration(forModel.GetType().Name, new CodeAttributeArgument(new CodePrimitiveExpression(forModel.AlternatingName))));
            }
            return codeMemberProperty;
        }

        internal CodeMemberProperty AddProperty(string name, Type type)
        {
            var property = new CodeMemberProperty();

            property.Attributes = (MemberAttributes)24578; //Public Final
            property.HasGet = true;
            property.HasSet = true;
            property.Name = name;

            property.Type = new CodeTypeReference(type);

            var memberName = char.ToLower(property.Name[0]) + property.Name.Substring(1);
            memberName = memberName.Insert(0, "_");

            var field = new CodeMemberField()
            {
                Name = memberName,
                Type = new CodeTypeReference(type),
                Attributes = MemberAttributes.Private
            };

            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName)));
            property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName), new CodePropertySetValueReferenceExpression()));

            _base.Members.Add(field);
            _base.Members.Add(property);
            return property;
        }

        internal CodeConstructor GenerateTypeConstructor(IEnumerable<KeyValuePair<string, Tuple<string, Type>>> propertyToDbColumn)
        {
            //Key = Column Name
            //Value = 
                //Value 1 = PropertyName
                //Value 2 = Type


            var codeConstructor = GenerateTypeConstructor();

            foreach (var columInfoModel in propertyToDbColumn)
            {
                var codeIndexerExpression = new CodeIndexerExpression(new CodeVariableReferenceExpression("record"), new CodePrimitiveExpression(columInfoModel.Key));
                var castExp = new CodeCastExpression(columInfoModel.Value.Item2, codeIndexerExpression);
                var setExpr = new CodeAssignStatement(new CodeVariableReferenceExpression(columInfoModel.Value.Item1), castExp);
                codeConstructor.Statements.Add(setExpr);
            }

            return codeConstructor;
        }

        internal CodeConstructor GenerateTypeConstructor()
        {
            var codeConstructor = new CodeConstructor();
            codeConstructor.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(ObjectFactoryMethodAttribute).Name));
            codeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IDataRecord).Name, "record"));
            codeConstructor.Attributes = MemberAttributes.Public;
            _base.Members.Insert(0, codeConstructor);
            return codeConstructor;
        }

        internal CodeConstructor GenerateTypeConstructorBasedOnElements()
        {
            var codeMemberProperties = _base.Members.Cast<CodeMemberProperty>().Where(s => s != null).ToArray();

            var dic = new Dictionary<string, Tuple<string, Type>>();

            foreach (var s in codeMemberProperties)
            {
                var name = s.Name;

                var hasForNameAttribute =
                      s.CustomAttributes.Cast<CodeAttributeDeclaration>()
                          .FirstOrDefault(e => e.AttributeType.BaseType == typeof(ForModel).FullName);
                if (hasForNameAttribute != null)
                    name = hasForNameAttribute.Arguments.Cast<CodeAttributeArgument>().FirstOrDefault().Value.ToString();

                var tuple = new Tuple<string, Type>(s.Name, Type.GetType(s.Type.BaseType));
                dic.Add(name,tuple);
            }

            return
                GenerateTypeConstructor(dic);
        }

        public void Add(CodeTypeMember property)
        {
            _base.Members.Add(property);
        }
    }
}
