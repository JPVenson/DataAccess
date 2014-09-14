using System.Collections.Generic;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryFactory
{
    public class QueryFactoryResult : IQueryFactoryResult
    {
        public QueryFactoryResult(string query)
        {
            Query = query;
        }

        public QueryFactoryResult(string query, params IQueryParameter[] parameters) 
            : this(query)
        {
            Parameters = parameters;
        }

        public string Query { get; private set; }
        public IEnumerable<IQueryParameter> Parameters { get; private set; }
    }
}