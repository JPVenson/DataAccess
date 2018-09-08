#region

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using Microsoft.CSharp;

#endregion

namespace JPB.DataAccess.DbInfoConfig
{
	/// <summary>
	///     Only for internal use
	/// </summary>
	public class FactoryHelper
	{
		/// <summary>
		///     Creates a new CodeDOM Element that is ether a Factory(Public Static Object Factory) or an constructor
		///     Both functions accepts only one Argument of type IDataRecord
		///     Both functions are empty
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		public static CodeMemberMethod GenerateTypeConstructor(bool factory = false)
		{
			CodeMemberMethod pocoCreator;
			if (factory)
			{
				pocoCreator = new CodeMemberMethod();
				pocoCreator.Name = "Factory";
				pocoCreator.Attributes = MemberAttributes.Static;
			}
			else
			{
				pocoCreator = new CodeConstructor();
				pocoCreator.Attributes = MemberAttributes.Public;
			}
			pocoCreator.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(ObjectFactoryMethodAttribute).Name));
			pocoCreator.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IDataRecord).Name, "record"));
			pocoCreator.Attributes |= MemberAttributes.Public;
			return pocoCreator;
		}

		/// <summary>
		///     Generates a Constructor with a Full Ado.Net constructor
		/// </summary>
		/// <param name="target"></param>
		/// <param name="settings"></param>
		/// <param name="importNameSpace"></param>
		/// <returns></returns>
		public static CodeMemberMethod GenerateConstructor(DbClassInfoCache target, FactoryHelperSettings settings,
			CodeNamespace importNameSpace)
		{
			var codeConstructor = GenerateTypeConstructor();
			GenerateBody(target, settings, importNameSpace, codeConstructor, new CodeBaseReferenceExpression());
			return codeConstructor;
		}

		/// <summary>
		///     Generates a Function with a Full ado.net constructor beavior.
		///     It Creates a new Instance of
		///     <paramref name="target"></paramref>
		///     and then fills all public properties
		/// </summary>
		/// <param name="target"></param>
		/// <param name="settings"></param>
		/// <param name="importNameSpace"></param>
		/// <returns></returns>
		public static CodeMemberMethod GenerateFactory(DbClassInfoCache target, FactoryHelperSettings settings,
			CodeNamespace importNameSpace)
		{
			var codeFactory = GenerateTypeConstructor(true);
			var super = new CodeVariableReferenceExpression("super");

			var pocoVariable = new CodeVariableDeclarationStatement(target.Type, "super");
			codeFactory.Statements.Add(pocoVariable);

			var codeAssignment = new CodeAssignStatement(super, new CodeObjectCreateExpression(target.Type));
			codeFactory.Statements.Add(codeAssignment);

			GenerateBody(target, settings, importNameSpace, codeFactory, super);
			codeFactory.ReturnType = new CodeTypeReference(target.Type);
			codeFactory.Statements.Add(new CodeMethodReturnStatement(super));

			return codeFactory;
		}

		/// <summary>
		///     Creates the short code type reference.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="imports">The imports.</param>
		/// <returns></returns>
		public static CodeTypeReference CreateShortCodeTypeReference(Type type, CodeNamespaceImportCollection imports)
		{
			var result = new CodeTypeReference(type);

			Shortify(result, type, imports);

			return result;
		}

		/// <summary>
		///     Shortifies the specified type reference.
		/// </summary>
		/// <param name="typeReference">The type reference.</param>
		/// <param name="type">The type.</param>
		/// <param name="imports">The imports.</param>
		public static void Shortify(CodeTypeReference typeReference, Type type, CodeNamespaceImportCollection imports)
		{
			if (typeReference.ArrayRank > 0)
			{
				Shortify(typeReference.ArrayElementType, type, imports);
				return;
			}

			if (type.Namespace != null && imports.Cast<CodeNamespaceImport>()
					.Any(cni => cni.Namespace == type.Namespace))
			{
				var prefix = type.Namespace + '.';

				if (prefix != null)
				{
					var pos = typeReference.BaseType.IndexOf(prefix);
					if (pos == 0)
					{
						typeReference.BaseType = typeReference.BaseType.Substring(prefix.Length);
					}
				}
			}
		}

		/// <summary>
		///     Creates a new body in stlye of an Ado.net Constructor and attaches it to the <paramref name="target" />
		/// </summary>
		/// <param name="properties"></param>
		/// <param name="settings"></param>
		/// <param name="importNameSpace"></param>
		/// <param name="container"></param>
		/// <param name="target"></param>
		public static void GenerateBody(Dictionary<string, DbPropertyInfoCache> properties,
			FactoryHelperSettings settings,
			CodeNamespace importNameSpace,
			CodeMemberMethod container,
			CodeExpression target)
		{
			foreach (var propertyInfoCache in properties.Values)
			{
				propertyInfoCache.Refresh();

				var columnName = propertyInfoCache.DbName;

				if (settings.EnforcePublicPropertys)
				{
					if (propertyInfoCache.Getter.MethodInfo.IsPrivate)
					{
						throw new AccessViolationException(string.Format(
						"The Getter of {0} is private. Full creation cannot be enforced", propertyInfoCache.PropertyName));
					}
					if (propertyInfoCache.Setter.MethodInfo.IsPrivate)
					{
						throw new AccessViolationException(string.Format(
						"The Setter of {0} is private. Full creation cannot be enforced", propertyInfoCache.PropertyName));
					}
				}

				var codeIndexerExpression = new CodeIndexerExpression(new CodeVariableReferenceExpression("record"),
					new CodePrimitiveExpression(columnName));

				var variableName = columnName.ToLower();
				CodeVariableDeclarationStatement bufferVariable = null;

				//var propertyRef = new CodeVariableReferenceExpression(propertyInfoCache.PropertyName);

				var refToProperty = new CodeFieldReferenceExpression(target, propertyInfoCache.PropertyName);

				var attributes = propertyInfoCache.Attributes;
				var valueConverterAttributeModel =
					attributes.FirstOrDefault(s => s.Attribute is ValueConverterAttribute);
				var isXmlProperty = attributes.FirstOrDefault(s => s.Attribute is FromXmlAttribute);
				CodeVariableReferenceExpression uncastLocalVariableRef = null;

				if (isXmlProperty != null)
				{
					bufferVariable = new CodeVariableDeclarationStatement(typeof(object), variableName);
					container.Statements.Add(bufferVariable);
					uncastLocalVariableRef = new CodeVariableReferenceExpression(variableName);
					var buffAssignment = new CodeAssignStatement(uncastLocalVariableRef, codeIndexerExpression);
					container.Statements.Add(buffAssignment);

					var checkXmlForNull = new CodeConditionStatement();
					checkXmlForNull.Condition = new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression(variableName),
						CodeBinaryOperatorType.IdentityInequality,
						new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("System.DBNull"), "Value"));
					container.Statements.Add(checkXmlForNull);

					var xmlRecordType = new CodeTypeReferenceExpression(typeof(XmlDataRecord));
					importNameSpace.Imports.Add(new CodeNamespaceImport(typeof(DbClassInfoCache).Namespace));
					importNameSpace.Imports.Add(new CodeNamespaceImport(typeof(DataConverterExtensions).Namespace));
					importNameSpace.Imports.Add(new CodeNamespaceImport(typeof(DbConfigHelper).Namespace));

					if (propertyInfoCache.CheckForListInterface())
					{
						var typeArgument = propertyInfoCache.PropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();
						var codeTypeOfListArg = new CodeTypeOfExpression(typeArgument);

						var instanceHelper = new CodeMethodInvokeExpression(
							new CodeTypeReferenceExpression(typeof(NonObservableDbCollection<>).MakeGenericType(typeArgument)),
							"FromXml",
							new CodeCastExpression(typeof(string), new CodeVariableReferenceExpression(variableName)));

						//var tryParse = new CodeMethodInvokeExpression(xmlRecordType,
						//	"TryParse",
						//	new CodeCastExpression(typeof(string), uncastLocalVariableRef),
						//	codeTypeOfListArg,
						//	new CodePrimitiveExpression(false));

						//var xmlDataRecords = new CodeMethodInvokeExpression(tryParse, "CreateListOfItems");
						//var getClassInfo = new CodeMethodInvokeExpression(codeTypeOfListArg, "GetClassInfo");

						//var xmlRecordsToObjects = new CodeMethodInvokeExpression(xmlDataRecords, "Select",
						//	new CodeMethodReferenceExpression(getClassInfo, "SetPropertysViaReflection"));

						//CodeObjectCreateExpression collectionCreate;
						//if (typeArgument != null && (typeArgument.IsClass && typeArgument.GetInterface("INotifyPropertyChanged") != null))
						//{
						//	collectionCreate = new CodeObjectCreateExpression(typeof(DbCollection<>).MakeGenericType(typeArgument),
						//		xmlRecordsToObjects);
						//}
						//else
						//{
						//	collectionCreate = new CodeObjectCreateExpression(typeof(NonObservableDbCollection<>).MakeGenericType(typeArgument),
						//		xmlRecordsToObjects);
						//}

						var setExpr = new CodeAssignStatement(refToProperty, instanceHelper);
						checkXmlForNull.TrueStatements.Add(setExpr);
					}
					else
					{
						var typeofProperty = new CodeTypeOfExpression(propertyInfoCache.PropertyType);
						var getClassInfo = new CodeMethodInvokeExpression(typeofProperty, "GetClassInfo");

						var tryParse = new CodeMethodInvokeExpression(xmlRecordType,
							"TryParse",
							new CodeCastExpression(typeof(string), uncastLocalVariableRef),
							typeofProperty,
							new CodePrimitiveExpression(true));

						var setProps = new CodeMethodInvokeExpression(getClassInfo, "SetPropertysViaReflection", tryParse,
							new CodeSnippetExpression("null"),
							new CodeSnippetExpression("null"));
						var setExpr = new CodeAssignStatement(refToProperty,
							new CodeCastExpression(propertyInfoCache.PropertyType, setProps));
						checkXmlForNull.TrueStatements.Add(setExpr);
					}
				}
				else
				{
					//Should the SQL value be converted
					if (valueConverterAttributeModel != null)
					{
						//create object buff123;
						bufferVariable = new CodeVariableDeclarationStatement(typeof(object), variableName);
						container.Statements.Add(bufferVariable);
						//Assing buff123 = record[x]
						uncastLocalVariableRef = new CodeVariableReferenceExpression(variableName);
						var buffAssignment = new CodeAssignStatement(uncastLocalVariableRef, codeIndexerExpression);
						container.Statements.Add(buffAssignment);

						var converter = valueConverterAttributeModel.Attribute as ValueConverterAttribute;
						//Create the converter and then convert the value before everything else

						importNameSpace.Imports.Add(new CodeNamespaceImport(converter.Converter.Namespace));
						var converterCall = new CodeObjectCreateExpression(converter.Converter);
						var converterInstanceCall = new CodeMethodInvokeExpression(converterCall, "Convert",
							new CodeVariableReferenceExpression(variableName),
							new CodeTypeOfExpression(propertyInfoCache.PropertyType),
							new CodePrimitiveExpression(converter.Parameter),
							new CodeVariableReferenceExpression("System.Globalization.CultureInfo.CurrentCulture"));
						var codeAssignment = new CodeAssignStatement(new CodeVariableReferenceExpression(variableName),
							converterInstanceCall);
						container.Statements.Add(codeAssignment);
					}
					else
					{
						if (propertyInfoCache.PropertyType.IsEnum)
						{
							bufferVariable = new CodeVariableDeclarationStatement(typeof(object), variableName);
							container.Statements.Add(bufferVariable);
							uncastLocalVariableRef = new CodeVariableReferenceExpression(variableName);
							var setToValue = new CodeAssignStatement(refToProperty,
								new CodeCastExpression(
									new CodeTypeReference(propertyInfoCache.PropertyType, CodeTypeReferenceOptions.GenericTypeParameter),
									uncastLocalVariableRef));
							container.Statements.Add(setToValue);
						}
					}

					var isNullable = false;

					var baseType = Nullable.GetUnderlyingType(propertyInfoCache.PropertyType);

					if (baseType != null)
					{
						isNullable = true;
					}

					if (propertyInfoCache.PropertyType == typeof(string))
					{
						baseType = typeof(string);
					}
					else if (propertyInfoCache.PropertyType.IsArray)
					{
						baseType = propertyInfoCache.PropertyType;
					}

					if (baseType != null && !settings.AssertDataNotDbNull)
					{
						if (bufferVariable == null)
						{
							bufferVariable = new CodeVariableDeclarationStatement(typeof(object), variableName);
							container.Statements.Add(bufferVariable);
							uncastLocalVariableRef = new CodeVariableReferenceExpression(variableName);
							var buffAssignment = new CodeAssignStatement(uncastLocalVariableRef, codeIndexerExpression);
							container.Statements.Add(buffAssignment);
						}

						var checkForDbNull = new CodeConditionStatement
						{
							Condition = new CodeBinaryOperatorExpression(uncastLocalVariableRef,
								CodeBinaryOperatorType.IdentityEquality,
								new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("System.DBNull"), "Value"))
						};

						CodeAssignStatement setToNull;
						if (!isNullable && baseType != typeof(string))
						{
							setToNull = new CodeAssignStatement(refToProperty,
							new CodeDefaultValueExpression(
							CreateShortCodeTypeReference(baseType, importNameSpace.Imports)));
						}
						else
						{
							setToNull = new CodeAssignStatement(refToProperty, new CodePrimitiveExpression(null));
						}

						var setToValue = new CodeAssignStatement(refToProperty,
							new CodeCastExpression(
								new CodeTypeReference(baseType, CodeTypeReferenceOptions.GenericTypeParameter),
								uncastLocalVariableRef));
						checkForDbNull.TrueStatements.Add(setToNull);
						checkForDbNull.FalseStatements.Add(setToValue);
						container.Statements.Add(checkForDbNull);
					}
					else
					{
						if (bufferVariable != null)
						{
							CodeExpression castExp = new CodeCastExpression(
								new CodeTypeReference(propertyInfoCache.PropertyType, CodeTypeReferenceOptions.GenericTypeParameter),
								new CodeVariableReferenceExpression(bufferVariable.Name));
							var setExpr = new CodeAssignStatement(refToProperty, castExp);
							container.Statements.Add(setExpr);
						}
						else
						{
							CodeExpression castExp = new CodeCastExpression(
								new CodeTypeReference(propertyInfoCache.PropertyType, CodeTypeReferenceOptions.GenericTypeParameter),
								codeIndexerExpression);
							var setExpr = new CodeAssignStatement(refToProperty, castExp);
							container.Statements.Add(setExpr);
						}
					}
				}
			}
		}

		/// <summary>
		///     Generates a C# DOM body for an IDataReader loader.
		/// </summary>
		/// <param name="sourceType">Type of the source.</param>
		/// <param name="settings">The settings.</param>
		/// <param name="importNameSpace">The import name space.</param>
		/// <param name="container">The container.</param>
		/// <param name="target">The target.</param>
		public static void GenerateBody(DbClassInfoCache sourceType,
			FactoryHelperSettings settings,
			CodeNamespace importNameSpace,
			CodeMemberMethod container,
			CodeExpression target)
		{
			GenerateBody(sourceType.Propertys, settings, importNameSpace, container, target);
		}

		/// <summary>
		///     Generates a type constructor by using the
		///     <see
		///         cref="GenerateBody(Dictionary{string,DbPropertyInfoCache},FactoryHelperSettings,CodeNamespace,CodeMemberMethod,CodeExpression)" />
		///     .
		/// </summary>
		/// <param name="propertyToDbColumn">The property to database column.</param>
		/// <param name="altNamespace">The alt namespace.</param>
		/// <returns></returns>
		public static CodeMemberMethod GenerateTypeConstructor(IEnumerable<DbPropertyInfoCache> propertyToDbColumn,
			string altNamespace)
		{
			var codeConstructor = GenerateTypeConstructor();
			GenerateBody(propertyToDbColumn.ToDictionary(s => s.DbName), new FactoryHelperSettings(),
				new CodeNamespace(altNamespace), codeConstructor, new CodeThisReferenceExpression());
			return codeConstructor;
		}

		private static string debug(CodeCompileUnit cp)
		{
			var writer = new StringWriter();
			writer.NewLine = Environment.NewLine;

			new CSharpCodeProvider().GenerateCodeFromCompileUnit(cp, writer, new CodeGeneratorOptions
			{
				BlankLinesBetweenMembers = false,
				VerbatimOrder = true,
				ElseOnClosing = true
			});

			writer.Flush();
			return writer.ToString();
		}

		private static Regex _pessimisticCreatedDllSyntax = new Regex(@"([a-zA-Z0-9]*_Poco\.dll)", RegexOptions.Compiled);

		/// <summary>
		/// Lists all dlls that are created by the Framework at any time when using the <code>CollisonDetectionMode.Pessimistic</code>
		/// Settings
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<string> ListPessimisticCreatedDlls()
		{
			foreach (var fileInDirectory in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
			{
				if (_pessimisticCreatedDllSyntax.IsMatch(fileInDirectory))
				{
					yield return fileInDirectory;
				}
			}
		}

		[SecurityCritical]
		internal static Func<IDataRecord, object> CreateFactory(DbClassInfoCache target, FactoryHelperSettings settings)
		{
			var configAttrCtorAtt = target.Attributes.FirstOrDefault(s => s.Attribute is AutoGenerateCtorAttribute);
			AutoGenerateCtorAttribute classCtorAttr = null;
			if (configAttrCtorAtt != null)
			{
				classCtorAttr = configAttrCtorAtt.Attribute as AutoGenerateCtorAttribute;
			}

			CodeNamespace importNameSpace;
			importNameSpace = new CodeNamespace(target.Type.Namespace);
			var cp = new CompilerParameters();
			string superName;
			var compiler = new CodeTypeDeclaration();
			compiler.IsClass = true;

			var generateFactory = classCtorAttr != null &&
								  classCtorAttr.CtorGeneratorMode == CtorGeneratorMode.FactoryMethod || target.Type.IsSealed;

			CodeMemberMethod codeConstructor;
			var codeName = target.Name.Split('.').Last();

			if (generateFactory)
			{
				codeConstructor = GenerateFactory(target, settings, importNameSpace);
				superName = codeName + "_Factory";
				compiler.Attributes = MemberAttributes.Static;
			}
			else
			{
				compiler.BaseTypes.Add(target.Type);
				codeConstructor = GenerateConstructor(target, settings, importNameSpace);
				compiler.IsPartial = true;
				superName = codeName + "_Super";
			}
			if (target.Constructors.Any(f => f.Arguments.Any()))
			{
				throw new TypeAccessException(
				string.Format("Target type '{0}' does not define an public parametherless constructor. POCO's!!!!", target.Name));
			}

			compiler.Attributes |= MemberAttributes.Public;
			compiler.Name = superName;
			compiler.Members.Add(codeConstructor);

			cp.GenerateInMemory = true;
			if (settings.FileCollisonDetection == CollisonDetectionMode.Pessimistic)
			{
				cp.OutputAssembly =
						Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
						+ @"\"
						+ Guid.NewGuid().ToString("N")
						+ "_Poco.dll";
			}
			else
			{
				cp.OutputAssembly =
						Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
						+ @"\" + target.Type.FullName
						+ "_Poco.dll";
			}

			settings.TempFileData.Add(cp.OutputAssembly);

			ConstructorInfo[] constructorInfos = null;
			TypeInfo targetType = null;

			if (File.Exists(cp.OutputAssembly) && settings.FileCollisonDetection == CollisonDetectionMode.Optimistic)
			{
				var bufferAssam = Assembly.Load(cp.OutputAssembly);
				targetType = target.Type.GetTypeInfo();
				var type = bufferAssam.DefinedTypes.FirstOrDefault(s => s == targetType);
				if (targetType != null)
				{
					constructorInfos = targetType.GetConstructors();
				}

				if (constructorInfos == null)
				{
					throw new Exception(
					string.Format(
					"A dll with a matching name for type: {0} was found and the FileCollisonDetection is Optimistic but no matching Constuctors where found",
					type.Name));
				}
			}

			if (constructorInfos == null)
			{
				var callingAssm = Assembly.GetEntryAssembly();
				if (callingAssm == null)
				{
					callingAssm = Assembly.GetExecutingAssembly();
				}

				if (settings.CreateDebugCode)
				{
					cp.TempFiles = new TempFileCollection(Path.GetDirectoryName(callingAssm.Location), true);
					cp.GenerateInMemory = false;
					cp.TempFiles.KeepFiles = true;
					cp.IncludeDebugInformation = true;
				}

				//cp.GenerateExecutable = true;
				cp.ReferencedAssemblies.Add(target.Type.Assembly.Location);
				cp.ReferencedAssemblies.Add("System.dll");
				cp.ReferencedAssemblies.Add("System.Core.dll");
				cp.ReferencedAssemblies.Add("System.Data.dll");
				cp.ReferencedAssemblies.Add("System.Xml.dll");
				cp.ReferencedAssemblies.Add("System.Xml.Linq.dll");
				cp.ReferencedAssemblies.Add(typeof(DbAccessLayer).Assembly.Location);
				var compileUnit = new CodeCompileUnit();

				foreach (var defaultNamespace in settings.DefaultNamespaces)
				{
					importNameSpace.Imports.Add(new CodeNamespaceImport(defaultNamespace));
				}

				foreach (
					var additionalNamespace in
					target.Attributes.Where(f => f.Attribute is AutoGenerateCtorNamespaceAttribute)
						.Select(f => f.Attribute as AutoGenerateCtorNamespaceAttribute))
				{
					importNameSpace.Imports.Add(new CodeNamespaceImport(additionalNamespace.UsedNamespace));
				}

				if (classCtorAttr != null && classCtorAttr.FullSateliteImport)
				{
					foreach (var referencedAssembly in target.Type.Assembly.GetReferencedAssemblies())
					{
						cp.ReferencedAssemblies.Add(referencedAssembly.Name);
					}
				}

				importNameSpace.Types.Add(compiler);

				compileUnit.Namespaces.Add(importNameSpace);
				var provider = new CSharpCodeProvider();
				var compileAssemblyFromDom = provider.CompileAssemblyFromDom(cp, compileUnit);

				if (compileAssemblyFromDom.Errors.Count > 0 && !settings.EnforceCreation)
				{
					var sb = new StringBuilder(string.Format("There are {0} errors due compilation.",
						compileAssemblyFromDom.Errors.Count));
					var errNr = 0;
					foreach (CompilerError error in compileAssemblyFromDom.Errors)
					{
						sb.AppendLine(errNr++ + error.ErrorNumber + ":" + error.Column + "," + error.Line + " -> " + error.ErrorText);
					}
					var ex =
						new InvalidDataException(sb.ToString());

					ex.Data.Add("Object", compileAssemblyFromDom);

					throw ex;
				}

				var compiledAssembly = compileAssemblyFromDom.CompiledAssembly;

				targetType = compiledAssembly.DefinedTypes.First();
				constructorInfos = targetType.GetConstructors();
				if (!constructorInfos.Any())
				{
					if (settings.EnforceCreation)
					{
						return null;
					}
					var ex =
						new InvalidDataException("There are was an unknown error due compilation. No CTOR was build");

					ex.Data.Add("Object", compileAssemblyFromDom);
					foreach (CompilerError error in compileAssemblyFromDom.Errors)
					{
						ex.Data.Add(error.ErrorNumber, error);
					}
					throw ex;
				}
			}

			var matchingCtor = constructorInfos.FirstOrDefault(s =>
			{
				var param = s.GetParameters();
				if (generateFactory)
				{
					if (param.Length < 1)
					{
						return false;
					}
					if (param.First().ParameterType != typeof(IDataRecord))
					{
						return false;
					}
				}
				return true;
			});

			var dm = new DynamicMethod("Create" + target.Name.Split('.')[0], target.Type, new[] { typeof(IDataRecord) },
				target.Type, true);
			var il = dm.GetILGenerator();
			if (generateFactory)
			{
				il.Emit(OpCodes.Nop);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Call, targetType.GetMethod("Factory"));
				il.Emit(OpCodes.Ret);
			}
			else
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Newobj, matchingCtor);
				il.Emit(OpCodes.Ret);
			}

			if (!settings.CreateDebugCode)
			{
				foreach (string tempFile in cp.TempFiles)
				{
					if (!tempFile.EndsWith("dll") && !tempFile.EndsWith("pdb"))
					{
						File.Delete(tempFile);
					}
				}
			}

			var func = (Func<IDataRecord, object>)dm.CreateDelegate(typeof(Func<IDataRecord, object>));
			return func;
		}

		internal static TypeBuilder GetTypeBuilder(string name)
		{
			var an = new AssemblyName(name);
			var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(name + "Module");
			var tb = moduleBuilder.DefineType(name
				, TypeAttributes.Public |
				  TypeAttributes.Class |
				  TypeAttributes.AutoClass |
				  TypeAttributes.AnsiClass |
				  TypeAttributes.BeforeFieldInit |
				  TypeAttributes.AutoLayout
				, null);
			return tb;
		}

		internal static Type CompileNewType(string name)
		{
			var tb = GetTypeBuilder(name);
			tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
			return tb.CreateType();
		}
	}
}