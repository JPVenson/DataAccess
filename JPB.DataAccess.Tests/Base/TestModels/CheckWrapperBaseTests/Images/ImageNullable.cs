using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
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