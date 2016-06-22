using System.Text;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	public static class UsersMeta
	{
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
		}

		public const string TableName = "Users";
		public const string SelectStatement = "SELECT * FROM " + TableName;
		public const string PrimaryKeyName = "User_ID";
		public const string ContentName = "UserName";
		public static readonly string CreateMsSql;
		public static readonly string CreateSqLite;

	}

	public class ImageMeta
	{
		static ImageMeta()
		{
			var sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE {0} (", TableName));
			sb.AppendLine(string.Format(" {0} BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL,", PrimaryKeyName));
			sb.AppendLine(string.Format(" {0} NVARCHAR(MAX),", ContentName));
			sb.AppendLine(string.Format(" {0} INT NOT NULL,", ForgeinKeyName));
			sb.AppendLine(string.Format(" CONSTRAINT [ImageToBook] FOREIGN KEY ({0}) REFERENCES {1}({2})", ForgeinKeyName, BookMeta.TableName, BookMeta.PrimaryKeyName));
			sb.AppendLine(");");
			CreateMsSQl = sb.ToString();

			sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE {0} (", TableName));
			sb.AppendLine(string.Format(" {0} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,", PrimaryKeyName));
			sb.AppendLine(string.Format(" {0} TEXT,", ContentName));
			sb.AppendLine(string.Format(" {0} INTEGER NOT NULL,", ForgeinKeyName));
			sb.AppendLine(string.Format(" FOREIGN KEY ({0}) REFERENCES {1}({2})", ForgeinKeyName, BookMeta.TableName, BookMeta.PrimaryKeyName));
			sb.AppendLine(");");
			CreateSqLite = sb.ToString();
		}

		public static readonly string CreateMsSQl;
		public static readonly string CreateSqLite;

		public const string TableName = "Image";
		public const string SelectStatement = "SELECT * FROM " + TableName;
		public const string PrimaryKeyName = "ImageId";
		public const string ContentName = "Text";
		public const string ForgeinKeyName = "IdBook";
	}

	public class BookMeta
	{
		static BookMeta()
		{
			var sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE {0} (", TableName));
			sb.AppendLine(string.Format(" {0} INT PRIMARY KEY IDENTITY(1,1) NOT NULL,", PrimaryKeyName));
			sb.AppendLine(string.Format(" {0} NVARCHAR(MAX)", ContentName));
			sb.AppendLine(");");
			CreateMsSQl = sb.ToString();

			sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE {0}", TableName));
			sb.AppendLine(string.Format("({0} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, ", PrimaryKeyName));
			sb.AppendLine(string.Format("{0} TEXT", ContentName));
			sb.AppendLine(")");
			CreateSqLite = sb.ToString();
		}

		public static readonly string CreateMsSQl;
		public static readonly string CreateSqLite;

		public const string TableName = "Book";
		public const string SelectStatement = "SELECT * FROM " + TableName;
		public const string PrimaryKeyName = "BookId";
		public const string ContentName = "BookName";
	}
}