#region

using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	public class Image
	{
		[PrimaryKey]
		public long ImageId { get; set; }

		public string Text { get; set; }

		[ForeignKeyDeclaration("BookId", typeof(Book))]
		public int IdBook { get; set; }
	}
}