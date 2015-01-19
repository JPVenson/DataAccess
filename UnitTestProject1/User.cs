using System.Data;
using JPB.DataAccess.ModelsAnotations;

namespace UnitTestProject1
{
    [ForModel("Users")]
    [SelectFactory("SELECT * FROM Users")]
    public class User
    {
        [ForModel("UserName")]
        public string Name { get; set; }

        [PrimaryKey]
        [ForModel("User_ID")]
        public long UserId { get; set; }
    }

    public class UserImpl : User
    {
        public UserImpl(IDataRecord rec)
        {
            Name = (string)rec["UserName"];
            UserId = (long)rec["User_ID"];
        }
    }
}