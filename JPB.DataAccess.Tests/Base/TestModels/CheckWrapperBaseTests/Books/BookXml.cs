using System.Collections.Generic;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Images;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Books
{
	public class BookXml
	{
		[PrimaryKey]
		public int BookId { get; set; }

		public string Text { get; set; }

		[FromXml(nameof(Images))]
		public IEnumerable<Image> Images { get; set; }

		[SelectFactoryMethod(TargetDatabase = DbAccessType.MsSql)]
		public static IQueryFactoryResult SelectStatement()
		{
			return new QueryFactoryResult($"SELECT *," +
										  $"(SELECT * FROM [{ImageMeta.TableName}] WHERE [{ImageMeta.TableName}].[{ImageMeta.ForgeinKeyName}] =" +
										  $" [{BookMeta.TableName}].[{BookMeta.PrimaryKeyName}] FOR XML AUTO, ROOT('ArrayOfImage')) AS [{nameof(Images)}]" +
										  $" FROM [Book]");
		}
	}
}
