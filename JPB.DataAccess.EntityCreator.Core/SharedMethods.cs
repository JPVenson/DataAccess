using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.ClassBuilder;
using JPB.DataAccess.EntityCollections;
using JPB.DataAccess.EntityCreator.Core.Compiler;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntityCreator.Core
{
	public static class SharedMethods
	{
		public static ILogger Logger = new DefaultLogger();

		public static IEnumerable<ISharedInterface> GetSuggestedInterfaces(IEnumerable<ITableInfoModel> tables)
		{
			IDictionary<string, Dictionary<ITableInfoModel, IColumInfoModel>> sharedProps = new Dictionary<string, Dictionary<ITableInfoModel, IColumInfoModel>>();

			foreach (var tableInfoModel in tables)
			{
				foreach (var columInfoModel in tableInfoModel.ColumnInfos)
				{
					var columnName = columInfoModel.GetPropertyName();
					var hasValue = sharedProps.FirstOrDefault(f => f.Key == columnName).Value;

					if (hasValue == null)
					{
						sharedProps.Add(columnName, hasValue = new Dictionary<ITableInfoModel, IColumInfoModel>());
					}
					hasValue.Add(tableInfoModel, columInfoModel);
				}
			}
			sharedProps = sharedProps.Where(f => f.Value.Count > 1)
									 .ToDictionary(f => f.Key, f => f.Value.ToDictionary(g => g.Key, g => g.Value));
			return sharedProps.Select(f => new SharedInterface("IHas" + f.Key, null, f.Value.Select(e => e.Value).ToList())).ToList();
		}

		public static void CompileView(ITableInfoModel tableInfoModel, IMsSqlCreator sourceCreator, Stream to = null)
		{
			CompileTableLike(tableInfoModel, sourceCreator, "View", to);
		}

		public static void CompileTable(ITableInfoModel tableInfoModel, IMsSqlCreator sourceCreator, Stream to = null)
		{
			CompileTableLike(tableInfoModel, sourceCreator, "Table", to);
		}

		private static void CompileTableLike(ITableInfoModel tableInfoModel,
			IMsSqlCreator sourceCreator,
			string typeName,
			Stream to = null)
		{
			if (tableInfoModel.Exclude)
			{
				return;
			}

			var targetCsName = tableInfoModel.GetClassName();

			var compiler = new ClassCompiler(sourceCreator.TargetDir, targetCsName);
			
			compiler.Type = typeName;
			compiler.CompileHeader = sourceCreator.GenerateCompilerHeader;
			compiler.Namespace = sourceCreator.Namespace;
			compiler.TableName = tableInfoModel.Info.TableName;
			if (to != null)
			{
				compiler.WriteAlways = true;
			}

			compiler.GenerateConfigMethod = sourceCreator.GenerateConfigMethod;

			foreach (var columInfoModel in tableInfoModel.ColumnInfos)
			{
				if (columInfoModel.Exclude)
				{
					continue;
				}

				var codeMemberProperty = compiler.AddProperty(columInfoModel);

				if (sourceCreator.GenerateDbValidationAnnotations)
				{
					compiler.Namespaces.Add(typeof(ValidationAttribute).Namespace);
					if (!columInfoModel.ColumnInfo.Nullable)
					{
						//new RequiredAttribute();
						codeMemberProperty.Attributes.Add(new AttributeInfo()
						{
							Name = nameof(RequiredAttribute),
							DoesNotSupportDbConfigApi = true
						});
					}

					if (columInfoModel.ColumnInfo.MaxLength.HasValue 
					    && columInfoModel.ColumnInfo.MaxLength > 0)
					{
						if ((
							columInfoModel.ColumnInfo.SqlType == SqlDbType.NVarChar
							||
							columInfoModel.ColumnInfo.SqlType == SqlDbType.Char
							||
							columInfoModel.ColumnInfo.SqlType == SqlDbType.NChar
							||
							columInfoModel.ColumnInfo.SqlType == SqlDbType.VarChar
						))
						{
							//new StringLengthAttribute();
							codeMemberProperty.Attributes.Add(new AttributeInfo()
							{
								Name = nameof(StringLengthAttribute),
								PropertySetters =
								{
									{ "maximumLength", columInfoModel.ColumnInfo.MaxLength.Value.ToString() }
								},
								DoesNotSupportDbConfigApi = true
							});
						}
						else if (columInfoModel.ColumnInfo.SqlType == SqlDbType.Binary)
						{
							//new MaxLengthAttribute();
							codeMemberProperty.Attributes.Add(new AttributeInfo()
							{
								Name = nameof(MaxLengthAttribute),
								PropertySetters =
								{
									{ "length", columInfoModel.ColumnInfo.MaxLength.Value.ToString() }
								},
								DoesNotSupportDbConfigApi = true
							});
						}
					}
				}

				if (columInfoModel.PrimaryKey)
				{
					codeMemberProperty.Attributes.Add(new AttributeInfo() { Name = nameof(PrimaryKeyAttribute) });
				}

				if (columInfoModel.InsertIgnore)
				{
					codeMemberProperty.Attributes.Add(new AttributeInfo() { Name = nameof(InsertIgnoreAttribute) });
				}

				if (columInfoModel.ForgeinKeyDeclarations != null)
				{
					var isRefTypeKnown =
						sourceCreator.Tables
							.FirstOrDefault(s =>
								s.Info.TableName == columInfoModel.ForgeinKeyDeclarations.TableName);

					if (isRefTypeKnown == null)
					{
						codeMemberProperty.Attributes.Add(new AttributeInfo()
						{
							Name = nameof(ForeignKeyDeclarationAttribute),
							ConstructorSetters =
							{
								{"foreignKey", "\"" + columInfoModel.ForgeinKeyDeclarations.TargetColumn + "\""},
								{"foreignTable", "\"" + columInfoModel.ForgeinKeyDeclarations.TableName + "\""},
							}
						});
					}
					else
					{
						codeMemberProperty.Attributes.Add(new AttributeInfo()
						{
							Name = nameof(ForeignKeyDeclarationAttribute),
							ConstructorSetters =
							{
								{"foreignKey", $"\"{columInfoModel.ForgeinKeyDeclarations.TargetColumn}\""},
								{"foreignTable", $"typeof({isRefTypeKnown.GetClassName()})"},
							}
						});
					}
				}

				if (columInfoModel.ColumnInfo.SqlType == SqlDbType.Timestamp)
				{
					codeMemberProperty.Attributes.Add(new AttributeInfo() { Name = nameof(RowVersionAttribute) });
				}

				if (tableInfoModel.CreateFallbackProperty)
				{
					compiler.AddFallbackProperty();
				}
			}

			foreach (var columInfoModel in tableInfoModel.ColumnInfos)
			{
				if (columInfoModel.Exclude)
				{
					continue;
				}

				if (columInfoModel.ForgeinKeyDeclarations != null)
				{
					var isRefTypeKnown =
						sourceCreator.Tables
							.FirstOrDefault(s =>
								s.Info.TableName == columInfoModel.ForgeinKeyDeclarations.TableName);

					if (isRefTypeKnown != null)
					{
						compiler.Generator.NamespaceImports.Add(typeof(EagarDataRecord).Namespace);
						compiler.Generator.NamespaceImports.Add(typeof(DbAccessLayerHelper).Namespace);
						var navPropertyName = columInfoModel.GetPropertyName();
						if (navPropertyName.StartsWith("Id"))
						{
							navPropertyName = navPropertyName.Remove(0, 2);
						}
						var tempName = navPropertyName;
						var counter = 1;
						while (compiler.Generator.Properties.Any(f => f.Name == tempName || tempName == columInfoModel.GetPropertyName()))
						{
							tempName = navPropertyName + counter++;
						}

						navPropertyName = tempName;
						var refProp = compiler.AddProperty(navPropertyName, null, new BuilderType()
						{
							Name = isRefTypeKnown.GetClassName(),
						});
						refProp.ForeignKey = new PropertyInfo.ForeignKeyDeclaration(columInfoModel.ForgeinKeyDeclarations.SourceColumn,
							columInfoModel.ForgeinKeyDeclarations.TargetColumn);
						refProp.Attributes.Add(new AttributeInfo()
						{
							Name = nameof(ForeignKeyAttribute),
							ConstructorSetters =
							{
								{"foreignKey", columInfoModel.ForgeinKeyDeclarations.SourceColumn.AsStringOfString()},
								{"referenceKey", columInfoModel.ForgeinKeyDeclarations.TargetColumn.AsStringOfString()},
							}
						});
					}
				}
			}

			foreach (var infoModel in sourceCreator.Tables)
			{
				foreach (var columInfoModel in infoModel
					.ColumnInfos
					.Where(f => f.ForgeinKeyDeclarations?.TableName == tableInfoModel.Info.TableName))
				{

					compiler.Generator.NamespaceImports.Add(typeof(EagarDataRecord).Namespace);
					compiler.Generator.NamespaceImports.Add(typeof(DbAccessLayerHelper).Namespace);
					var navPropertyName = infoModel.GetClassName();
					if (navPropertyName.StartsWith("Id"))
					{
						navPropertyName = navPropertyName.Remove(0, 2);
					}
					
					if (navPropertyName.EndsWith("s"))
					{
						navPropertyName = navPropertyName.TrimEnd('s') + "es";
					}
					else if (navPropertyName.EndsWith("y"))
					{
						navPropertyName = navPropertyName.TrimEnd('y') + "ies";
					}
					else
					{
						navPropertyName += "s";
					}

					var tempName = navPropertyName;
					var counter = 1;
					while (compiler.Generator.Properties.Any(f => f.Name == tempName || tempName == columInfoModel.GetPropertyName()))
					{
						tempName = navPropertyName + counter++;
					}

					navPropertyName = tempName;

					var refProp = compiler.AddProperty(navPropertyName, null, new BuilderType()
					{
						Name = $"{nameof(DbCollection<object>)}",
						IsList = true,
						GenericTypes =
						{
							new BuilderType()
							{
								Name = infoModel.GetClassName()
							}
						}
					});
					refProp.ForeignKey = new PropertyInfo.ForeignKeyDeclaration(columInfoModel.ForgeinKeyDeclarations.SourceColumn,
						columInfoModel.ForgeinKeyDeclarations.TargetColumn);
					refProp.ForeignKey.DirectionFromParent = true;
					refProp.Attributes.Add(new AttributeInfo()
					{
						Name = nameof(ForeignKeyAttribute),
						ConstructorSetters =
						{
							{"referenceKey", columInfoModel.ForgeinKeyDeclarations.SourceColumn.AsStringOfString()},
							{"foreignKey", columInfoModel.ForgeinKeyDeclarations.TargetColumn.AsStringOfString()},
						}
					});
				}
			}

			compiler.GenerateConfigMethod = true;
			compiler.Generator.GenerateFactory = tableInfoModel.CreateSelectFactory || sourceCreator.GenerateFactory;
			compiler.Generator.GenerateConstructor = sourceCreator.GenerateConstructor;

			Logger.WriteLine("Compile Class {0}", compiler.Name);
			compiler.Compile(tableInfoModel.ColumnInfos, sourceCreator.SplitByType, sourceCreator.SetNotifyProperties, to);
		}

		public static void AutoAlignNames(IEnumerable<ITableInfoModel> tableNames, string tableSuffix = null)
		{
			Logger.WriteLine("Auto rename Columns after common cs usage");
			Logger.WriteLine();
			foreach (var tableInfoModel in tableNames)
			{
				var tableName = tableInfoModel.Info.TableName;
				if (!string.IsNullOrEmpty(tableInfoModel.NewTableName))
				{
					tableName = tableInfoModel.NewTableName;
				}

				Logger.WriteLine("Check Table: {0}", tableName);
				var newName = CheckOrAlterColumnName(tableName, tableSuffix);
				if (newName != tableName)
				{
					Logger.WriteLine("Alter Table '{0}' to '{1}'", tableName, newName);
					tableInfoModel.NewTableName = newName;
				}

				Logger.WriteLine();

				foreach (var columnInfo in tableInfoModel.ColumnInfos)
				{
					var columnName = columnInfo.ColumnInfo.ColumnName;
					if (!string.IsNullOrEmpty(columnInfo.NewColumnName))
					{
						columnName = columnInfo.NewColumnName;
					}

					Logger.WriteLine("\tCheck Column: '{0}'", columnName);
					var newColumnName = CheckOrAlterColumnName(columnName);
					if (newColumnName != columnName)
					{
						Logger.WriteLine("\tAlter Column '{0}' to '{1}'", columnName, newColumnName);
						columnInfo.NewColumnName = newColumnName;
					}
				}
			}

			Logger.WriteLine("Renaming is done");
		}

		private static readonly char[] _invalidColumnChars = { '_', ' ', '.' };

		public static string CheckOrAlterNavigationPropertyName(IColumInfoModel column, ITableInfoModel tableInfoModel,
			string suffix = null)
		{
			var sourceColumn = column.ForgeinKeyDeclarations.SourceColumn;
			if (sourceColumn.StartsWith("Id"))
			{
				sourceColumn = sourceColumn.Remove(0, 2);
			}
			return CheckOrAlterColumnName(sourceColumn);

			if (column.ForgeinKeyDeclarations.TableName == tableInfoModel.GetClassName())
			{
				return CheckOrAlterColumnName(sourceColumn, "s");
			}
		}

		public static string CheckOrAlterColumnName(string tableName, string suffix = null)
		{
			var newName = tableName;

			foreach (var unvalidPart in _invalidColumnChars)
			{
				while (newName.Contains(unvalidPart))
				{
					var indexOfElement = newName.IndexOf(unvalidPart.ToString(CultureInfo.InvariantCulture), System.StringComparison.Ordinal) + 1;
					if (indexOfElement < newName.Length)
					{
						var elementAt = newName.ElementAt(indexOfElement);
						if (!_invalidColumnChars.Contains(elementAt))
						{
							var remove = newName.Remove(indexOfElement, 1);
							newName = remove.Insert(indexOfElement, elementAt.ToString(CultureInfo.InvariantCulture).ToUpper());
						}
					}
					newName = newName.Replace(unvalidPart.ToString(CultureInfo.InvariantCulture), string.Empty);
				}
			}

			if (!string.IsNullOrWhiteSpace(suffix))
			{
				if (!newName.EndsWith(suffix))
				{
					newName += suffix;
				}
			}

			return newName;
		}
	}
}
