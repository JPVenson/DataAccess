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
			sb.AppendLine($"CREATE TABLE {TableName} (");
			sb.AppendLine($" {PrimaryKeyName} BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL,");
			sb.AppendLine($" {ContentName} NVARCHAR(MAX)");
			sb.AppendLine(");");
			CreateMsSql = sb.ToString();

			sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE {TableName}");
			sb.AppendLine($"({PrimaryKeyName} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, ");
			sb.AppendLine($"{ContentName} TEXT");
			sb.AppendLine(")");
			CreateSqLite = sb.ToString();

			sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE {TableName} (");
			sb.AppendLine($"{PrimaryKeyName} BIGINT NOT NULL AUTO_INCREMENT,");
			sb.AppendLine($"{ContentName} VARCHAR(350) NULL,");
			sb.AppendLine($"PRIMARY KEY ({PrimaryKeyName})");
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