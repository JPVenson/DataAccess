#region

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig.ClassBuilder;
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
		internal static Func<IDataRecord, object> CreateFactory(
			DbClassInfoCache target, 
			FactoryHelperSettings settings)
		{
			var generator = ClassInfoGenerator.SuperClass(target);
			
			generator.Namespace = target.Type.Namespace;
			if (target.Constructors.Any(f => f.Arguments.Any()))
			{
				throw new TypeAccessException(
				string.Format("Target type '{0}' does not define an public not " +
				              "parameterizable constructor. POCO's!!!!", target.Name));
			}
			var cp = new CompilerParameters();
			
			cp.GenerateInMemory = true;
			//if (settings.FileCollisonDetection == CollisonDetectionMode.Pessimistic)
			//{
			//	cp.OutputAssembly =
			//			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
			//			+ @"\"
			//			+ Guid.NewGuid().ToString("N")
			//			+ "_Poco.dll";
			//}
			//else
			//{
			//	cp.OutputAssembly =
			//			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
			//			+ @"\" + target.Type.FullName
			//			+ "_Poco.dll";
			//}

			//settings.TempFileData.Add(cp.OutputAssembly);

			ConstructorInfo[] constructorInfos = null;
			TypeInfo targetType = null;

			//if (File.Exists(cp.OutputAssembly) && settings.FileCollisonDetection == CollisonDetectionMode.Optimistic)
			//{
			//	var bufferAssam = Assembly.Load(cp.OutputAssembly);
			//	targetType = target.Type.GetTypeInfo();
			//	var type = bufferAssam.DefinedTypes.FirstOrDefault(s => s == targetType);
			//	if (targetType != null)
			//	{
			//		constructorInfos = targetType.GetConstructors();
			//	}

			//	if (constructorInfos == null)
			//	{
			//		throw new Exception(
			//		string.Format(
			//		"A dll with a matching name for type: {0} was found and the FileCollisonDetection is Optimistic but no matching Constuctors where found",
			//		type.Name));
			//	}
			//}

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

				foreach (var defaultNamespace in settings.DefaultNamespaces)
				{
					generator.NamespaceImports.Add(defaultNamespace);
				}

				foreach (
					var additionalNamespace in
					target.Attributes.Where(f => f.Attribute is AutoGenerateCtorNamespaceAttribute)
						.Select(f => f.Attribute as AutoGenerateCtorNamespaceAttribute))
				{
					generator.NamespaceImports.Add(additionalNamespace.UsedNamespace);
				}

				var provider = new CSharpCodeProvider();
				var compileAssemblyFromDom = 
					provider.CompileAssemblyFromSource(cp, generator.RenderPocoClass());

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
				if (param.Length < 1)
				{
					return false;
				}
				if (param.First().ParameterType != typeof(EagarDataRecord))
				{
					return false;
				}
				return true;
			});

			var dm = new DynamicMethod("Create" + target.Name.Split('.')[0], target.Type, new[] { typeof(IDataRecord) },
				target.Type, true);
			var il = dm.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Newobj, matchingCtor ?? throw new InvalidOperationException());
			il.Emit(OpCodes.Ret);

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