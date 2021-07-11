using System.Linq;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Poco;
using JPB.DataAccess.EntityCreator.Core.Poco.MsSQL;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	public class DatabaseMsSqlStructure :
		IDatabaseStructure
	{
		private readonly DbAccessLayer _db;

		public DatabaseMsSqlStructure(DbAccessLayer db)
		{
			_db = db;
		}

		public string GetVersion()
		{
			return _db.RunSelect<string>(_db.Database.CreateCommand("SELECT SERVERPROPERTY('productversion')"))
				.FirstOrDefault();
		}

		public ITableInformations[] GetTables()
		{
			return _db.Select<TableInformations>();
		}

		public ITableInformations[] GetViews()
		{
			return _db.Select<ViewInformation>();
		}

		public IStoredProcedureInformation[] GetStoredProcedures()
		{
			return _db.Select<StoredProcedureInformation>();
		}

		public Any[] GetEnumValuesOfType(string tableName)
		{
			return _db.Select<Any>(new[] {tableName});
		}

		public string GetDatabaseName()
		{
			return _db.Database.DatabaseName;
		}

		public IColumnInfo[] GetColumnsOf(string table, string database)
		{
			return _db.Select<ColumnInfo>(new object[]
			{
				table, database
			});
		}

		public string GetPrimaryKeyOf(string table, string database)
		{
			return _db.RunSelect(typeof(string),
				_db.Database.CreateCommandWithParameterValues("SELECT COLUMN_NAME " +
				                                              "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc " +
				                                              "JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu " +
				                                              "ON tc.CONSTRAINT_NAME = ccu.Constraint_name " +
				                                              "WHERE tc.CONSTRAINT_TYPE = 'Primary Key' " +
				                                              "AND tc.TABLE_CATALOG = @database " +
				                                              "AND tc.TABLE_NAME = @tableName",
					new QueryParameter("tableName", table),
					new QueryParameter("database", database)
				)).FirstOrDefault() as string;
		}

		public IForgeinKeyInfoModel[] GetForeignKeys(string table, string database)
		{
			return _db.Select<ForgeinKeyInfoModel>(new object[] {table, database});
		}
	}
}