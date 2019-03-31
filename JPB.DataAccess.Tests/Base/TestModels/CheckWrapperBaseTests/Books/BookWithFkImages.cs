#region

using System.Collections.Generic;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

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

	[ForModel(BookMeta.TableName)]
	public class BookWithFkImages
	{
		[PrimaryKey]
		public int BookId { get; set; }
		public string BookName { get; set; }
		public int? IdUser { get; set; }

		[ForeignKey(nameof(BookId), nameof(ImageWithFkBooks.IdBook))]
		public virtual DbCollection<ImageWithFkBooks> Images { get; set; }

		[ForeignKey(nameof(IdUser), UsersMeta.PrimaryKeyName)]
		public virtual Users User { get; set; }
	}
}