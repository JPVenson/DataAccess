using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.QueryBuilder
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryLazyEnumerator : IEnumerator, IDisposable
    {
        private readonly QueryBuilder _queryBuilder;
        private readonly Type _type;
        private IDataReader executeReader;
        private Task _loadingTask;
        private DbAccessLayer _accessLayer;

        public QueryLazyEnumerator(QueryBuilder queryBuilder, Type type)
        {
            _queryBuilder = queryBuilder;
            _type = type;
            _accessLayer = new DbAccessLayer(queryBuilder.Database);
            _accessLayer.Database.Connect(IsolationLevel.ReadCommitted);
            executeReader = _queryBuilder.Compile().ExecuteReader();
        }

        public void Dispose()
        {
            _accessLayer.Database.CloseConnection();
        }
        
        public bool MoveNext()
        {
            if (executeReader.IsClosed)
            {
                Dispose();
                return false;
            }

            if (executeReader.Read())
                return true;
            return false;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public object Current
        {
            get
            {
                return DataConverterExtensions.SetPropertysViaReflection(_type, executeReader);
            }
        }
    }

    public class QueryLazyEnumerator<T> : QueryLazyEnumerator, IEnumerator<T>
    {
        public QueryLazyEnumerator(QueryBuilder queryBuilder, Type type) : base(queryBuilder, type)
        {
        }

        public void Dispose()
        {
            
        }

        public new T Current { get { return (T) base.Current; } }
    }
}