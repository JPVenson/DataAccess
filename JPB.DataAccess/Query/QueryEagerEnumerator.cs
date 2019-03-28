#region

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;

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

		public static IEnumerable<List<T>> Partition<T>(IList<T> source, int size)
		{
			for (var i = 0; i < Math.Ceiling(source.Count / (Double)size); i++)
				yield return new List<T>(source.Skip(size * i).Take(size));
		}

		private void LoadResults()
		{
			var dbCommand = _queryContainer.Compile(out var columns);
			foreach (var queryCommandInterceptor in _queryContainer.Interceptors)
			{
				dbCommand = queryCommandInterceptor.NonQueryExecuting(dbCommand);

				if (dbCommand == null)
				{
					throw new InvalidOperationException($"The Command interceptor: " +
					                                    $"'{queryCommandInterceptor}' has returned null");
				}
			}
			_queryContainer.AccessLayer.RaiseSelect(dbCommand);
			var dataRecords = _queryContainer.AccessLayer.EnumerateDataRecordsAsync(dbCommand)
				.ToArray();

			if (_queryContainer.PostProcessors.Any())
			{
				var context = new QueryProcessingRecordsContext(_queryContainer, _queryContainer.PostProcessors, columns);
				foreach (var queryContainerPostProcessor in _queryContainer.PostProcessors)
				{
					dataRecords = queryContainerPostProcessor.Transform(dataRecords, _type, context);
				}

				columns = context.Columns;
			}

			foreach (var queryContainerJoin in _queryContainer.Joins)
			{
				var context = new QueryProcessingRecordsContext(_queryContainer,
					_queryContainer.PostProcessors,
					columns);
				dataRecords = new RelationProcessor(queryContainerJoin.Value)
					.JoinTables(dataRecords,
					queryContainerJoin.Value.TargetTableType,
					context);
				columns = context.Columns;
			}

			var columnNames = columns.Select(f => f.NaturalName.TrimAlias()).ToArray();
			dataRecords = dataRecords.Select(record =>
					new EagarDataRecord(columnNames, record.MetaHeader.Values.ToArray()))
				.ToArray();

			var records = Partitioner.Create(dataRecords, true)
				.AsParallel()
				.Select(dataRecord => _queryContainer.AccessLayer
					.SetPropertysViaReflection(_queryContainer.AccessLayer.GetClassInfo(_type),
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