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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using Microsoft.CSharp;

namespace JPB.DataAccess.DbInfoConfig
{
	/// <summary>
	///     Only for internal use
	/// </summary>
	public class FactoryHelper
	{
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

		public static CodeMemberMethod GenerateConstructor(Type target, FactoryHelperSettings settings, CodeNamespace importNameSpace)
		{
			var codeConstructor = GenerateTypeConstructor();
			GenerateBody(target, settings, importNameSpace, codeConstructor, new CodeBaseReferenceExpression());
			return codeConstructor;
		}

		public static CodeMemberMethod GenerateFactory(Type target, FactoryHelperSettings settings, CodeNamespace importNameSpace)
		{
			var codeFactory = GenerateTypeConstructor(true);
			var super = new CodeVariableReferenceExpression("super");

			var pocoVariable = new CodeVariableDeclarationStatement(target, "super");
			codeFactory.Statements.Add(pocoVariable);

			var codeAssignment = new CodeAssignStatement(super, new CodeObjectCreateExpression(target));
			codeFactory.Statements.Add(codeAssignment);

			GenerateBody(target, settings, importNameSpace, codeFactory, super);
			codeFactory.ReturnType = new CodeTypeReference(target);
			codeFactory.Statements.Add(new CodeMethodReturnStatement(super));

			return codeFactory;
		}

		public static CodeTypeReference CreateShortCodeTypeReference(Type type, CodeNamespaceImportCollection imports)
		{
			var result = new CodeTypeReference(type);

			Shortify(result, type, imports);

			return result;
		}

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
						throw new AccessViolationException(string.Format(
							"The Getter of {0} is private. Full creation cannot be enforced", propertyInfoCache.PropertyName));
					if (propertyInfoCache.Setter.MethodInfo.IsPrivate)
						throw new AccessViolationException(string.Format(
							"The Setter of {0} is private. Full creation cannot be enforced", propertyInfoCache.PropertyName));
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

						var tryParse = new CodeMethodInvokeExpression(xmlRecordType,
							"TryParse",
							new CodeCastExpression(typeof(string), uncastLocalVariableRef),
							codeTypeOfListArg,
							new CodePrimitiveExpression(false));

						var xmlDataRecords = new CodeMethodInvokeExpression(tryParse, "CreateListOfItems");
						var getClassInfo = new CodeMethodInvokeExpression(codeTypeOfListArg, "GetClassInfo");

						var xmlRecordsToObjects = new CodeMethodInvokeExpression(xmlDataRecords, "Select",
							new CodeMethodReferenceExpression(getClassInfo, "SetPropertysViaReflection"));

						CodeObjectCreateExpression collectionCreate;
						if (typeArgument != null && (typeArgument.IsClass && typeArgument.GetInterface("INotifyPropertyChanged") != null))
						{
							collectionCreate = new CodeObjectCreateExpression(typeof(DbCollection<>).MakeGenericType(typeArgument),
								xmlRecordsToObjects);
						}
						else
						{
							collectionCreate = new CodeObjectCreateExpression(typeof(NonObservableDbCollection<>).MakeGenericType(typeArgument),
								xmlRecordsToObjects);
						}

						var setExpr = new CodeAssignStatement(refToProperty, collectionCreate);
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

						var setProps = new CodeMethodInvokeExpression(getClassInfo, "SetPropertysViaReflection", tryParse);
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
						isNullable = true;

					if (propertyInfoCache.PropertyType == typeof(string))
					{
						baseType = typeof(string);
					}
					else if (propertyInfoCache.PropertyType.IsArray)
					{
						baseType = propertyInfoCache.PropertyType;
					}

					if (baseType != null)
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

		public static void GenerateBody(Type sourceType,
			FactoryHelperSettings settings,
			CodeNamespace importNameSpace,
			CodeMemberMethod container,
			CodeExpression target)
		{
			GenerateBody(sourceType.GetClassInfo().Propertys, settings, importNameSpace, container, target);
		}

		public static CodeMemberMethod GenerateTypeConstructor(IEnumerable<DbPropertyInfoCache> propertyToDbColumn, string altNamespace)
		{
			var codeConstructor = GenerateTypeConstructor();
			GenerateBody(propertyToDbColumn.ToDictionary(s => s.DbName), new FactoryHelperSettings(), new CodeNamespace(altNamespace), codeConstructor, new CodeThisReferenceExpression());
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

		[SecurityCritical]
		internal static Func<IDataRecord, object> CreateFactory(Type target, FactoryHelperSettings settings)
		{
			var classInfo = target.GetClassInfo();
			var classCtorAttr =
				classInfo.Attributes.First(s => s.Attribute is AutoGenerateCtorAttribute).Attribute as
					AutoGenerateCtorAttribute;

			CodeNamespace importNameSpace;
			importNameSpace = new CodeNamespace(target.Namespace);
			var cp = new CompilerParameters();
			string superName;
			var compiler = new CodeTypeDeclaration();
			compiler.IsClass = true;

			var generateFactory = target.IsSealed || classCtorAttr.CtorGeneratorMode == CtorGeneratorMode.FactoryMethod;

			CodeMemberMethod codeConstructor;
			if (generateFactory)
			{
				codeConstructor = GenerateFactory(target, settings, importNameSpace);
				superName = target.Name + "_Factory";
				compiler.Attributes = MemberAttributes.Static;
			}
			else
			{
				compiler.BaseTypes.Add(target);
				codeConstructor = GenerateConstructor(target, settings, importNameSpace);
				compiler.IsPartial = true;
				superName = target.Name + "_Super";
			}
			if (classInfo.Constructors.Any(f => f.Arguments.Any()))
			{
				throw new TypeAccessException(string.Format("Target type '{0}' does not define an public parametherless constructor. POCO's!!!!", target.Name));
			}

			compiler.Attributes |= MemberAttributes.Public;
			compiler.Name = superName;
			compiler.Members.Add(codeConstructor);

			cp.GenerateInMemory = true;
			cp.OutputAssembly =
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
				+ @"\" + target.FullName
				+ "_Poco.dll";

			Assembly compiledAssembly;
			ConstructorInfo[] constructorInfos = null;
			TypeInfo targetType = null;

			if (File.Exists(cp.OutputAssembly) && settings.ReuseFactorys)
			{
				var bufferAssam = Assembly.Load(cp.OutputAssembly);
				targetType = target.GetTypeInfo();
				var type = bufferAssam.DefinedTypes.FirstOrDefault(s => s == targetType);
				if (targetType != null)
					constructorInfos = targetType.GetConstructors();
			}

			if (constructorInfos == null)
			{
				var callingAssm = Assembly.GetEntryAssembly();
				if (callingAssm == null)
				{
					//testing we are?
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
				cp.ReferencedAssemblies.Add(target.Assembly.Location);
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

				foreach (var additionalNamespace in classInfo.Attributes.Where(f => f.Attribute is AutoGenerateCtorNamespaceAttribute).Select(f => f.Attribute as AutoGenerateCtorNamespaceAttribute))
				{
					importNameSpace.Imports.Add(new CodeNamespaceImport(additionalNamespace.UsedNamespace));
				}

				if (classCtorAttr.FullSateliteImport)
				{
					foreach (var referencedAssembly in target.Assembly.GetReferencedAssemblies())
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
					var ex =
						new InvalidDataException(string.Format("There are {0} errors due compilation. See Data",
							compileAssemblyFromDom.Errors.Count));

					ex.Data.Add("Object", compileAssemblyFromDom);
					int errNr = 0;
					foreach (CompilerError error in compileAssemblyFromDom.Errors)
					{
						ex.Data.Add(errNr++ +
							error.ErrorNumber + ":" + error.Column + "," + error.Line, error.ErrorText);
					}
					throw ex;
				}

				compiledAssembly = compileAssemblyFromDom.CompiledAssembly;

				targetType = compiledAssembly.DefinedTypes.First();
				constructorInfos = targetType.GetConstructors();
				if (!constructorInfos.Any())
				{
					if (settings.EnforceCreation)
						return null;
					var ex =
						new InvalidDataException(string.Format("There are was an unknown error due compilation. No CTOR was build"));

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
						return false;
					if (param.First().ParameterType != typeof(IDataRecord))
						return false;
				}
				return true;
			});

			var dm = new DynamicMethod("Create" + target.Name, target, new[] { typeof(IDataRecord) }, target, true);
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

				//if (settings.HideSuperCreation)
				//{
				//	il.Emit(OpCodes.Ldarg_0);
				//	il.Emit(OpCodes.Newobj, matchingCtor);
				//	var buffVariable = il.DeclareLocal(typeof(object));
				//	var classVariable = il.DeclareLocal(target);

				//	il.Emit(OpCodes.Stloc, buffVariable);
				//	il.Emit(OpCodes.Ldloc, buffVariable);
				//	il.Emit(OpCodes.Castclass, target);

				//	il.Emit(OpCodes.Stloc, classVariable);
				//	il.Emit(OpCodes.Ldloc, classVariable);
				//	il.Emit(OpCodes.Ret);
				//}
				//else
				//{

				//}
			}

			if (!settings.CreateDebugCode)
				foreach (string tempFile in cp.TempFiles)
				{
					if (!tempFile.EndsWith("dll") && !tempFile.EndsWith("pdb"))
						File.Delete(tempFile);
				}

			var func = (Func<IDataRecord, object>)dm.CreateDelegate(typeof(Func<IDataRecord, object>));
			return func;
		}

		public static T Cast<T>(object o)
		{
			return (T)o;
		}

		//var an = new AssemblyName();
		//an.Name = "HelloReflectionEmit";
		//var ad = AppDomain.CurrentDomain;

		//var ab = ad.DefineDynamicAssembly(an,
		//	AssemblyBuilderAccess.Save);
		//var mb = ab.DefineDynamicModule(an.Name, "Hello.dll");
		//var tb = mb.DefineType("Foo.Bar", TypeAttributes.Public | TypeAttributes.Class);
		//var fb = tb.DefineMethod("CreateFactory",
		//	MethodAttributes.Public |
		//	MethodAttributes.Static,
		//	sourceType, new Type[] { typeof(IDataRecord) });
		//var dataRecord = fb.GetParameters()[0];
		//var ilg = fb.GetILGenerator();
		//ilg.Emit(OpCodes.Ldarg_0);
		//ilg.Emit(OpCodes.Stl);
	}
}