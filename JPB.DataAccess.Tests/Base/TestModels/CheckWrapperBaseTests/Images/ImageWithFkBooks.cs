using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[ForModel(ImageMeta.TableName)]
	public class ImageWithFkBooks
	{
		[PrimaryKey]
		public long ImageId { get; set; }

		public string Text { get; set; }

		public int IdBook { get; set; }

		[ForeignKey(nameof(IdBook), nameof(BookWithFkImages.BookId))]
		public virtual BookWithFkImages Book { get; set; }
	}
}