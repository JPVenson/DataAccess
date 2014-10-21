using System;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.EntryCreator.MsSql
{
    public class ColumnInfo
    {
        [SelectFactoryMehtod()]
        public static IQueryFactoryResult SelectColumns(string tableName)
        {
            return new QueryFactoryResult("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName AND TABLE_CATALOG = @database", new[]
            {
                new QueryParameter("@tableName", tableName),
                new QueryParameter("@database", MsSqlCreator.Manager.Database.DatabaseName)
            });
        }

        [ForModel("COLUMN_NAME")]
        public string ColumnName { get; set; }

        [ForModel("ORDINAL_POSITION")]
        public int PositionFromTop { get; set; }

        [ForModel("IS_NULLABLE")]
        [ValueConverter(typeof(NoYesConverter))]
        public bool Nullable { get; set; }

        [ForModel("DATA_TYPE")]
        [ValueConverter(typeof(DbTypeToCsType))]
        public Type TargetType { get; set; }
    }
}