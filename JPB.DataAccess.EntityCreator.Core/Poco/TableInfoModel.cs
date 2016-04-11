/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.DataAccess.EntityCreator.MsSql;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.EntityCreator.Core.Poco
{
	[Serializable]
	public class TableInfoModel : ITableInfoModel
	{
		public ITableInformations Info { get; set; }
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
}