#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query
{
	internal class QueryEagerEnumerator : IEnumerator, IDisposable
	{
		private readonly ArrayList _elements;
		private readonly IQueryContainer _queryContainer;
		private readonly Type _type;
		private int _counter;
		private List<IDataRecord> _enumerateDataRecords;
		private Task _task;

		internal QueryEagerEnumerator(IQueryContainer queryContainer, Type type)
		{
			_queryContainer = queryContainer;
			_type = type;
			_elements = new ArrayList();
			_counter = 0;
			Load();
		}

		public void Dispose()
		{
			if (_task != null)
				_task.Dispose();
			if (_enumerateDataRecords != null)
				_enumerateDataRecords.Clear();
			_elements.Clear();
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
				Current = _queryContainer.AccessLayer.SetPropertysViaReflection(_queryContainer.AccessLayer.GetClassInfo(_type),
					dataRecord);
				_elements.Add(Current);

				return true;
			}
			catch (Exception)
			{
				throw;
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
				var query = _queryContainer.Compile();
				_queryContainer.AccessLayer.RaiseSelect(query);
				_enumerateDataRecords = _queryContainer.AccessLayer.EnumerateDataRecords(query);
			}, TaskCreationOptions.PreferFairness);
			_task.Start();
		}
	}

	internal class QueryEagerEnumerator<T> : QueryEagerEnumerator, IEnumerator<T>
	{
		internal QueryEagerEnumerator(IQueryContainer queryContainer, Type type)
			: base(queryContainer, type)
		{
		}

		internal QueryEagerEnumerator(IQueryContainer queryContainer)
			: base(queryContainer, typeof(T))
		{
		}

		public new T Current
		{
			get { return (T) base.Current; }
		}
	}
}