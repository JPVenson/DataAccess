using System.Collections.Generic;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryFactory
{
    public interface IQueryFactoryResult
    {
        string Query { get; } 
        IEnumerable<IQueryParameter> Parameters { get; } 
    }
}