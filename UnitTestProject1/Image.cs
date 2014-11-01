using System;
using JPB.DataAccess.ModelsAnotations;

namespace UnitTestProject1
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