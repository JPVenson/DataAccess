#region

using System;
using System.Text;
using JPB.DataAccess.Manager;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData
{
	public class UsersMeta : IDatabaseMeta
	{
		public const string TableName = "Users";
		public const string SelectStatement = "SELECT * FROM " + TableName;
		public const string SelectPrimaryKeyStatement = "SELECT " + PrimaryKeyName + " FROM " + TableName;
		public const string PrimaryKeyName = "User_ID";
		public const string ContentName = "UserName";
		public static readonly string CreateMsSql;
		public static readonly string CreateSqLite;
		public static readonly string CreateMySql;

		static UsersMeta()
		{
			var sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE {0} (", TableName));
			sb.AppendLine(string.Format(" {0} BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL,", PrimaryKeyName));
			sb.AppendLine(string.Format(" {0} NVARCHAR(MAX)", ContentName));
			sb.AppendLine(");");
			CreateMsSql = sb.ToString();

			sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE {0}", TableName));
			sb.AppendLine(string.Format("({0} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, ", PrimaryKeyName));
			sb.AppendLine(string.Format("{0} TEXT", ContentName));
			sb.AppendLine(")");
			CreateSqLite = sb.ToString();

			sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE {0} (", TableName));
			sb.AppendLine(string.Format("{0} BIGINT NOT NULL AUTO_INCREMENT,", PrimaryKeyName));
			sb.AppendLine(string.Format("{0} VARCHAR(350) NULL,", ContentName));
			sb.AppendLine(string.Format("PRIMARY KEY ({0})", PrimaryKeyName));
			sb.AppendLine(")");
			sb.AppendLine(";");
			CreateMySql = sb.ToString();
		}

		/// <inheritdoc />
		public string CreationCommand(DbAccessType accessType)
		{
			switch (accessType)
			{
				case DbAccessType.MsSql:
					return CreateMsSql;
				case DbAccessType.MySql:
					return CreateMySql;
				case DbAccessType.SqLite:
					return CreateSqLite;
				default:
					throw new ArgumentOutOfRangeException(nameof(accessType), accessType, null);
			}
		}
	}
}