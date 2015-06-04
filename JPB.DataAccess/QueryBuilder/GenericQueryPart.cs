using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryBuilder
{
    /// <summary>
    /// Wrapper for Generic Query parts
    /// </summary>
    public class GenericQueryPart : ICloneable
    {
        public static GenericQueryPart FromCommand(IDbCommand command)
        {
            return new GenericQueryPart(command.CommandText, command.Parameters.Cast<IDataParameter>().Select(s => new QueryParameter(s.ParameterName, s.Value)));
        }

        public GenericQueryPart(string prefix, IEnumerable<IQueryParameter> parameters)
        {
            Debug.Assert(prefix != null, "prefix != null");
            Prefix = prefix;
            QueryParameters = parameters;
        }

        public GenericQueryPart(string prefix)
        {
            Debug.Assert(prefix != null, "prefix != null");
            Prefix = prefix;
            QueryParameters = new IQueryParameter[0];
        }

        public virtual string Render()
        {
            return Prefix;
        }

        public string Prefix { get; internal set; }
        public IEnumerable<IQueryParameter> QueryParameters { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}