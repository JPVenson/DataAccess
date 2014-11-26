using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JPB.DataAccess.QueryBuilder
{
    public class QueryLazyEnumerator : IEnumerator
    {
        private readonly QueryBuilder _queryBuilder;
        private readonly Type _type;
        private IDataReader executeReader;
        private Task _loadingTask;

        public QueryLazyEnumerator(QueryBuilder queryBuilder, Type type)
        {
            _queryBuilder = queryBuilder;
            _type = type;
            Init();
        }

        private void Init()
        {
            _loadingTask = new Task(() =>
            {
                executeReader = _queryBuilder.Compile().ExecuteReader();
            });
            _loadingTask.Start();
        }

        public bool MoveNext()
        {
            _loadingTask.Wait();
            return executeReader.Read();
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
}