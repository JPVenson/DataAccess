using JPB.DataAccess.Helper.LocalDb;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.UnitTests.TestModels.CheckWrapperBaseTests
{
	public class Image
	{
		[PrimaryKey]
		public long ImageId { get; set; }

		public string Text { get; set; }

		[ForeignKeyDeclaration("BookId", typeof(Book))]
		public int IdBook { get; set; }
	}

	public class Book
	{
		[PrimaryKey]
		public int BookId { get; set; }

		public string BookName { get; set; }
	}
}