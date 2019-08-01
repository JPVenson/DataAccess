

using JPB.DataAccess.Framework.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Books
{
	public class Book
	{
		[PrimaryKey]
		public int BookId { get; set; }

		public string Text { get; set; }

		[ForeignKeyDeclaration(typeof(User.Users))]
		public int? IdUser { get; set; }
	}
}