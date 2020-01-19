using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.EntityCollections;
using JPB.DataAccess.Helper;

#pragma warning disable 1591

namespace JPB.DataAccess.DbInfoConfig.ClassBuilder
{
	public class PropertyInfo
	{
		private string _name;

		public PropertyInfo()
		{
			Attributes = new List<AttributeInfo>();
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
			}
		}

		public string DbName { get; set; }
		public ClassType Type { get; set; }
		public bool IsRest { get; set; }
		public string ValueConverterType { get; set; }
		public bool IsXml { get; set; }
		public ForeignKeyDeclaration ForeignKey { get; set; }

		public IList<AttributeInfo> Attributes { get; set; }

		private string GetDbName()
		{
			return DbName ?? Name;
		}

		public void RenderProperty(ConsoleStringBuilderInterlaced sb, bool generateConfigMethod,
			bool notifyPropertysChanged)
		{
			var memberName = "_" + Char.ToLowerInvariant(Name[0]) + Name.Substring(1);
			if (notifyPropertysChanged)
			{
				sb.AppendInterlacedLine(
					$"private {Type.GetTypeName()} {memberName};");
			}

			if (!generateConfigMethod)
			{
				foreach (var attributeInfo in Attributes)
				{
					attributeInfo.Render(sb);
				}
			}
			else
			{
				foreach (var attributeInfo in Attributes.Where(e => e.DoesNotSupportDbConfigApi))
				{
					attributeInfo.Render(sb);
				}
			}

			sb.AppendInterlaced("public ");
			if (ForeignKey != null)
			{
				sb.Append("virtual ");
			}

			sb.Append($"{Type.GetTypeName()}");
			sb.Append($" {Name} ");
			if (!notifyPropertysChanged)
			{
				sb.AppendLine($" {{ get; set; }}");
			}
			else
			{
				sb.AppendLine($"");
				sb.AppendInterlacedLine("{")
					.Up()
					.AppendInterlacedLine($"get {{ return {memberName}; }}")
					.AppendInterlacedLine("set")
					.AppendInterlacedLine("{")
					.Up()
					.AppendInterlacedLine($"{memberName} = value;")
					.AppendInterlacedLine("SendPropertyChanged();")
					.Down()
					.AppendInterlacedLine("}")
					.Down()
					.AppendInterlacedLine("}");
			}
		}

		public void RenderAssignment(ConsoleStringBuilderInterlaced sb, string readerName, string targetName)
		{
			if (IsRest)
			{
				return;
			}

			DbName = $"\"{GetDbName()}\"";

			var readFromTarget = $"reader[{DbName}]";

			if (IsXml)
			{
				readFromTarget = $"(string)reader[{DbName}]";

				if (!string.IsNullOrWhiteSpace(ValueConverterType))
				{
					readFromTarget = $"new {ValueConverterType}().{nameof(IValueConverter.Convert)}({readFromTarget});";
				}

				if (Type.IsList)
				{
					var pocoName = Type.GenericTypes[0].Name;
					readFromTarget = $@"new {nameof(NonObservableDbCollection<object>)}<{pocoName}>(
			{nameof(XmlDataRecord)}.{nameof(XmlDataRecord.TryParse)}(
				xmlStream: {readFromTarget},
				target: {pocoName},
				single: false)
	            .{nameof(XmlDataRecord.CreateListOfItems)}()
	            .Select(item => {pocoName}
					.{nameof(DbConfigHelper.GetClassInfo)}()
					.{nameof(DbAccessLayerHelper.SetPropertiesViaReflection)}(reader: {nameof(EagarDataRecord)}.{nameof(EagarDataRecord.WithExcludedFields)}(item))))";
				}
				else
				{
					readFromTarget = $"{nameof(XmlDataRecord)}.{nameof(XmlDataRecord.TryParse)}" +
					                 $"(xmlStream: (string){readFromTarget}, " +
					                 $"target: {Type.Name}, " +
					                 "single: true)";
				}
			}
			else if (ForeignKey != null)
			{
				var varName = $"readersOf{Name}";

				if (ForeignKey.DirectionFromParent)
				{
					sb.AppendInterlacedLine(
						$"var {varName} = (({nameof(EagarDataRecord)}[])reader[{DbName}]);");
					var realType = Type.GenericTypes.FirstOrDefault();

					readFromTarget =
						$@"{varName} == null ? null : new {Type.GetTypeName()}({varName}.Select(item => (({realType.Name})(typeof({realType.Name}).{nameof(DbConfigHelper.GetClassInfo)}().{nameof(DbAccessLayerHelper.SetPropertiesViaReflection)}(reader: item)))))";

				}
				else
				{
					sb.AppendInterlacedLine(
						$"var {varName} = (({nameof(EagarDataRecord)}[])reader[{DbName}])?.FirstOrDefault();");
					readFromTarget =
						$@"{varName} == null ? null : (({Type.GetTypeName()})(typeof({Type.GetTypeName()}).{nameof(DbConfigHelper.GetClassInfo)}().{nameof(DbAccessLayerHelper.SetPropertiesViaReflection)}(reader: {varName})))";
				}
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(ValueConverterType))
				{
					readFromTarget = $"new {ValueConverterType}().{nameof(IValueConverter.Convert)}({readFromTarget});";
				}

				var typeAssignment = $"{Type.GetTypeName()}";
				readFromTarget = $"({typeAssignment}){readFromTarget}";
			}

			sb.AppendInterlacedLine(
				$"{targetName}.{Name} = {readFromTarget};");
		}

		public class ForeignKeyDeclaration
		{
			public ForeignKeyDeclaration()
			{
			}

			public ForeignKeyDeclaration(string attributeForeignKey, string attributeReferenceKey)
			{
				SourceColumn = attributeForeignKey;
				TargetColumn = attributeForeignKey;
			}

			public string SourceColumn { get; set; }
			public string TargetColumn { get; set; }
			public bool DirectionFromParent { get; set; }
		}
	}
}