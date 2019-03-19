﻿#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query
{
	internal class QueryEagerEnumerator : IEnumerator, IDisposable
	{
		private IEnumerator _elements;
		private readonly IQueryContainer _queryContainer;
		private readonly Type _type;
		private readonly bool _loadAsync;

		private Task _task;

		internal QueryEagerEnumerator(IQueryContainer queryContainer, Type type, bool loadAsync)
		{
			_queryContainer = queryContainer;
			_type = type;
			_loadAsync = loadAsync;
			Load();
		}

		public void Dispose()
		{
			_task?.Dispose();
		}

		public bool MoveNext()
		{
			_task.Wait();
			return _elements.MoveNext();
		}

		public void Reset()
		{
			_elements.Reset();
		}

		public object Current
		{
			get { return _elements.Current; }
		}

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
			var dbCommand = _queryContainer.Compile();
			foreach (var queryCommandInterceptor in _queryContainer.Interceptors)
			{
				dbCommand = queryCommandInterceptor.NonQueryExecuting(dbCommand);

				if (dbCommand == null)
				{
					throw new InvalidOperationException($"The Command interceptor: '{queryCommandInterceptor}' has returned null");
				}
			}
			_queryContainer.AccessLayer.RaiseSelect(dbCommand);
			var dataRecords = _queryContainer.AccessLayer.EnumerateDataRecordsAsync(dbCommand);

			if (_queryContainer.PostProcessors.Any())
			{
				var context = new QueryProcessingRecordsContext(); 
				var processedRecords = new List<IDataRecord>();

				foreach (var element in dataRecords)
				{
					var item = element;
					foreach (var queryContainerPostProcessor in _queryContainer.PostProcessors)
					{
						item = queryContainerPostProcessor.Transform(item, _type, context);
					}
					processedRecords.Add(item);
				}

				dataRecords = processedRecords;
			}

			var records = dataRecords.Select(dataRecord => _queryContainer.AccessLayer.SetPropertysViaReflection(_queryContainer.AccessLayer.GetClassInfo(_type),
					dataRecord))
				.ToArray();

			var elements = new ArrayList();
			if (_queryContainer.PostProcessors.Any())
			{
				var context = new QueryProcessingEntitiesContext(records);
				foreach (var element in records)
				{
					var item = element;
					foreach (var queryContainerPostProcessor in _queryContainer.PostProcessors)
					{
						item = queryContainerPostProcessor.Transform(item, _type, context);
					}

					elements.Add(item);
				}
			}
			else
			{
				elements.AddRange(records);
			}

			_elements = elements.GetEnumerator();
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
			get
			{
				if (base.Current is T)
				{
					return (T)base.Current;
				}
				return (T)Convert.ChangeType(base.Current, typeof(T));
			}
		}
	}
}