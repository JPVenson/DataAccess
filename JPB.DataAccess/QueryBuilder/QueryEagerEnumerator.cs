using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JPB.DataAccess.Config;
using JPB.DataAccess.DbInfoConfig;

namespace JPB.DataAccess.QueryBuilder
{
	internal class QueryEagerEnumerator : IEnumerator
	{
		private readonly ArrayList _elements;
		private readonly QueryBuilder _queryBuilder;
		private readonly Type _type;
		private int _counter;
		private List<IDataRecord> _enumerateDataRecords;
		private Task _task;

		internal QueryEagerEnumerator(QueryBuilder queryBuilder, Type type)
		{
			_queryBuilder = queryBuilder;
			_type = type;
			_elements = new ArrayList();
			_counter = 0;
			Load();
		}

		public bool MoveNext()
		{
			_task.Wait();

			try
			{
				_counter++;

				if (_elements.Count >= _counter)
				{
					Current = _elements[_counter];
					return true;
				}

				if (_enumerateDataRecords.Count < _counter)
					return false;

				var dataRecord = _enumerateDataRecords.ElementAt(_counter - 1);
				Current = _type.GetClassInfo().SetPropertysViaReflection(dataRecord);
				_elements.Add(Current);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public void Reset()
		{
			_counter = 0;
		}

		public object Current { get; private set; }

		/// <summary>
		///     Mehtod for async loading this will bring us some m secs
		/// </summary>
		private void Load()
		{
			_task = new Task(() =>
			{
				var query = _queryBuilder.Compile();
				_enumerateDataRecords = _queryBuilder.Database.EnumerateDataRecords(query, true);
			});
			_task.Start();
		}
	}

	internal class QueryEagerEnumerator<T> : QueryEagerEnumerator, IEnumerator<T>
	{
		internal QueryEagerEnumerator(QueryBuilder queryBuilder, Type type)
			: base(queryBuilder, type)
		{
		}

		public void Dispose()
		{
		}

		public new T Current
		{
			get { return (T) base.Current; }
		}
	}
}