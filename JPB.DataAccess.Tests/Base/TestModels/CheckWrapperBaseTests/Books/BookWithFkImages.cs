#region

using System.Collections.Generic;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[ForModel(BookMeta.TableName)]
	public class BookWithFkImages
	{
		[PrimaryKey]
		public int BookId { get; set; }
		public string Text { get; set; }
		public int? IdUser { get; set; }

		[ForeignKey(nameof(BookId), nameof(ImageWithFkBooks.IdBook))]
		public virtual DbCollection<ImageWithFkBooks> Images { get; set; }

		[ForeignKey(nameof(IdUser), UsersMeta.PrimaryKeyName)]
		public virtual Users User { get; set; }

		[ForeignKey(nameof(IdUser), UsersMeta.PrimaryKeyName)]
		public virtual Users User1 { get; set; }
	}
}