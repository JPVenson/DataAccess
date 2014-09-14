﻿using System.Collections.Generic;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace testing
{
    [ForModel("Users")]
    public class User
    {
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

        [SelectFactoryMehtod]
        public static IQueryFactoryResult CreateQuery(string testParam)
        {
            return new QueryFactoryResult("SELECT * FROM Users");
        }

        [UpdateFactoryMethod]
        public string UpdateQuery()
        {
            return string.Empty;
        }
    }
}