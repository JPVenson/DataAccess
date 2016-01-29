using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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

		private static CodeMemberMethod GenerateConstructor(Type target, FactoryHelperSettings settings, CodeNamespace importNameSpace)
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

		public static void GenerateBody(Type sourceType,
			FactoryHelperSettings settings,
			CodeNamespace importNameSpace,
			CodeMemberMethod container,
			CodeExpression target)
		{
			//Key = Column Name
			//Value = 
			//Value 1 = MethodName
			//Value 2 = Type

			foreach (var propertyInfoCache in sourceType.GetClassInfo().PropertyInfoCaches.Values)
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
						CodeBinaryOperatorType.ValueEquality,
						new CodeDefaultValueExpression(new CodeTypeReference(typeof(object))));
					container.Statements.Add(checkXmlForNull);

					var xmlRecordType = new CodeTypeReferenceExpression(typeof(XmlDataRecord));
					importNameSpace.Imports.Add(new CodeNamespaceImport("JPB.DataAccess.MetaInfoStore"));
					importNameSpace.Imports.Add(new CodeNamespaceImport("JPB.DataAccess"));

					if (propertyInfoCache.CheckForListInterface())
					{
						var typeArgument = propertyInfoCache.PropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();
						var generica = new CodeTypeReferenceExpression(typeArgument);

						var tryParse = new CodeMethodInvokeExpression(xmlRecordType,
							"TryParse",
							new CodeCastExpression(typeof(string), uncastLocalVariableRef),
							generica);

						var xmlDataRecords = new CodeMethodInvokeExpression(tryParse, "CreateListOfItems");
						var xmlRecordsToObjects = new CodeMethodInvokeExpression(xmlDataRecords, "Select",
							new CodeMethodReferenceExpression(generica, "SetPropertysViaReflection"));

						CodeObjectCreateExpression collectionCreate;
						if (typeArgument != null && (typeArgument.IsClass && typeArgument.GetInterface("INotifyPropertyChanged") != null))
						{
							collectionCreate = new CodeObjectCreateExpression(typeof(DbCollection<>),
								xmlRecordsToObjects);
						}
						else
						{
							collectionCreate = new CodeObjectCreateExpression(typeof(NonObservableDbCollection<>),
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

					if (propertyInfoCache.PropertyType == typeof(string))
					{
						baseType = typeof(string);
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
								new CodeTypeReference(baseType)));
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

		public static CodeMemberMethod GenerateTypeConstructor(
			IEnumerable<KeyValuePair<string, Tuple<string, Type>>> propertyToDbColumn)
		{
			//Key = Column Name
			//Value = 
			//Value 1 = MethodName
			//Value 2 = Type

			var codeConstructor = GenerateTypeConstructor();
			var config = new DbConfig();

			foreach (var columInfoModel in propertyToDbColumn)
			{
				var codeIndexerExpression = new CodeIndexerExpression(new CodeVariableReferenceExpression("record"),
					new CodePrimitiveExpression(columInfoModel.Key));

				var baseType = Nullable.GetUnderlyingType(columInfoModel.Value.Item2);
				var refToProperty = new CodeVariableReferenceExpression(columInfoModel.Value.Item1);

				if (columInfoModel.Value.Item2 == typeof(string))
				{
					baseType = typeof(string);
				}

				if (baseType != null)
				{
					var variableName = columInfoModel.Key.ToLower();
					var uncastLocalVariableRef = new CodeVariableReferenceExpression(variableName);
					var bufferVariable = new CodeVariableDeclarationStatement(typeof(object), variableName);
					var buffAssignment = new CodeAssignStatement(uncastLocalVariableRef, codeIndexerExpression);
					var checkForDbNull = new CodeConditionStatement();
					checkForDbNull.Condition = new CodeBinaryOperatorExpression(uncastLocalVariableRef,
						CodeBinaryOperatorType.IdentityEquality,
						new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("System.DBNull"), "Value"));

					var setToNull = new CodeAssignStatement(refToProperty,
						new CodeDefaultValueExpression(
							new CodeTypeReference(baseType)));
					var setToValue = new CodeAssignStatement(refToProperty,
						new CodeCastExpression(
							new CodeTypeReference(baseType, CodeTypeReferenceOptions.GenericTypeParameter),
							uncastLocalVariableRef));
					checkForDbNull.TrueStatements.Add(setToNull);
					checkForDbNull.FalseStatements.Add(setToValue);
					codeConstructor.Statements.Add(bufferVariable);
					codeConstructor.Statements.Add(buffAssignment);
					codeConstructor.Statements.Add(checkForDbNull);
				}
				else
				{
					CodeExpression castExp = new CodeCastExpression(
						new CodeTypeReference(columInfoModel.Value.Item2, CodeTypeReferenceOptions.GenericTypeParameter),
						codeIndexerExpression);
					var setExpr = new CodeAssignStatement(refToProperty, castExp);
					codeConstructor.Statements.Add(setExpr);
				}
			}
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
			CodeNamespace importNameSpace;
			importNameSpace = new CodeNamespace(target.Namespace);
			var cp = new CompilerParameters();
			string superName;
			var compiler = new CodeTypeDeclaration();
			compiler.IsClass = true;

			var generateCtor = !target.IsSealed;

			CodeMemberMethod codeConstructor;
			if (generateCtor)
			{
				compiler.BaseTypes.Add(target);
				codeConstructor = GenerateConstructor(target, settings, importNameSpace);
				compiler.IsPartial = true;
				superName = target.Name + "_Super";
			}
			else
			{
				codeConstructor = GenerateFactory(target, settings, importNameSpace);
				superName = target.Name + "_Factory";
				compiler.Attributes = MemberAttributes.Static;
			}
			if (target.GetClassInfo().ConstructorInfoCaches.Any(f => f.Arguments.Any()))
			{
				throw new TypeAccessException(string.Format("Target type '{0}' does not define an public parametherless constructor. POCO's!!!!", target.Name));
			}

			compiler.Attributes |= MemberAttributes.Public;
			compiler.Name = superName;
			compiler.Members.Add(codeConstructor);

			cp.GenerateInMemory = true;

			if (settings.CreateDebugCode)
			{
				cp.GenerateInMemory = false;
				cp.TempFiles = new TempFileCollection(Path.GetTempPath(), true);
				cp.TempFiles.KeepFiles = true;
				cp.IncludeDebugInformation = true;
			}

			cp.GenerateExecutable = false;
			cp.ReferencedAssemblies.Add(target.Assembly.ManifestModule.ToString());
			cp.ReferencedAssemblies.Add("System.dll");
			cp.ReferencedAssemblies.Add("System.Core.dll");
			cp.ReferencedAssemblies.Add("System.Data.dll");
			cp.ReferencedAssemblies.Add("System.Xml.dll");
			cp.ReferencedAssemblies.Add("System.Xml.Linq.dll");
			cp.ReferencedAssemblies.Add("JPB.DataAccess.dll");
			var compileUnit = new CodeCompileUnit();

			importNameSpace.Imports.Add(new CodeNamespaceImport("System"));
			importNameSpace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
			importNameSpace.Imports.Add(new CodeNamespaceImport("System.CodeDom.Compiler"));
			importNameSpace.Imports.Add(new CodeNamespaceImport("System.Linq"));
			importNameSpace.Imports.Add(new CodeNamespaceImport("System.Data"));
			importNameSpace.Imports.Add(new CodeNamespaceImport("JPB.DataAccess.ModelsAnotations"));
			importNameSpace.Imports.Add(new CodeNamespaceImport("JPB.DataAccess.AdoWrapper"));
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
				if (generateCtor)
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
			if (generateCtor)
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
			else
			{
				il.Emit(OpCodes.Nop);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Call, targetType.GetMethod("Factory"));
				il.Emit(OpCodes.Ret);
			}
	
			var func = (Func<IDataRecord, object>)dm.CreateDelegate(typeof(Func<IDataRecord, object>));
			return func;
		}

		public static object a(IDataRecord z)
		{
			return e.function(z);
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