/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.ClassBuilder;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntityCreator.Core.Compiler
{
	public class ClassCompiler : ElementCompiler
	{
		private static string GetDefaultTemplate()
		{
#if DEBUG
			return File.ReadAllText("../../../../JPB.DataAccess.EntityCreator.Core/DefaultTemplate.mdoc");
#endif
			return File.ReadAllText("DefaultTemplate.mdoc");
		}

		public ClassCompiler(string targetDir, string targetCsName)
			: base(targetDir, targetCsName, new DefaultLogger(), GetDefaultTemplate())
		{

		}

		public PropertyInfo AddFallbackProperty()
		{
			var codeMemberProperty = AddProperty("FallbackDictionary", null, typeof(Dictionary<string, object>));
			codeMemberProperty.Attributes.Add(new AttributeInfo()
			{
				Name = nameof(LoadNotImplimentedDynamicAttribute),
			});
			return codeMemberProperty;
		}

		public PropertyInfo AddProperty(IColumInfoModel info)
		{
			var propertyName = info.GetPropertyName();
			var targetType = info.ColumnInfo.TargetType;
			if (info.ColumnInfo.Nullable && !info.ColumnInfo.TargetType.IsClass)
			{
				targetType = typeof(Nullable<>).MakeGenericType(targetType);
			}

			var codeMemberProperty = AddProperty(propertyName, info.ColumnInfo.ColumnName, targetType);

			if (info.IsRowVersion)
			{
				codeMemberProperty.Attributes.Add(new AttributeInfo() { Name = nameof(RowVersionAttribute) });
			}

			if (!string.IsNullOrEmpty(info.NewColumnName))
			{
				codeMemberProperty.Attributes.Add(new AttributeInfo()
				{
					Name = nameof(ForModelAttribute),
					ConstructorSetters =
					{
						{"alternatingName", "\"" + info.ColumnInfo.ColumnName + "\""}
					}
				});
			}
			return codeMemberProperty;
		}

		public PropertyInfo AddProperty(string name, string dbName, Type type)
		{
			return AddProperty(name, dbName, BuilderType.FromCsType(type));
		}

		public PropertyInfo AddProperty(string name, string dbName, IBuilderType propType)
		{
			var property = new PropertyInfo();
			property.Name = name;
			property.DbName = dbName;
			property.Type = propType;
			base.Generator.Properties.Add(property);
			return property;
		}

		/// <inheritdoc />
		public override string Type { get; set; } = "Table";

		public override void PreCompile()
		{
		}
	}
}

