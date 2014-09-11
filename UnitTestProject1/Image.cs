using System;
using JPB.DataAccess.ModelsAnotations;
using testing.Annotations;

namespace testing
{
    [Serializable]
    [ForModel("Images")]
    [SelectFactory("SELECT * FROM Images")]
    public class Image
    {
        [PrimaryKey]
        [ForModel("Image_ID")]
        public long Id { get; set; }

        [ForModel("Content")]
        public string Text { get; set; }
    }
}