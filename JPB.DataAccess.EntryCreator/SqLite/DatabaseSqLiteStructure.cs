using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.EntityCreator.Core.Poco;
using JPB.DataAccess.EntityCreator.Core.Poco.SqLite;
using JPB.DataAccess.EntityCreator.MsSql;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.EntityCreator.SqLite
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
