using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.EntityCreator.Core.Compiler;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.Logger;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntityCreator.Core
{
	public static class SharedMethods
	{
		public static ILogger Logger = new DefaultLogger();

		public static void CompileTable(ITableInfoModel tableInfoModel, IMsSqlCreator sourceCreator, Stream to = null)
		{
			if (tableInfoModel.Exclude)
				return;

			var targetCsName = tableInfoModel.GetClassName();

			var compiler = new ClassCompiler(sourceCreator.TargetDir, targetCsName);
			compiler.CompileHeader = sourceCreator.GenerateCompilerHeader;
			compiler.Namespace = sourceCreator.Namespace;
			compiler.TableName = tableInfoModel.Info.TableName;
			if (to != null)
			{
				compiler.WriteAllways = true;
			}
			compiler.GenerateConfigMethod = sourceCreator.GenerateConfigMethod;

			if (tableInfoModel.CreateSelectFactory || sourceCreator.GenerateConstructor)
			{
				compiler.GenerateTypeConstructorBasedOnElements(tableInfoModel.ColumnInfos.Where(s => !s.Exclude));
			}

			foreach (var columInfoModel in tableInfoModel.ColumnInfos)
			{
				if (columInfoModel.Exclude)
					continue;

				var codeMemberProperty = compiler.AddProperty(columInfoModel);
				if (columInfoModel.PrimaryKey)
				{
					codeMemberProperty.CustomAttributes.Add(
						new CodeAttributeDeclaration(typeof(PrimaryKeyAttribute).Name));
				}
				if (columInfoModel.InsertIgnore)
				{
					codeMemberProperty.CustomAttributes.Add(
						new CodeAttributeDeclaration(typeof(InsertIgnoreAttribute).Name));
				}
				if (columInfoModel.ForgeinKeyDeclarations != null)
				{
					var isRefTypeKnown =
						sourceCreator.Tables.FirstOrDefault(s => s.Info.TableName == columInfoModel.ForgeinKeyDeclarations.TableName);

					if (isRefTypeKnown == null)
					{
						codeMemberProperty.CustomAttributes.Add(
							new CodeAttributeDeclaration(typeof(ForeignKeyDeclarationAttribute).Name,
								new CodeAttributeArgument(new CodePrimitiveExpression(columInfoModel.ForgeinKeyDeclarations.TargetColumn)),
								new CodeAttributeArgument(new CodePrimitiveExpression(columInfoModel.ForgeinKeyDeclarations.TableName))));
					}
					else
					{
						codeMemberProperty.CustomAttributes.Add(
							new CodeAttributeDeclaration(typeof(ForeignKeyDeclarationAttribute).Name,
								new CodeAttributeArgument(new CodePrimitiveExpression(columInfoModel.ForgeinKeyDeclarations.TargetColumn)),
								new CodeAttributeArgument(new CodeTypeOfExpression(isRefTypeKnown.GetClassName()))));
					}
				}

				if (columInfoModel.ColumnInfo.TargetType2 == "Timestamp")
				{
					codeMemberProperty.CustomAttributes.Add(
						new CodeAttributeDeclaration(typeof(RowVersionAttribute).Name));
				}
			}

			if (tableInfoModel.CreateFallbackProperty)
			{
				compiler.AddFallbackProperty();
			}

			Logger.WriteLine("Compile Class {0}", compiler.Name);
			compiler.Compile(tableInfoModel.ColumnInfos, to);
		}

		public static void AutoAlignNames(IEnumerable<ITableInfoModel> tableNames)
		{
			Logger.WriteLine("Auto rename Columns after common cs usage");
			Logger.WriteLine();
			foreach (var tableInfoModel in tableNames)
			{
				var tableName = tableInfoModel.Info.TableName;
				Logger.WriteLine("Check Table: {0}", tableName);
				var newName = CheckOrAlterName(tableName);
				if (newName != tableName)
				{
					Console.WriteLine("Alter Table'{0}' to '{1}'", tableName, newName);
					tableInfoModel.NewTableName = newName;
				}

				Logger.WriteLine();

				foreach (var columnInfo in tableInfoModel.ColumnInfos)
				{
					var columnName = columnInfo.ColumnInfo.ColumnName;
					Logger.WriteLine("\tCheck Column: {0}", columnName);
					var newColumnName = CheckOrAlterName(columnName);
					if (newColumnName != columnName)
					{
						Logger.WriteLine("\tAlter Column'{0}' to '{1}'", columnName, newColumnName);
						columnInfo.NewColumnName = newColumnName;
					}
				}
			}

			Logger.WriteLine("Renaming is done");
		}

		private static readonly char[] unvalid = { '_', ' ', '.' };

		public static string CheckOrAlterName(string name)
		{
			var newName = name;

			foreach (var unvalidPart in unvalid)
			{
				if (newName.Contains(unvalidPart))
				{
					var indexOfElement = newName.IndexOf(unvalidPart.ToString(CultureInfo.InvariantCulture), System.StringComparison.Ordinal) + 1;
					if (indexOfElement < newName.Length)
					{
						var elementAt = newName.ElementAt(indexOfElement);
						if (!unvalid.Contains(elementAt))
						{
							var remove = newName.Remove(indexOfElement, 1);
							newName = remove.Insert(indexOfElement, elementAt.ToString(CultureInfo.InvariantCulture).ToUpper());
						}
					}
					newName = newName.Replace(unvalidPart.ToString(CultureInfo.InvariantCulture), string.Empty);
				}
			}

			return newName;
		}
	}
}
