/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.EntityCreator.Core.Models;
using JPB.DataAccess.EntityCreator.DatabaseStructure;
using JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts;

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
		public bool WrapNullables { get; set; }

		public TableInfoModel()
		{
			ColumnInfos = new List<IColumInfoModel>();
		}

		public TableInfoModel(ITableInformations info, string database, IStructureAccess db)
		{
			CreateSelectFactory = true;
			Info = info;
			Database = database;
			ColumnInfos = db.GetColumnsOf(Info.TableName, database)
				.Select(s => new ColumnInfoModel(s)).ToList();

			var primaryKeyName = db.GetPrimaryKeyOf(info.TableName, Database);

			var columInfoModel = ColumnInfos.FirstOrDefault(s => s.ColumnInfo.ColumnName == primaryKeyName);
			if (columInfoModel != null)
			{
				columInfoModel.PrimaryKey = true;
			}

			var forgeinKeyDeclarations = db.GetForeignKeys(info.TableName, database);

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