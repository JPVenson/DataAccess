using System.Collections.Generic;
using JPB.DataAccess.ModelsAnotations;

namespace UnitTestProject1
{
    [ForModel("Users")]
    [SelectFactory("SELECT * FROM Users")]
    internal class PocoUsers
    {
        [LoadNotImplimentedDynamic]
        public IDictionary<string, object> UnresolvedObjects { set; get; }
    }
}