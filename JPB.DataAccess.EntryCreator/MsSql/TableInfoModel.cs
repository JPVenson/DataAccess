using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.EntityCreator.MsSql
{
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

    public class TableInfoModel
    {
        public TableInformations Info { get; set; }
        public string Database { get; set; }
        public List<ColumInfoModel> ColumnInfos { get; set; }

        public string NewTableName { get; set; }
        public bool Exclude { get; set; }
        public bool CreateFallbackProperty { get; set; }

        public bool CreateSelectFactory { get; set; }
        public bool CreateDataRecordLoader { get; set; }
        public string NewNamespace { get; set; }

        public TableInfoModel(TableInformations info, string database)
        {
            CreateSelectFactory = true;
            Info = info;
            Database = database;
            ColumnInfos = MsSqlCreator.Manager.Select<ColumnInfo>(new object[] { Info.TableName }).Select(s => new ColumInfoModel(s)).ToList();

            var firstOrDefault = MsSqlCreator.Manager.RunPrimetivSelect(typeof(string),
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
        }

        public string GetClassName()
        {
            return string.IsNullOrEmpty(NewTableName)
                    ? Info.TableName
                    : NewTableName;
        }
    }

    public class ColumInfoModel
    {
        public ColumInfoModel(ColumnInfo columnInfo)
        {
            ColumnInfo = columnInfo;
        }

        public ColumnInfo ColumnInfo { get; set; }
        public string NewColumnName { get; set; }

        public bool PrimaryKey { get; set; }

        public string GetPropertyName()
        {
            return string.IsNullOrEmpty(NewColumnName) ? ColumnInfo.ColumnName : NewColumnName;
        }
    }
}