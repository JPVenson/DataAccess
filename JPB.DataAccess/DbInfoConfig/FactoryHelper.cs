using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
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
		public static CodeConstructor GenerateTypeConstructor(bool factory = true)
		{
			var codeConstructor = new CodeConstructor();
			if (factory)
			{
				codeConstructor.CustomAttributes.Add(new CodeAttributeDeclaration(typeof (ObjectFactoryMethodAttribute).Name));
				codeConstructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof (IDataRecord).Name, "record"));
			}

			codeConstructor.Attributes = MemberAttributes.Public;
			return codeConstructor;
		}

		public static CodeConstructor GenerateTypeConstructorEx(Type target, FactoryHelperSettings settings,
			CodeNamespace importNameSpace)
		{
			//Key = Column Name
			//Value = 
			//Value 1 = MethodName
			//Value 2 = Type
			var codeConstructor = GenerateTypeConstructor();

			foreach (DbPropertyInfoCache propertyInfoCache in target.GetClassInfo().PropertyInfoCaches.Values)
			{
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
				var refToProperty = new CodeVariableReferenceExpression(propertyInfoCache.PropertyName);

				var attributes = propertyInfoCache.AttributeInfoCaches;
				var valueConverterAttributeModel =
					attributes.FirstOrDefault(s => s.Attribute is ValueConverterAttribute);
				var isXmlProperty = attributes.FirstOrDefault(s => s.Attribute is FromXmlAttribute);
				CodeVariableReferenceExpression uncastLocalVariableRef = null;

				if (isXmlProperty != null)
				{
					bufferVariable = new CodeVariableDeclarationStatement(typeof (object), variableName);
					codeConstructor.Statements.Add(bufferVariable);
					uncastLocalVariableRef = new CodeVariableReferenceExpression(variableName);
					var buffAssignment = new CodeAssignStatement(uncastLocalVariableRef, codeIndexerExpression);
					codeConstructor.Statements.Add(buffAssignment);

					var checkXmlForNull = new CodeConditionStatement();
					checkXmlForNull.Condition = new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression(variableName),
						CodeBinaryOperatorType.ValueEquality,
						new CodeDefaultValueExpression(new CodeTypeReference(typeof (object))));
					codeConstructor.Statements.Add(checkXmlForNull);

					var xmlRecordType = new CodeTypeReferenceExpression(typeof (XmlDataRecord));
					importNameSpace.Imports.Add(new CodeNamespaceImport("JPB.DataAccess.MetaInfoStore"));
					importNameSpace.Imports.Add(new CodeNamespaceImport("JPB.DataAccess"));

					if (propertyInfoCache.CheckForListInterface())
					{
						var typeArgument = propertyInfoCache.PropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();
						var generica = new CodeTypeReferenceExpression(typeArgument);

						var tryParse = new CodeMethodInvokeExpression(xmlRecordType,
							"TryParse",
							new CodeCastExpression(typeof (string), uncastLocalVariableRef),
							generica);

						var xmlDataRecords = new CodeMethodInvokeExpression(tryParse, "CreateListOfItems");
						var xmlRecordsToObjects = new CodeMethodInvokeExpression(xmlDataRecords, "Select",
							new CodeMethodReferenceExpression(generica, "SetPropertysViaReflection"));

						CodeObjectCreateExpression collectionCreate;
						if (typeArgument.IsClass && typeArgument.GetInterface("INotifyPropertyChanged") != null)
						{
							collectionCreate = new CodeObjectCreateExpression(typeof (DbCollection<>),
								xmlRecordsToObjects);
						}
						else
						{
							collectionCreate = new CodeObjectCreateExpression(typeof (NonObservableDbCollection<>),
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
							new CodeCastExpression(typeof (string), uncastLocalVariableRef),
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
						bufferVariable = new CodeVariableDeclarationStatement(typeof (object), variableName);
						codeConstructor.Statements.Add(bufferVariable);
						uncastLocalVariableRef = new CodeVariableReferenceExpression(variableName);
						var buffAssignment = new CodeAssignStatement(uncastLocalVariableRef, codeIndexerExpression);
						codeConstructor.Statements.Add(buffAssignment);

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
						codeConstructor.Statements.Add(codeAssignment);
					}

					var baseType = Nullable.GetUnderlyingType(propertyInfoCache.PropertyType);

					if (propertyInfoCache.PropertyType == typeof (string))
					{
						baseType = typeof (string);
					}

					if (baseType != null)
					{
						if (bufferVariable == null)
						{
							bufferVariable = new CodeVariableDeclarationStatement(typeof (object), variableName);
							codeConstructor.Statements.Add(bufferVariable);
							uncastLocalVariableRef = new CodeVariableReferenceExpression(variableName);
							var buffAssignment = new CodeAssignStatement(uncastLocalVariableRef, codeIndexerExpression);
							codeConstructor.Statements.Add(buffAssignment);
						}

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
						codeConstructor.Statements.Add(checkForDbNull);
					}
					else
					{
						CodeExpression castExp = new CodeCastExpression(
							new CodeTypeReference(propertyInfoCache.PropertyType, CodeTypeReferenceOptions.GenericTypeParameter),
							codeIndexerExpression);
						var setExpr = new CodeAssignStatement(refToProperty, castExp);
						codeConstructor.Statements.Add(setExpr);
					}
				}
			}
			return codeConstructor;
		}

		public static CodeConstructor GenerateTypeConstructor(
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

				if (columInfoModel.Value.Item2 == typeof (string))
				{
					baseType = typeof (string);
				}

				if (baseType != null)
				{
					var variableName = columInfoModel.Key.ToLower();
					var uncastLocalVariableRef = new CodeVariableReferenceExpression(variableName);
					var bufferVariable = new CodeVariableDeclarationStatement(typeof (object), variableName);
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

		internal static Func<IDataRecord, object> CreateFactory(Type target, FactoryHelperSettings settings)
		{
			CodeNamespace importNameSpace;
			importNameSpace = new CodeNamespace(target.Namespace);
			var cp = new CompilerParameters();
			var codeConstructor = GenerateTypeConstructorEx(target, settings, importNameSpace);
			var superName = target.Name + "_super";
			var compiler = new CodeTypeDeclaration(superName);
			compiler.IsClass = true;
			compiler.IsPartial = true;
			compiler.BaseTypes.Add(target);

			compiler.Members.Add(codeConstructor);
			cp.GenerateInMemory = true;

			if (settings.CreateDebugCode)
			{
				cp.GenerateInMemory = false;
				cp.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
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

			var constructorInfos = compiledAssembly.DefinedTypes.First().GetConstructors();
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
				if (param.Length != 1)
					return false;
				if (param.FirstOrDefault().ParameterType != typeof (IDataRecord))
					return false;
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
			var dm = new DynamicMethod("Create" + target.Name, target, new[] {typeof (IDataRecord)}, target, true);
			var il = dm.GetILGenerator();
			Func<IDataRecord, object> func;
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Newobj, matchingCtor);

			if (settings.HideSuperCreation)
			{
				var variable = il.DeclareLocal(target);
				il.Emit(OpCodes.Stloc, variable);
				il.Emit(OpCodes.Ldloc, variable);
				il.Emit(OpCodes.Castclass, target);
				il.Emit(OpCodes.Ret, variable);
				//il.Emit(OpCodes.Ret);
				//func = (Func<IDataRecord, object>)dm.CreateDelegate(typeof(Func<IDataRecord, object>));
				//return func;
				//var castMethod = typeof(FactoryHelper).GetMethod("Cast").MakeGenericMethod(target);
				//return s =>
				//{
				//	var super = matchingCtor.Invoke(new object[] { s });
				//	return castMethod.Invoke(null, new object[] { super });
				//};
			}
			else
			{
				il.Emit(OpCodes.Ret);
			}

			func = (Func<IDataRecord, object>) dm.CreateDelegate(typeof (Func<IDataRecord, object>));
			return func;

			//var paramCtor = Expression.Parameter(typeof(IDataRecord), "val");

			//var factoryDelegate = Expression.Lambda<Func<IDataRecord, object>>(
			//	Expression.New(matchingCtor, paramCtor), paramCtor
			//).Compile();
			////var factoryDelegate = expression.Compile();
			//return factoryDelegate;
		}


		public static T Cast<T>(object o)
		{
			return (T) o;
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
		//	target, new Type[] { typeof(IDataRecord) });
		//var dataRecord = fb.GetParameters()[0];
		//var ilg = fb.GetILGenerator();
		//ilg.Emit(OpCodes.Ldarg_0);
		//ilg.Emit(OpCodes.Stl);
	}
}