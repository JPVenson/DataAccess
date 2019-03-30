using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;

#pragma warning disable 1591

namespace JPB.DataAccess.DbInfoConfig.ClassBuilder
{
	public class ClassInfoGenerator
	{
		public ClassInfoGenerator()
		{
			NamespaceImports = new HashSet<string>();
			CompilerHeader = new List<string>();
			Properties = new List<PropertyInfo>();
			Attributes = new List<AttributeInfo>();
		}

		public HashSet<string> NamespaceImports { get; set; }
		public string Namespace { get; set; }
		public string ClassName { get; set; }
		public IList<PropertyInfo> Properties { get; set; }
		public bool GenerateConstructor { get; set; }
		public bool GenerateFactory { get; set; }
		public string Inherts { get; set; }
		public IList<string> CompilerHeader { get; }
		public IList<AttributeInfo> Attributes { get; set; }
		public bool GenerateConfigMethod { get; set; }
		public GeneratedCodeAttribute GeneratedCodeAttribute { get; set; }
		public bool IsSuperClass { get; set; }


		public string RenderPocoClass()
		{
			var sb = new ConsoleStringBuilderInterlaced();
			if (GenerateConfigMethod)
			{
				NamespaceImports.Add(typeof(ConfigurationResolver<>).Namespace);
			}

			NamespaceImports.Add(typeof(EagarDataRecord).Namespace);
			NamespaceImports.Add(typeof(GeneratedCodeAttribute).Namespace);
			NamespaceImports.Add(typeof(DbCollection<>).Namespace);
			foreach (var namespaceImport in NamespaceImports)
			{
				sb.AppendLine($"using {namespaceImport};");
			}

			sb.AppendLine();
			sb.AppendLine("namespace " + Namespace);
			sb.AppendLine("{");
			sb.Up();
			sb.AppendLine();
			foreach (var headerLine in CompilerHeader)
			{
				sb.AppendInterlacedLine($"//{headerLine}");
			}

			sb.AppendLine();
			if (!GenerateConfigMethod)
			{
				foreach (var attributeInfo in Attributes)
				{
					attributeInfo.Render(sb);
				}
			}
			if (GeneratedCodeAttribute != null)
			{
				sb.AppendInterlacedLine(
				$"[{nameof(GeneratedCodeAttribute)}(tool: \"{GeneratedCodeAttribute.Tool}\", version: \"{GeneratedCodeAttribute.Version}\")]");
			}

			sb.AppendInterlaced("public partial class " + ClassName);
			if (!string.IsNullOrWhiteSpace(Inherts))
			{
				sb.Append($": {Inherts}");
			}

			sb.AppendLine();
			sb.AppendInterlacedLine("{");
			sb.Up();
			sb.AppendInterlacedLine($"public {ClassName}() {{}}");

			if (GenerateConstructor && !GenerateFactory)
			{
				var readerName = "reader";
				sb.AppendInterlacedLine($"public {ClassName}({nameof(EagarDataRecord)} {readerName}) ");
				sb.AppendInterlacedLine("{").Up();

				foreach (var propertyInfo in Properties)
				{
					propertyInfo.RenderAssignment(sb, readerName, "this");
				}

				sb.Down().AppendInterlacedLine("}");
			}

			if (!IsSuperClass)
			{
				foreach (var propertyInfo in Properties)
				{
					propertyInfo.RenderProperty(sb, GenerateConfigMethod);
				}
			}

			if (!GenerateConstructor && GenerateFactory)
			{
				var readerName = "reader";
				sb.AppendInterlacedLine($"public static {ClassName} Factory({nameof(EagarDataRecord)} {readerName})");
				sb.AppendInterlacedLine("{").Up();
				var targetName = "super";
				sb.AppendInterlacedLine($"var {targetName} = new {ClassName}();");

				foreach (var propertyInfo in Properties)
				{
					propertyInfo.RenderAssignment(sb, readerName, targetName);
				}

				sb.AppendInterlacedLine($"return {targetName};");

				sb.Down().AppendInterlacedLine("}");
			}

			if (GenerateConfigMethod)
			{
				var configArgument = string.Format("ConfigurationResolver<{0}>", ClassName);
				var configName = "config";
				string[] eventNames =
				{
					"BeforeConfig()", "AfterConfig()", $"BeforeConfig({configArgument} {configName})",
					$"AfterConfig({configArgument} {configName})"
				};

				foreach (var eventName in eventNames)
				{
					sb.AppendInterlacedLine($"static partial void {eventName};");
				}

				sb.AppendInterlacedLine($"[{nameof(ConfigMehtodAttribute)}]");
				sb.AppendInterlacedLine($"public static void Configuration({configArgument} {configName})");
				sb.AppendInterlacedLine("{");
				sb.Up();
				sb.AppendInterlacedLine("BeforeConfig();");
				sb.AppendInterlacedLine($"BeforeConfig({configName});");

				if (!GenerateConstructor)
				{
					sb.AppendInterlacedLine(
						$"config.{nameof(ConfigurationResolver<object>.SetFactory)}" +
						"(" +
						$"{ClassName}.Factory, " +
						"true" +
						");");
				}

				foreach (var propertyInfoAttribute in Attributes)
				{
					sb.AppendInterlacedLine(
						$"config.{nameof(ConfigurationResolver<object>.SetClassAttribute)}" +
						"(" +
						$"new {propertyInfoAttribute.Name}({propertyInfoAttribute.RenderAssignments()})" +
						");");
				}

				foreach (var propertyInfo in Properties)
				{
					foreach (var propertyInfoAttribute in propertyInfo.Attributes)
					{
						sb.AppendInterlacedLine(
							$"config.{nameof(ConfigurationResolver<object>.SetPropertyAttribute)}" +
							"(" +
							$"s => s.{propertyInfo.Name}" +
							", " +
							$"new {propertyInfoAttribute.Name}({propertyInfoAttribute.RenderAssignments()})" +
							");");
					}
				}

				sb.AppendInterlacedLine($"AfterConfig({configName});");
				sb.AppendInterlacedLine("AfterConfig();");

				sb.Down();
				sb.AppendInterlacedLine("}");
			}

			sb.Down();
			sb.AppendInterlacedLine("}");
			sb.Down();
			sb.AppendLine("}");
			using (var writer = new StringWriter())
			{
				sb.WriteToSteam(writer, wrapper => { }, () => { });
				var stringBuilder = writer.GetStringBuilder();
				return stringBuilder.ToString();
			}
		}

		public static ClassInfoGenerator SuperClass(DbClassInfoCache target)
		{
			var gen = new ClassInfoGenerator();
			var codeName = target.Name.Split('.').Last();
			gen.ClassName = codeName + "_Super";
			gen.IsSuperClass = true;
			gen.Attributes.Add(new AttributeInfo()
			{
				Name = nameof(ForModelAttribute),
				ConstructorSetters =
				{
					{"alternatingName", target.TableName.AsStringOfString()}
				}
			});
			gen.Inherts = target.Name;
			gen.GenerateConstructor = true;

			foreach (var dbPropertyInfoCach in target.Propertys)
			{
				gen.Properties.Add(new PropertyInfo
				{
					Type = ClassType.FromProperty(dbPropertyInfoCach.Value),
					Name = dbPropertyInfoCach.Value.PropertyName,
					DbName = dbPropertyInfoCach.Value.DbName,
					ForeignKey = dbPropertyInfoCach.Value.ForginKeyAttribute == null
						? null
						: new PropertyInfo.ForeignKeyDeclaration(
							dbPropertyInfoCach.Value.ForginKeyAttribute.Attribute.ForeignKey,
							dbPropertyInfoCach.Value.ForginKeyAttribute.Attribute.ReferenceKey),
					IsRest = dbPropertyInfoCach.Value.Attributes.Any(f =>
						f.Attribute is LoadNotImplimentedDynamicAttribute),
					IsXml = dbPropertyInfoCach.Value.FromXmlAttribute != null,
					ValueConverterType =
						(dbPropertyInfoCach.Value.Attributes.FirstOrDefault(f => f.Attribute is ValueConverterAttribute)
							?.Attribute as ValueConverterAttribute)?.Converter.FullName
				});
			}

			return gen;
		}
	}
}