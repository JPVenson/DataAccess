using System;
using System.Collections.Generic;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryBuilder
{
    /// <summary>
    /// 
    /// </summary>
    public class CteQueryPart : GenericQueryPart
    {
        private readonly Action<QueryBuilder> _cteAction;
        private readonly QueryBuilder _nestedQueryPart;

        public CteQueryPart(string prefix, IEnumerable<IQueryParameter> parameters)
            : base(prefix, parameters)
        {
            throw new Exception();
        }

        public CteQueryPart(string prefix)
            : base(prefix)
        {
            throw new Exception();
        }
    }
}