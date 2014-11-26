using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JPB.DataAccess.QueryBuilder
{
    public class QueryEagerEnumerator : IEnumerator
    {
        private readonly QueryBuilder _queryBuilder;
        private readonly Type _type;
        List<IDataRecord> enumerateDataRecords;
        private ArrayList elements;
        private int counter;
        Task _task;

        public QueryEagerEnumerator(QueryBuilder queryBuilder, Type type)
        {
            _queryBuilder = queryBuilder;
            _type = type;
            elements = new ArrayList();
            counter = 0;
            Load();
        }

        /// <summary>
        /// Mehtod for async loading this will bring us some m secs
        /// </summary>
        private void Load()
        {
            _task = new Task(() =>
            {
                var query = _queryBuilder.Compile();
                enumerateDataRecords = _queryBuilder.Database.EnumerateDataRecords(query);
            });
            _task.Start();
        }

        public bool MoveNext()
        {
            _task.Wait();

            counter++;
            try
            {
                if (elements.Count >= counter)
                {
                    Current = elements[counter];
                    return true;
                }

                if (enumerateDataRecords.Count > counter)
                    return false;

                var dataRecord = enumerateDataRecords.ElementAt(counter);
                Current = DataConverterExtensions.SetPropertysViaReflection(_type, dataRecord);
                elements.Add(Current);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Reset()
        {
            counter = 0;
        }

        public object Current { get; private set; }
    }
}