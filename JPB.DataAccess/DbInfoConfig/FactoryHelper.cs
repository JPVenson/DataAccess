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
using JPB.DataAccess.ModelsAnotations;
using Microsoft.CSharp;

namespace JPB.DataAccess.DbInfoConfig
{
	/// <summary>
	/// </summary>
	public class FactoryHelperSettings
	{
		static FactoryHelperSettings()
		{
			_defaultNamespaces = new[]
			{
				"System",
				"System.Collections.Generic",
				"System.CodeDom.Compiler",
				"System.Linq",
				"System.Data",
				"JPB.DataAccess.ModelsAnotations",
				"JPB.DataAccess.AdoWrapper",
			};
		}

		/// <summary>
		///     Check and throw exception if not all propertys can be accessed by the Super class
		/// </summary>
		public bool EnforcePublicPropertys { get; set; }

		/// <summary>
		///     If any error is thrown so throw exception
		/// </summary>
		public bool EnforceCreation { get; set; }

		/// <summary>
		///     [Not Implimented]
		///     Shame on me.
		///     To set all propertys from the outside ill create a super class that inherts from the POCO .
		///     to get rid of this super class you can set this property to true then the superclass will be cased into its
		///     baseclass.
		///     If set to true the factory will cast the object to its base class and hide the super creation
		/// </summary>
		public bool HideSuperCreation { get; set; }

		/// <summary>
		///     Include PDB debug infos
		/// </summary>
		public bool CreateDebugCode { get; set; }

		private static readonly string[] _defaultNamespaces;

		/// <summary>
		/// A Collection that includes all Namespaces that are used by default to create new Factorys
		/// </summary>
		public IEnumerable<string> DefaultNamespaces
		{
			get { return _defaultNamespaces; }
		}
	}

	/// <summary>
	///     Only for internal use
	/// </summary>
	internal class FactoryHelper
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

		private static CodeMemberMethod GenerateFactory(Type target, FactoryHelperSettings settings, CodeNamespace importNameSpace)
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

		private static void Shortify(CodeTypeReference typeReference, Type type, CodeNamespaceImportCollection imports)
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

				var attributes = propertyInfoCache.AttributeInfoCaches;
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
							codeTypeOfListArg);

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
							typeofProperty);

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

					var baseType = Nullable.GetUnderlyingType(propertyInfoCache.PropertyType);

					if (propertyInfoCache.PropertyType == typeof (string))
					{
						baseType = typeof (string);
					}
					else if(propertyInfoCache.PropertyType.IsArray)
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

						var setToNull = new CodeAssignStatement(refToProperty,
							new CodeDefaultValueExpression(
								CreateShortCodeTypeReference(baseType, importNameSpace.Imports)));
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
						CodeExpression castExp = new CodeCastExpression(
							new CodeTypeReference(propertyInfoCache.PropertyType, CodeTypeReferenceOptions.GenericTypeParameter),
							codeIndexerExpression);
						var setExpr = new CodeAssignStatement(refToProperty, castExp);
						container.Statements.Add(setExpr);
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
			GenerateBody(sourceType.GetClassInfo().PropertyInfoCaches, settings, importNameSpace, container, target);
		}

		public static CodeMemberMethod GenerateTypeConstructor(IEnumerable<KeyValuePair<string, Tuple<string, Type>>> propertyToDbColumn, string altNamespace)
		{
			//Key = Column Name
			//Value = 
			//Value 1 = PropName
			//Value 2 = Type

			var codeConstructor = GenerateTypeConstructor();
			var config = new DbConfig();

			var fakeProps = propertyToDbColumn.Select(f =>
			{
				var dbPropertyInfoCache = new DbPropertyInfoCache();

				if (f.Key != f.Value.Item1)
				{
					dbPropertyInfoCache.AttributeInfoCaches.Add(new DbAttributeInfoCache(new ForModelAttribute(f.Key)));
				}

				dbPropertyInfoCache.PropertyName = f.Value.Item1;
				dbPropertyInfoCache.PropertyType = f.Value.Item2;

				return dbPropertyInfoCache;
			}).ToDictionary(f => f.PropertyName);

			GenerateBody(fakeProps, new FactoryHelperSettings(), new CodeNamespace(altNamespace), codeConstructor, new CodeThisReferenceExpression());


			//foreach (var columInfoModel in propertyToDbColumn)
			//{
			//	var codeIndexerExpression = new CodeIndexerExpression(new CodeVariableReferenceExpression("record"),
			//		new CodePrimitiveExpression(columInfoModel.Key));

			//	var baseType = Nullable.GetUnderlyingType(columInfoModel.Value.Item2);
			//	var refToProperty = new CodeVariableReferenceExpression(columInfoModel.Value.Item1);

			//	if (columInfoModel.Value.Item2 == typeof(string))
			//	{
			//		baseType = typeof(string);
			//	}

			//	if (baseType != null)
			//	{
			//		var variableName = columInfoModel.Key.ToLower();
			//		var uncastLocalVariableRef = new CodeVariableReferenceExpression(variableName);
			//		var bufferVariable = new CodeVariableDeclarationStatement(typeof(object), variableName);
			//		var buffAssignment = new CodeAssignStatement(uncastLocalVariableRef, codeIndexerExpression);
			//		var checkForDbNull = new CodeConditionStatement();
			//		checkForDbNull.Condition = new CodeBinaryOperatorExpression(uncastLocalVariableRef,
			//			CodeBinaryOperatorType.IdentityEquality,
			//			new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("System.DBNull"), "Value"));

			//		var setToNull = new CodeAssignStatement(refToProperty,
			//			new CodeDefaultValueExpression(
			//				new CodeTypeReference(baseType)));
			//		var setToValue = new CodeAssignStatement(refToProperty,
			//			new CodeCastExpression(
			//				new CodeTypeReference(baseType, CodeTypeReferenceOptions.GenericTypeParameter),
			//				uncastLocalVariableRef));
			//		checkForDbNull.TrueStatements.Add(setToNull);
			//		checkForDbNull.FalseStatements.Add(setToValue);
			//		codeConstructor.Statements.Add(bufferVariable);
			//		codeConstructor.Statements.Add(buffAssignment);
			//		codeConstructor.Statements.Add(checkForDbNull);
			//	}
			//	else
			//	{
			//		CodeExpression castExp = new CodeCastExpression(
			//			new CodeTypeReference(columInfoModel.Value.Item2, CodeTypeReferenceOptions.GenericTypeParameter),
			//			codeIndexerExpression);
			//		var setExpr = new CodeAssignStatement(refToProperty, castExp);
			//		codeConstructor.Statements.Add(setExpr);
			//	}
			//}
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
				classInfo.AttributeInfoCaches.First(s => s.Attribute is AutoGenerateCtorAttribute).Attribute as
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
			if (classInfo.ConstructorInfoCaches.Any(f => f.Arguments.Any()))
			{
				throw new TypeAccessException(string.Format("Target type '{0}' does not define an public parametherless constructor. POCO's!!!!", target.Name));
			}

			compiler.Attributes |= MemberAttributes.Public;
			compiler.Name = superName;
			compiler.Members.Add(codeConstructor);

			cp.GenerateInMemory = true;

			var callingAssm = Assembly.GetEntryAssembly();
			if (callingAssm == null)
			{
				//are we testing are we?
				callingAssm = Assembly.GetExecutingAssembly();
			}

			cp.TempFiles = new TempFileCollection(Path.GetDirectoryName(callingAssm.Location), true);

			if (settings.CreateDebugCode)
			{
				cp.GenerateInMemory = false;
				cp.TempFiles.KeepFiles = true;
				cp.IncludeDebugInformation = true;
			}

			cp.GenerateExecutable = false;
			cp.ReferencedAssemblies.Add(target.Assembly.ManifestModule.Name);
			cp.ReferencedAssemblies.Add("System.dll");
			cp.ReferencedAssemblies.Add("System.Core.dll");
			cp.ReferencedAssemblies.Add("System.Data.dll");
			cp.ReferencedAssemblies.Add("System.Xml.dll");
			cp.ReferencedAssemblies.Add("System.Xml.Linq.dll");
			cp.ReferencedAssemblies.Add("JPB.DataAccess.dll");
			var compileUnit = new CodeCompileUnit();

			foreach (var defaultNamespace in settings.DefaultNamespaces)
			{
				importNameSpace.Imports.Add(new CodeNamespaceImport(defaultNamespace));
			}

			foreach (var additionalNamespace in classInfo.AttributeInfoCaches.Where(f => f.Attribute is AutoGenerateCtorNamespaceAttribute).Select(f => f.Attribute as AutoGenerateCtorNamespaceAttribute))
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


			if (compileAssemblyFromDom.Errors.Count > 0 && settings.EnforceCreation)
			{
				var ex =
					new InvalidDataException(string.Format("There where {0} errors due compilation. See Data",
						compileAssemblyFromDom.Errors.Count));

				ex.Data.Add("Object", compileAssemblyFromDom);
				foreach (CompilerError error in compileAssemblyFromDom.Errors)
				{
					ex.Data.Add(error.ErrorNumber, error);
				}
				throw ex;
			}

			var compiledAssembly = compileAssemblyFromDom.CompiledAssembly;

			var targetType = compiledAssembly.DefinedTypes.First();
			var constructorInfos = targetType.GetConstructors();
			if (!constructorInfos.Any())
			{
				if (settings.EnforceCreation)
					return null;
				var ex =
					new InvalidDataException(string.Format("There where was an unknown error due compilation. No CTOR was build"));

				ex.Data.Add("Object", compileAssemblyFromDom);
				foreach (CompilerError error in compileAssemblyFromDom.Errors)
				{
					ex.Data.Add(error.ErrorNumber, error);
				}
				throw ex;
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

			if (matchingCtor == null)
			{
				if (settings.EnforceCreation)
					return null;
				var ex =
					new InvalidDataException(string.Format("There where was an unknown error due compilation. No CTOR was build"));

				ex.Data.Add("Object", compileAssemblyFromDom);
				foreach (CompilerError error in compileAssemblyFromDom.Errors)
				{
					ex.Data.Add(error.ErrorNumber, error);
				}
				throw ex;
			}

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
				if (settings.HideSuperCreation)
				{
					var variable = il.DeclareLocal(target);
					il.Emit(OpCodes.Stloc, variable);
					il.Emit(OpCodes.Ldloc, variable);
					il.Emit(OpCodes.Castclass, target);
					il.Emit(OpCodes.Ret, variable);
				}
				else
				{
					il.Emit(OpCodes.Ret);
				}
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

	public static class e
	{
		public static object function(IDataRecord d)
		{
			return null;
		}
	}
}