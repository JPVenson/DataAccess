/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using System;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Query;
using JPB.DataAccess.Query.Contracts;
using System.Data;
using System.Xml.Serialization;
using JPB.DataAccess.Manager;
using JPB.DataAccess.QueryBuilder;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	[Serializable]
	public class StoredPrcInfoModel
	{
		public StoredProcedureInformation Parameter { get; set; }
		public bool Exclude { get; set; }
		public string NewTableName { get; set; }

		public StoredPrcInfoModel(StoredProcedureInformation parameter)
		{
			Parameter = parameter;
		}

		public string GetClassName()
		{
			return string.IsNullOrEmpty(NewTableName) ? Parameter.TableName : NewTableName;
		}
	}

	public class DynamicTableContentModel
	{
		[ObjectFactoryMethod]
		public DynamicTableContentModel(IDataRecord record)
		{
			DataHolder = new Dictionary<string, object>();

			for (int i = 0; i < record.FieldCount; i++)
			{
				DataHolder.Add(record.GetName(i), record.GetValue(i));
			}
		}

		[SelectFactoryMethod]
		public static void SelectFromTable(IQueryBuilder<IRootQuery> queryBuilder, string tableName)
		{
			queryBuilder.SelectStar().QueryD(tableName);
		}

		[JPB.DataAccess.ModelsAnotations.LoadNotImplimentedDynamic]
		public IDictionary<string, object> DataHolder { get; set; }
	}

	[Serializable]
	public class TableInfoModel : ITableInfoModel
	{
		public TableInformations Info { get; set; }
		public string Database { get; set; }
		public IEnumerable<IColumInfoModel> ColumnInfos { get; set; }
		public string NewTableName { get; set; }
		public bool Exclude { get; set; }
		public bool CreateFallbackProperty { get; set; }
		public bool CreateSelectFactory { get; set; }
		public bool CreateDataRecordLoader { get; set; }
		public TableInfoModel()
		{

		}

		public TableInfoModel(TableInformations info, string database, DbAccessLayer db)
		{
			CreateSelectFactory = true;
			Info = info;
			Database = database;
			ColumnInfos = db.Select<ColumnInfo>(new object[] { Info.TableName, database }).Select(s => new ColumInfoModel(s)).ToList();

			var firstOrDefault = db.RunPrimetivSelect(typeof(string),
				"SELECT COLUMN_NAME " +
				"FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc " +
				"JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu " +
				"ON tc.CONSTRAINT_NAME = ccu.Constraint_name " +
				"WHERE tc.CONSTRAINT_TYPE = 'Primary Key' " +
				"AND tc.TABLE_CATALOG = @database " +
				"AND tc.TABLE_NAME = @tableName", new List<IQueryParameter>()
				{
					new QueryParameter("tableName",info.TableName),
					new QueryParameter("database",Database)
				}).FirstOrDefault() as string;

			var columInfoModel = ColumnInfos.FirstOrDefault(s => s.ColumnInfo.ColumnName == firstOrDefault);
			if (columInfoModel != null)
				columInfoModel.PrimaryKey = true;


			var forgeinKeyDeclarations = db.Select<ForgeinKeyInfoModel>(new object[] { info.TableName, database });

			foreach (var item in ColumnInfos)
			{
				var fod = forgeinKeyDeclarations.FirstOrDefault(s => s.SourceColumn == item.ColumnInfo.ColumnName);
				if (fod != null)
				{
					item.ForgeinKeyDeclarations = fod;
				}
			}
		}

		public string GetClassName()
		{
			return string.IsNullOrEmpty(NewTableName)
					? Info.TableName
					: NewTableName;
		}
	}

	[Serializable]
	public class ForgeinKeyInfoModel
	{
		public ForgeinKeyInfoModel()
		{

		}

		[SelectFactoryMethod]
		public static void Callup(IQueryBuilder<IRootQuery> builder, string tableName, string database)
		{
			builder.QueryText("SELECT ccu.column_name AS SourceColumn ,kcu.table_name AS TargetTable ,kcu.column_name AS TargetColumn")
				.QueryText("FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu")
				.QueryText("INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc")
				.QueryText("ON ccu.CONSTRAINT_NAME = rc.CONSTRAINT_NAME")
				.QueryText("INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu")
				.QueryText("ON kcu.CONSTRAINT_NAME = rc.UNIQUE_CONSTRAINT_NAME")
				.QueryD("WHERE ccu.TABLE_NAME = @tableName AND ccu.TABLE_CATALOG = @database", new
				{
					database = database,
					tableName = tableName
				})
				.QueryText("ORDER BY ccu.table_name");
		}

		[ForModel("TargetTable")]
		public string TableName { get; set; }

		[ForModel("SourceColumn")]
		public string SourceColumn { get; set; }

		[ForModel("TargetColumn")]
		public string TargetColumn { get; set; }

		public override string ToString()
		{
			return string.Format("Column '{0}' references column '{1}' on table '{2}'", SourceColumn, TargetColumn, TableName);
		}
	}

	[Serializable]
	public class EnumDeclarationModel
	{
		public EnumDeclarationModel()
		{
			Values = new Dictionary<int, string>();
		}
		public Dictionary<int, string> Values { get; private set; }
		public string Name { get; set; }
	}

	[Serializable]
	public class ColumInfoModel : IColumInfoModel
	{
		public ColumInfoModel()
		{

		}

		public ColumInfoModel(ColumnInfo columnInfo)
		{
			ColumnInfo = columnInfo;
			if (columnInfo.TargetType2.ToLower() == "timestamp")
			{
				IsRowVersion = true;
			}
		}

		public ColumnInfo ColumnInfo { get; set; }
		public string NewColumnName { get; set; }
		public bool IsRowVersion { get; set; }
		public bool PrimaryKey { get; set; }
		public bool InsertIgnore { get; set; }
		public EnumDeclarationModel EnumDeclaration { get; set; }
		public bool Exclude { get; set; }
		public ForgeinKeyInfoModel ForgeinKeyDeclarations { get; set; }

		public string GetPropertyName()
		{
			return string.IsNullOrEmpty(NewColumnName) ? ColumnInfo.ColumnName : NewColumnName;
		}
	}
}