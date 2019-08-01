#region

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using JPB.DataAccess.Framework.AdoWrapper;
using JPB.DataAccess.Framework.DbCollection;
using JPB.DataAccess.Framework.DbInfoConfig.ClassBuilder;
using JPB.DataAccess.Framework.DbInfoConfig.DbInfo;
using JPB.DataAccess.Framework.Manager;
using JPB.DataAccess.Framework.ModelsAnotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

#endregion

namespace JPB.DataAccess.Framework.DbInfoConfig
{
	/// <summary>
	///     Only for internal use
	/// </summary>
	public class FactoryHelper
	{
		private static IReadOnlyCollection<MetadataReference> _defaultReferences = new MetadataReference[]
		{
			MetadataReference.CreateFromFile(typeof(XmlReadMode).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(FactoryHelper).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(DbCollection<>).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Expression).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(GeneratedCodeAttribute).Assembly.Location),
		};

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



			//var cp = new CompilerParameters();
			//cp.GenerateInMemory = true;
			//var callingAssm = Assembly.GetEntryAssembly();
			//if (callingAssm == null)
			//{
			//	callingAssm = Assembly.GetExecutingAssembly();
			//}

			//if (settings.CreateDebugCode)
			//{
			//	cp.TempFiles = new TempFileCollection(Path.GetDirectoryName(callingAssm.Location), true);
			//	cp.GenerateInMemory = false;
			//	cp.TempFiles.KeepFiles = true;
			//	cp.IncludeDebugInformation = true;
			//}
			var references = new List<string>();
			references.Add(target.Type.Assembly.Location);
#if NETFULL
			references.Add("System.dll");
			references.Add("System.Core.dll");
			references.Add("System.Data.dll");
			references.Add("System.Xml.dll");
			references.Add("System.Xml.Linq.dll");
#elif NETCORE

#endif
			references.Add(typeof(DbAccessLayer).Assembly.Location);

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

			var syntaxTree = CSharpSyntaxTree.ParseText(generator.RenderPocoClass(), new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None, SourceCodeKind.Regular));
			using (var assemblyStream = new MemoryStream())
			{
				EmitResult compileAssemblyFromDom;
				try
				{
					var cSharpCompilation = CSharpCompilation.Create(target.Type.FullName
					                                                 + "_Poco.dll",
						new[] {syntaxTree},
						_defaultReferences.Concat(new MetadataReference[]
						{
							MetadataReference.CreateFromFile(target.Type.Assembly.Location), 
						}),
						new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

					cSharpCompilation = cSharpCompilation.AddReferences();

					compileAssemblyFromDom = cSharpCompilation
						.Emit(assemblyStream);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
				finally
				{
					
				}
			
				if (!compileAssemblyFromDom.Success)
				{
					var sb = new StringBuilder(string.Format("There are {0} errors due compilation.",
						compileAssemblyFromDom.Diagnostics.Length));
					var errNr = 0;
					foreach (var error in compileAssemblyFromDom.Diagnostics)
					{
						sb.AppendLine(errNr++ + error.Id + ":" + error.Location.GetLineSpan() + " -> " + error.GetMessage());
					}
					var ex =
						new InvalidDataException(sb.ToString());

					ex.Data.Add("Object", compileAssemblyFromDom);

					throw ex;
				}

				var compiledAssembly = Assembly.Load(assemblyStream.ToArray());

				var targetType = compiledAssembly.DefinedTypes.First();
				var constructorInfos = targetType.GetConstructors();
				if (!constructorInfos.Any())
				{
					if (settings.EnforceCreation)
					{
						return null;
					}
					var ex =
						new InvalidDataException("There are was an unknown error due compilation. No CTOR was build");

					ex.Data.Add("Object", compileAssemblyFromDom);
					for (var index = 0; index < compileAssemblyFromDom.Diagnostics.Length; index++)
					{
						var error = compileAssemblyFromDom.Diagnostics[index];
						ex.Data.Add(index, error);
					}
					throw ex;
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
				matchingCtor = matchingCtor ?? throw new InvalidOperationException(
					$"Could not find a matching constructor \".ctor(EagarDataRecord record)\" on \"{targetType}\"");

				return (record) => matchingCtor.Invoke(new object[] {record});

				//var dm = new DynamicMethod("Create" + target.Name.Split('.')[0], target.Type, new[] { typeof(IDataRecord) },
				//	target.Type, true);
				//var il = dm.GetILGenerator();

				//il.Emit(OpCodes.Ldarg_0);
				//il.Emit(OpCodes.Newobj, matchingCtor ?? throw new InvalidOperationException());
				//il.Emit(OpCodes.Ret);

				//if (!settings.CreateDebugCode)
				//{
				//	foreach (string tempFile in cp.TempFiles)
				//	{
				//		if (!tempFile.EndsWith("dll") && !tempFile.EndsWith("pdb"))
				//		{
				//			File.Delete(tempFile);
				//		}
				//	}
				//}

				//var func = (Func<IDataRecord, object>)dm.CreateDelegate(typeof(Func<IDataRecord, object>));
				//return func;
			}

		}
	}
}