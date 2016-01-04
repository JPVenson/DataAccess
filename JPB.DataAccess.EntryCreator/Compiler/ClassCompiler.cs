using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using JPB.DataAccess.Config;
using JPB.DataAccess.EntityCreator.MsSql;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;
using Microsoft.CSharp;
using System.Reflection;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.EntityCreator.Compiler
{
	public class ClassCompiler
	{
		static ClassCompiler()
		{
			Provider = new CSharpCodeProvider();
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

		public string Namespace { get; set; }

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
		public const string AttrbuteHeader = "JPB.DataAccess.EntityCreator.MsSql.MsSqlCreator";

		public static StringBuilder CreateHeader()
		{
			var copyrightBuilder = new StringBuilder();
			return copyrightBuilder;

			//copyrightBuilder.AppendLine("o--------------------------------o");
			//copyrightBuilder.AppendLine("| Made by Jean - Pierre Bachmann |");
			//copyrightBuilder.AppendLine("| Visit my Github page for more  |");
			//copyrightBuilder.AppendLine("|              infos             |");
			//copyrightBuilder.AppendLine("|  https://github.com/JPVenson/  |");
			//copyrightBuilder.AppendLine("|            DataAccess          |");
			//copyrightBuilder.AppendLine("|              Email:            |");
			//copyrightBuilder.AppendLine("|  jean-pierre_bachmann@live.de  |");
			//copyrightBuilder.AppendLine("o--------------------------------o");

			//return copyrightBuilder;
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

			var copyright = Encoding.Default.GetBytes(copyrightBuilder.ToString());

			var comments = Encoding.UTF8.GetString(copyright).Split('\n').Select(s => new CodeCommentStatement(s)).Concat(new[]
			{
				new CodeCommentStatement("Created by " + Environment.UserDomainName + @"\" + Environment.UserName),
				new CodeCommentStatement("Created on " + DateTime.Now.ToString("yyyy MMMM dd"))
			}).ToArray();

			//Create DOM class
			_base.Name = TargetName;

			//Add Copyright
			_base.Comments.AddRange(comments);

			//Write static members
			_base.IsClass = true;
			_base.TypeAttributes = TypeAttributes.Sealed | TypeAttributes.Public;
			_base.IsPartial = true;

			//Add Code Generated Attribute
			var generatedCodeAttribute = new GeneratedCodeAttribute(AttrbuteHeader, "1.0.0.8");
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

				var writer = new StreamWriter(fileStream, Encoding.UTF8);
				writer.NewLine = Environment.NewLine;

				var cp = new CompilerParameters();
				cp.ReferencedAssemblies.Add("System.dll");
				cp.ReferencedAssemblies.Add("System.Core.dll");
				cp.ReferencedAssemblies.Add("System.Data.dll");
				cp.ReferencedAssemblies.Add("System.Xml.dll");
				cp.ReferencedAssemblies.Add("System.Xml.Linq.dll");
				cp.ReferencedAssemblies.Add("JPB.DataAccess.dll");

				var compileUnit = new CodeCompileUnit();
				CodeNamespace importNameSpace;

				if (string.IsNullOrEmpty(Namespace))
				{
					importNameSpace = new CodeNamespace("JPB.DataAccess.EntryCreator.AutoGeneratedEntrys");
				}
				else
				{
					importNameSpace = new CodeNamespace(Namespace);
				}
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
				writer.Flush();
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
					new CodeAttributeDeclaration(typeof(SelectFactoryMethodAttribute).FullName));

				//Create the Params
				string query = "EXEC " + TargetName;

				var nameOfListOfParamater = "paramaters";
				var listOfParams = new CodeObjectCreateExpression(typeof(List<IQueryParameter>));
				var listOfParamscreator = new CodeVariableDeclarationStatement(typeof(List<IQueryParameter>), nameOfListOfParamater, listOfParams);
				createFactoryMethod.Statements.Add(listOfParamscreator);
				int i = 0;
				foreach (var item in _base.Members)
				{
					if(item is CodeMemberProperty)
					{
						var variable = item as CodeMemberProperty;
						var paramName = "param" + i++;
						query += " @" + paramName + " ";
						var createParams = new CodeObjectCreateExpression(typeof(QueryParameter),
							new CodePrimitiveExpression(paramName),
							new CodeVariableReferenceExpression(variable.Name));
						var addToList =
							new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(nameOfListOfParamater),
								"Add", createParams);

						createFactoryMethod.Statements.Add(addToList);
					}
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
			var codeMemberProperty = AddProperty("FallbackDictorary", typeof(Dictionary<string, object>));
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
			if (info.IsRowVersion)
			{
				var forModel = new RowVersionAttribute();
				codeMemberProperty.CustomAttributes.Add(new CodeAttributeDeclaration(forModel.GetType().Name));
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

		public CodeConstructor GenerateTypeConstructor(bool factory = true)
		{
			var ctor = FactoryHelper.GenerateTypeConstructor(factory);
			_base.Members.Insert(0, ctor);
			return ctor;
		}

		public CodeConstructor GenerateTypeConstructor(
			IEnumerable<KeyValuePair<string, Tuple<string, Type>>> propertyToDbColumn)
		{
			return FactoryHelper.GenerateTypeConstructor(propertyToDbColumn);
		}

		private Type[] _externalTypes;

		internal CodeConstructor GenerateTypeConstructorBasedOnElements()
		{
			var codeMemberProperties = _base.Members.Cast<CodeTypeMember>().Where(s => s is CodeMemberProperty).Cast<CodeMemberProperty>().ToArray();

			var dic = new Dictionary<string, Tuple<string, Type>>();

			foreach (var item in codeMemberProperties)
			{
				var name = item.Name;

				var hasForNameAttribute =
					  item.CustomAttributes.Cast<CodeAttributeDeclaration>()
						  .FirstOrDefault(e => e.AttributeType.BaseType == typeof(ForModel).Name);
				if (hasForNameAttribute != null)
					name = (hasForNameAttribute.Arguments.Cast<CodeAttributeArgument>().FirstOrDefault().Value as CodePrimitiveExpression).Value.ToString();

				var tuple = new Tuple<string, Type>(item.Name, Type.GetType(item.Type.BaseType, null, (e, f, g) =>
				{
					var baseType = Type.GetType(f);
					if (baseType == null)
					{
						if (_externalTypes == null)
						{
							_externalTypes = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.dll")
							.Select(z => Assembly.LoadFile(z))
							.SelectMany(z => z.GetTypes())
							.ToArray();
						}

						baseType = _externalTypes.Single(s => s.FullName == f);
					}

					if (item.Type.ArrayElementType != null)
					{
						baseType = Type.GetType(item.Type.ArrayElementType.BaseType).MakeArrayType();
					}
					else if (item.Type.TypeArguments.Count > 0)
					{
						baseType = typeof(Nullable<>).MakeGenericType(item.Type.TypeArguments.Cast<CodeTypeReference>().Select(z => Type.GetType(z.BaseType)).ToArray());
					}

					return baseType;

				}));
				if (tuple.Item2 == null)
					Console.WriteLine();

				dic.Add(name, tuple);
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
