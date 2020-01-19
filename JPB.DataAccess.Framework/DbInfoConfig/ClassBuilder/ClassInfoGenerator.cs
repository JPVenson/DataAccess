﻿using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.EntityCollections;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.DbInfoConfig.ClassBuilder
{
	/// <summary>
	///		Creates POCO entities for ORM Usage
	/// </summary>
	public class ClassInfoGenerator
	{
		/// <summary>
		/// 
		/// </summary>
		public ClassInfoGenerator()
		{
			NamespaceImports = new HashSet<string>();
			CompilerHeader = new List<string>();
			Properties = new List<PropertyInfo>();
			Attributes = new List<AttributeInfo>();
		}

		/// <summary>
		///		Defines the list of Namespaces that must be imported
		/// </summary>
		public HashSet<string> NamespaceImports { get; set; }

		/// <summary>
		///		Sets or gets the Namespace this POCO is in
		/// </summary>
		public string Namespace { get; set; }

		/// <summary>
		///		Sets or gets the name of this class
		/// </summary>
		public string ClassName { get; set; }

		/// <summary>
		///		Gets or Sets the list of all properties
		/// </summary>
		public IList<PropertyInfo> Properties { get; set; }

		/// <summary>
		///		if set a Ado.net constructor will be created
		/// </summary>
		public bool GenerateConstructor { get; set; }

		/// <summary>
		///		if set a ado.net factory will be created
		/// </summary>
		public bool GenerateFactory { get; set; }

		/// <summary>
		///		The chain of all properties or interfaces that are inherted
		/// </summary>
		public string Inherts { get; set; }

		/// <summary>
		///		A list of comments added to the class
		/// </summary>
		public IList<string> CompilerHeader { get; }

		/// <summary>
		///		All attributes of this class
		/// </summary>
		public IList<AttributeInfo> Attributes { get; set; }

		/// <summary>
		///		If set a configuration method will be created and all attributes will be moved there
		/// </summary>
		public bool GenerateConfigMethod { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public GeneratedCodeAttribute GeneratedCodeAttribute { get; set; }

		/// <summary>
		///		Flag for controling whenever this is a Super class factory
		/// </summary>
		public bool IsSuperClass { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string RenderPocoClass(bool notifyPropertysChanged = false)
		{
			var sb = new ConsoleStringBuilderInterlaced();
			if (GenerateConfigMethod)
			{
				NamespaceImports.Add(typeof(ConfigurationResolver<>).Namespace);
			}

			if (notifyPropertysChanged)
			{
				NamespaceImports.Add(typeof(INotifyPropertyChanged).Namespace);
				NamespaceImports.Add(typeof(CallerMemberNameAttribute).Namespace);
			}

			NamespaceImports.Add(typeof(EagarDataRecord).Namespace);
			NamespaceImports.Add(typeof(GeneratedCodeAttribute).Namespace);
			NamespaceImports.Add(typeof(DbCollection<>).Namespace);

			sb.AppendInterlacedLine("//------------------------------------------------------------------------------");
			sb.AppendInterlacedLine("// <auto-generated>");
			sb.AppendInterlacedLine("// This code was generated by a tool.");
			sb.AppendInterlacedLine("// Runtime Version:2.0.50727.42");
			sb.AppendInterlacedLine("// Changes to this file may cause incorrect behavior and will be lost if");
			sb.AppendInterlacedLine("// the code is regenerated.");
			sb.AppendInterlacedLine("// </auto-generated>");
			sb.AppendInterlacedLine("//------------------------------------------------------------------------------");


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
			if (!string.IsNullOrWhiteSpace(Inherts) || notifyPropertysChanged)
			{
				sb.Append($": {Inherts}");
				if (notifyPropertysChanged)
				{
					if (!string.IsNullOrWhiteSpace(Inherts))
					{
						sb.Append(", ");
					}
					sb.Append(nameof(INotifyPropertyChanged));
				}
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
					propertyInfo.RenderProperty(sb, GenerateConfigMethod, notifyPropertysChanged);
				}
			}

			if (GenerateFactory)
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
					foreach (var propertyInfoAttribute in propertyInfo.Attributes
						.Where(e => !e.DoesNotSupportDbConfigApi))
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

			if (notifyPropertysChanged)
			{
				sb.AppendInterlacedLine($"#region {nameof(INotifyPropertyChanged)}");
				sb.AppendInterlacedLine(
					$"public event {nameof(PropertyChangedEventHandler)} {nameof(INotifyPropertyChanged.PropertyChanged)};");
				sb.AppendInterlacedLine("");
				sb.AppendInterlacedLine("protected virtual void SendPropertyChanged([CallerMemberName] string propertyName = null)");
				sb.AppendInterlacedLine("{").Up();
				sb.AppendInterlacedLine("PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));");
				sb.Down().AppendInterlacedLine("}");
				sb.AppendInterlacedLine("");
				sb.AppendInterlacedLine("#endregion");
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
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