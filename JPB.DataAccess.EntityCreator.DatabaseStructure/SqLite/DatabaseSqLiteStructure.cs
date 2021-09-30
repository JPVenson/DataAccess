using System.Linq;
using JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts;
using JPB.DataAccess.EntityCreator.DatabaseStructure.SqLite.Models;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure.SqLite
{
	public class DatabaseSqLiteStructure : IDatabaseStructure
	{
		private readonly DbAccessLayer _db;

		public DatabaseSqLiteStructure(DbAccessLayer db)
		{
			_db = db;
		}

		public IColumnInfo[] GetColumnsOf(string table, string database)
		{
			return _db.Select<ColumnInfo>(new object[]
			{
				table
			});
		}

		public string GetPrimaryKeyOf(string table, string database)
		{
			return _db.Select<ColumnInfo>(new object[]
			{
				table
			}).FirstOrDefault(e => e.IsPrimaryKey)?.ColumnName;
		}

		public IForgeinKeyInfoModel[] GetForeignKeys(string table, string database)
		{
			return _db.Select<ForgeinKeyInfoModel>(new object[]
			{
				table
			});
		}

		public string GetVersion()
		{
			return "";
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
			return new IStoredProcedureInformation[0];
		}

		public Any[] GetEnumValuesOfType(string tableName)
		{
			return _db.Select<Any>(new[] {tableName});
		}

		public string GetDatabaseName()
		{
			return "";
		}
	}
}
