using System;
using System.Text;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData
{
	public class BookMeta : IDatabaseMeta
	{
		public const string TableName = "Book";
		public const string SelectStatement = "SELECT * FROM " + TableName;
		public const string PrimaryKeyName = "BookId";
		public const string ContentName = "BookName";
		public const string ForgeinKeyName = "IdUser";

		public static readonly string CreateMsSql;
		public static readonly string CreateSqLite;
		public static readonly string CreateMySql;

		static BookMeta()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE {TableName} (");
			sb.AppendLine($" {PrimaryKeyName} BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL,");
			sb.AppendLine($" {ContentName} NVARCHAR(MAX),");
			sb.AppendLine($" {ForgeinKeyName} BIGINT NULL,");
			sb.AppendLine(
				$" CONSTRAINT [BookToUser] FOREIGN KEY ({ForgeinKeyName}) REFERENCES {UsersMeta.TableName}({UsersMeta.PrimaryKeyName})");
			sb.AppendLine(");");
			CreateMsSql = sb.ToString();

			sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE {TableName} (");
			sb.AppendLine($" {PrimaryKeyName} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, ");
			sb.AppendLine($" {ContentName} TEXT,");
			sb.AppendLine($" {ForgeinKeyName} INTEGER NULL,");
			sb.AppendLine($" FOREIGN KEY ({ForgeinKeyName}) REFERENCES {UsersMeta.TableName}({UsersMeta.PrimaryKeyName})");

			sb.AppendLine(")");
			CreateSqLite = sb.ToString();

			sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE {TableName} (");
			sb.AppendLine($" {PrimaryKeyName} BIGINT NOT NULL AUTO_INCREMENT,");
			sb.AppendLine($" {ContentName} varchar(350) NULL,");
			sb.AppendLine($" {ForgeinKeyName} INTEGER NULL,");
			sb.AppendLine($" PRIMARY KEY ({PrimaryKeyName}),");
			sb.AppendLine($" FOREIGN KEY ({ForgeinKeyName}) REFERENCES {UsersMeta.TableName}({UsersMeta.PrimaryKeyName})");
			sb.AppendLine(")");
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