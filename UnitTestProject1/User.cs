using System;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace testing
{
    [ForModel("Users")]
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