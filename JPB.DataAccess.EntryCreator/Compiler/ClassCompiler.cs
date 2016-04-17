/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.EntityCreator.MsSql;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.EntityCreator.Compiler
{
	public class ClassCompiler : ElementCompiler
	{
		public ClassCompiler(string targetDir, string targetCsName)
			: base(targetDir, targetCsName)
		{

		}

		internal CodeMemberProperty AddFallbackProperty()
		{
			var codeMemberProperty = AddProperty("FallbackDictorary", typeof(Dictionary<string, object>));
			var fallbackAtt = new LoadNotImplimentedDynamicAttribute();
			codeMemberProperty.CustomAttributes.Add(new CodeAttributeDeclaration(fallbackAtt.GetType().Name));
			return codeMemberProperty;
		}

		internal CodeMemberProperty AddProperty(ColumInfoModel info)
		{
			var propertyName = info.GetPropertyName();
			var targetType = info.ColumnInfo.TargetType.FullName;
			CodeMemberProperty codeMemberProperty;
			if (info.EnumDeclaration != null)
			{
				codeMemberProperty = AddProperty(propertyName, new CodeTypeReference(info.EnumDeclaration.Name));
				//var enumConverter = new ValueConverterAttribute(typeof(EnumMemberConverter));
				codeMemberProperty.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(ValueConverterAttribute).Name, new CodeAttributeArgument(new CodeTypeOfExpression(typeof(EnumMemberConverter)))));
			}
			else
			{
				codeMemberProperty = AddProperty(propertyName, new CodeTypeReference(targetType));

				if (info.IsRowVersion)
				{
					var forModel = new RowVersionAttribute();
					codeMemberProperty.CustomAttributes.Add(new CodeAttributeDeclaration(forModel.GetType().Name));
				}
			}

			if (!string.IsNullOrEmpty(info.NewColumnName))
			{
				var forModel = new ForModelAttribute(info.ColumnInfo.ColumnName);
				codeMemberProperty.CustomAttributes.Add(new CodeAttributeDeclaration(forModel.GetType().Name, new CodeAttributeArgument(new CodePrimitiveExpression(forModel.AlternatingName))));
			}


			return codeMemberProperty;
		}

		internal CodeMemberProperty AddProperty(string name, Type type)
		{
			return AddProperty(name, new CodeTypeReference(type));
		}

		internal CodeMemberProperty AddProperty(string name, CodeTypeReference propType)
		{
			var property = new CodeMemberProperty();

			property.Attributes = (MemberAttributes)24578; //Public Final
			property.HasGet = true;
			property.HasSet = true;
			property.Name = name;

			property.Type = propType;

			var memberName = char.ToLower(property.Name[0]) + property.Name.Substring(1);
			memberName = memberName.Insert(0, "_");

			var field = new CodeMemberField() {
				Name = memberName,
				Type = propType,
				Attributes = MemberAttributes.Private
			};

			property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName)));
			property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberName), new CodePropertySetValueReferenceExpression()));

			_base.Members.Add(field);
			_base.Members.Add(property);
			return property;
		}

		//public CodeMemberMethod GenerateTypeConstructor(
		//	IEnumerable<KeyValuePair<string, Tuple<string, Type>>> propertyToDbColumn)
		//{
		//	return FactoryHelper.GenerateTypeConstructor(propertyToDbColumn, NewNamespace);
		//}
		
		internal void GenerateTypeConstructorBasedOnElements(IEnumerable<ColumInfoModel> columnInfos)
		{
			Add(new CodeConstructor() {
				Attributes = MemberAttributes.Public
			});

			Add(FactoryHelper.GenerateTypeConstructor(ColumninfosToInfoCache(columnInfos), Namespace));
		}

		public override void PreCompile()
		{
			_base.IsClass = true;
		}
	}
}
