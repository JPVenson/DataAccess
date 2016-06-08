using System;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.TestModels.CheckWrapperBaseTests
{
	public class Image
	{
		[PrimaryKey]
		public long ImageId { get; set; }

		public string Text { get; set; }

		[ForeignKeyDeclaration("BookId", typeof(Book))]
		public int IdBook { get; set; }
	}

	public class ImageNullable
	{
		[PrimaryKey]
		public long ImageId { get; set; }

		public string Text { get; set; }

		[ForeignKeyDeclaration("BookId", typeof(Book))]
		public Nullable<int> IdBook { get; set; }
	}

	public class Book
	{
		[PrimaryKey]
		public int BookId { get; set; }

		public string BookName { get; set; }
	}
}