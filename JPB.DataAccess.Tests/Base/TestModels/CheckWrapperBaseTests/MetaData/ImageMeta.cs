using System;
using System.Text;
using JPB.DataAccess.Framework.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Images;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData
{
	public class ImageMeta : IDatabaseMeta
	{
		public const string TableName = nameof(Image);
		public const string SelectStatement = "SELECT * FROM " + TableName;
		public const string PrimaryKeyName = nameof(Image.ImageId);
		public const string ContentName = nameof(Image.Text);
		public const string ForgeinKeyName = nameof(Image.IdBook);

		public static readonly string CreateMsSql;
		public static readonly string CreateSqLite;
		public static readonly string CreateMySql;

		static ImageMeta()
		{
			var sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE {0} (", TableName));
			sb.AppendLine(string.Format(" {0} BIGINT PRIMARY KEY IDENTITY(1,1) NOT NULL,", PrimaryKeyName));
			sb.AppendLine(string.Format(" {0} NVARCHAR(MAX),", ContentName));
			sb.AppendLine(string.Format(" {0} BIGINT NULL,", ForgeinKeyName));
			sb.AppendLine(string.Format(" CONSTRAINT [ImageToBook] FOREIGN KEY ({0}) REFERENCES {1}({2})",
			ForgeinKeyName, BookMeta.TableName, BookMeta.PrimaryKeyName));
			sb.AppendLine(");");
			CreateMsSql = sb.ToString();

			sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE {0} (", TableName));
			sb.AppendLine(string.Format(" {0} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,", PrimaryKeyName));
			sb.AppendLine(string.Format(" {0} TEXT,", ContentName));
			sb.AppendLine(string.Format(" {0} INTEGER NULL,", ForgeinKeyName));
			sb.AppendLine(string.Format(" FOREIGN KEY ({0}) REFERENCES {1}({2})", ForgeinKeyName, BookMeta.TableName,
			BookMeta.PrimaryKeyName));
			sb.AppendLine(");");
			CreateSqLite = sb.ToString();

			sb = new StringBuilder();
			sb.AppendLine(string.Format("CREATE TABLE {0} (", TableName));
			sb.AppendLine(string.Format(" {0} BIGINT NOT NULL AUTO_INCREMENT,", PrimaryKeyName));
			sb.AppendLine(string.Format(" {0} text NOT NULL,", ContentName));
			sb.AppendLine(string.Format(" {0} BIGINT NULL,", ForgeinKeyName));
			sb.AppendLine(string.Format(" PRIMARY KEY ({0}),", PrimaryKeyName));
			sb.AppendLine(string.Format(" FOREIGN KEY ({0}) REFERENCES {1}({2})", ForgeinKeyName, BookMeta.TableName,
			BookMeta.PrimaryKeyName));
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