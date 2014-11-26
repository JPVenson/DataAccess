using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryBuilder
{
    public class QueryPart : ICloneable
    {
        public static QueryPart FromCommand(IDbCommand command)
        {
            return new QueryPart(command.CommandText, command.Parameters.Cast<IDataParameter>().Select(s => new QueryParameter(s.ParameterName, s.Value)));
        }

        public QueryPart(string prefix, IEnumerable<IQueryParameter> parameters)
        {
            Prefix = prefix;
            QueryParameters = parameters;
        }

        public QueryPart(string prefix)
        {
            Prefix = prefix;
            QueryParameters = new IQueryParameter[0];
        }

        public string Prefix { get; internal set; }
        public IEnumerable<IQueryParameter> QueryParameters { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}