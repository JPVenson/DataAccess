using System.Collections.Generic;
using JPB.DataAccess.ModelsAnotations;

namespace testing
{
    [ForModel("Users")]
    [SelectFactory("SELECT * FROM Users")]
    class PocoUsers
    {
        [LoadNotImplimentedDynamic]
        public IDictionary<string, object> UnresolvedObjects { set; get; }
    }
}