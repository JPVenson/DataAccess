using System.Collections.Generic;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryFactory
{
    public class QueryFactoryResult : IQueryFactoryResult
    {
        public QueryFactoryResult(string query, params IQueryParameter[] parameters)
        {
            Parameters = parameters;
            Query = query;
        }

        public string Query { get; private set; }
        public IEnumerable<IQueryParameter> Parameters { get; private set; }
    }
}