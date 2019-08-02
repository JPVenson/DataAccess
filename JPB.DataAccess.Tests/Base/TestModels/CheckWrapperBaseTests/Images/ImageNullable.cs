using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Books;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Images
{
	[ForModel("Image")]
	public class ImageNullable
	{
		[PrimaryKey]
		public long ImageId { get; set; }

		public string Text { get; set; }

		[ForeignKeyDeclaration("BookId", typeof(Book))]
		public int? IdBook { get; set; }
	}
}