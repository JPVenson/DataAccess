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
        public User()
        {
            
        }

        [ForModel("UserName")]
        public string Name { get; set; }

        public long? ID_Image { get; set; }

        [LoadNotImplimentedDynamic]
        public IDictionary<string, object> UnresolvedObjects { set; get; }

        [PrimaryKey]
        [ForModel("User_ID")]
        public long UserId { get; set; }

        [SelectFactoryMehtod]
        public static IQueryFactoryResult CreateQuery()
        {
            return new QueryFactoryResult("SELECT * FROM Users");
        }

        [UpdateFactoryMethod()]
        public string UpdateQuery()
        {
            return string.Empty;
        }
    }
}