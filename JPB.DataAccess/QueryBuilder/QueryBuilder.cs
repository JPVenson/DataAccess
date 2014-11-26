using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.QueryProvider;

namespace JPB.DataAccess.QueryBuilder
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryBuilder : IEnumerable
    {
        internal readonly IDatabase Database;
        private readonly Type _forType;

        /// <summary>
        /// Defines the Way how the Data will be loaded
        /// </summary>
        public EnumerationMode EnumerationMode { get; set; }

        public QueryBuilder(IDatabase database, Type forType)
            : this(database)
        {
            _forType = forType;
        }

        public QueryBuilder(IDatabase database)
        {
            this.Database = database;
            Parts = new ObservableCollection<QueryPart>();
        }

        internal ObservableCollection<QueryPart> Parts { get; set; }

        /// <summary>
        /// Will concat all QueryParts into a statement and will check for Spaces
        /// </summary>
        /// <returns></returns>
        public IDbCommand Compile()
        {
            var sb = new StringBuilder();
            var queryParts = this.Cast<QueryPart>().ToArray();
            QueryPart pref = null;

            foreach (var queryPart in queryParts)
            {
                //take care of spaces
                //check if the last statement ends with a space or the next will start with one
                if (pref != null)
                {
                    if (!pref.Prefix.EndsWith(" ", true, CultureInfo.InvariantCulture) && queryPart.Prefix.StartsWith(" ", true, CultureInfo.InvariantCulture))
                    {
                        queryPart.Prefix = " " + queryPart.Prefix;
                    }
                }
                sb.Append(queryPart.Prefix);
                pref = queryPart;
            }
            return Database.CreateCommandWithParameterValues(sb.ToString(), queryParts.SelectMany(s => s.QueryParameters));
        }

        public IEnumerator GetEnumerator()
        {
            if (_forType == null)
                throw new ArgumentNullException("No type Supplied", new Exception());

            if (EnumerationMode == EnumerationMode.FullOnLoad)
                return new QueryEagerEnumerator(this, _forType);
            return new QueryLazyEnumerator(this, _forType);
        }

        /// <summary>
        /// Executes a query without result parsing
        /// </summary>
        public int Execute()
        {
            return this.Compile().ExecuteGenericCommand(Database);
        }

        /// <summary>
        /// Adds a Query part to the Local collection
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public QueryBuilder Add(QueryPart part)
        {
            Parts.Add(part);
            return this;
        }
    }

    public class QueryBuilder<T> : QueryBuilder
    {
        public QueryBuilder(IDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
