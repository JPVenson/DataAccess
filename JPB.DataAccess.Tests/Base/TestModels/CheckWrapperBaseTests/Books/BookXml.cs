using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	public class BookXml
	{
		[PrimaryKey]
		public int BookId { get; set; }

		public string BookName { get; set; }

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
