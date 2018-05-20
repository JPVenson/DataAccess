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
		private readonly bool _loadAsync;
		private int _counter;
		private List<IDataRecord> _enumerateDataRecords;
		private Task _task;

		internal QueryEagerEnumerator(IQueryContainer queryContainer, Type type, bool loadAsync)
		{
			_queryContainer = queryContainer;
			_type = type;
			_loadAsync = loadAsync;
			_elements = new ArrayList();
			_counter = 0;
			Load();
		}

		public void Dispose()
		{
			if (_task != null)
			{
				_task.Dispose();
			}
			if (_enumerateDataRecords != null)
			{
				_enumerateDataRecords.Clear();
			}
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
				{
					return false;
				}

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
			if (_loadAsync)
			{
				_task = new Task(LoadResults, TaskCreationOptions.PreferFairness);
				_task.Start();
			}
			else
			{
				_task = Task.FromResult("");
				LoadResults();
			}
		}

		private void LoadResults()
		{
			var query = _queryContainer.Compile();
			_queryContainer.AccessLayer.RaiseSelect(query);
			_enumerateDataRecords = _queryContainer.AccessLayer.EnumerateDataRecordsAsync(query);
		}
	}

	internal class QueryEagerEnumerator<T> : QueryEagerEnumerator, IEnumerator<T>
	{
		internal QueryEagerEnumerator(IQueryContainer queryContainer, Type type, bool loadAsync)
			: base(queryContainer, type, loadAsync)
		{
		}

		internal QueryEagerEnumerator(IQueryContainer queryContainer, bool loadAsync)
			: base(queryContainer, typeof(T), loadAsync)
		{
		}

		public new T Current
		{
			get { return (T) base.Current; }
		}
	}
}