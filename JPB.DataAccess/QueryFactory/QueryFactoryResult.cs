using System.Collections.Generic;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryFactory
{
    /// <summary>
    /// Wraps a query and its Paramters into one single Object
    /// </summary>
    public class QueryFactoryResult : IQueryFactoryResult
    {
        public QueryFactoryResult(QueryBuilder.QueryBuilder builder)
        {
            var compileFlat = builder.CompileFlat();
            Query = compileFlat.Item1;
            this.Parameters = compileFlat.Item2;
        }

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