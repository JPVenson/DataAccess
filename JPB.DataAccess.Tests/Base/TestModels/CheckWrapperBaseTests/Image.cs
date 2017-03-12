using JPB.DataAccess.ModelsAnotations;

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

    [ForModel("Image")]
    public class ImageNullable
    {
        [PrimaryKey]
        public long ImageId { get; set; }

        public string Text { get; set; }

        [ForeignKeyDeclaration("BookId", typeof(Book))]
        public int? IdBook { get; set; }
    }

    public class Book
    {
        [PrimaryKey]
        public int BookId { get; set; }

        public string BookName { get; set; }
    }
}