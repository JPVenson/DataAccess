#region

using JPB.DataAccess.Framework.DbCollection;
using JPB.DataAccess.Framework.ModelsAnotations;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Images;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.MetaData;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests.Books
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
		public virtual User.Users User { get; set; }

		[ForeignKey(nameof(IdUser), UsersMeta.PrimaryKeyName)]
		public virtual User.Users User1 { get; set; }
	}
}