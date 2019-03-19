#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query
{
	/// <summary>
	/// </summary>
	internal class QueryLazyEnumerator : IEnumerator, IDisposable
	{
		private IDataReader _executeReader;
		private readonly DbClassInfoCache _type;
		private object _preObject = null;
		private Task _startupTask;
		private readonly IQueryContainer _queryContainer;

		internal QueryLazyEnumerator(IQueryContainer queryContainer, Type type, bool async)
		{
			_queryContainer = queryContainer;
			_type = queryContainer.AccessLayer.GetClassInfo(type);
			if (async)
			{
				_startupTask = new Task(() => OpenReader(queryContainer));
				_startupTask.Start();
			}
			else
			{
				OpenReader(queryContainer);
			}
		}

		private void OpenReader(IQueryContainer queryContainer)
		{
			_queryContainer.AccessLayer.Database.Connect();
			var dbCommand = queryContainer.Compile();
			foreach (var queryCommandInterceptor in _queryContainer.Interceptors)
			{
				dbCommand = queryCommandInterceptor.NonQueryExecuting(dbCommand);

				if (dbCommand == null)
				{
					throw new InvalidOperationException($"The Command interceptor: '{queryCommandInterceptor}' has returned null");
				}
			}
			queryContainer.AccessLayer.RaiseSelect(dbCommand);
			try
			{
				_executeReader = dbCommand.ExecuteReader();
			}
			catch (Exception ex)
			{
				_queryContainer.AccessLayer.RaiseFailedQuery(this, dbCommand, ex);
				throw;
			}
		}

		public void Dispose()
		{
			_startupTask?.Wait();
			if (_executeReader != null)
			{
				_executeReader.Close();
				_executeReader.Dispose();
			}

			_queryContainer.AccessLayer.Database.CloseConnection(true);
		}

		public bool MoveNext()
		{
			_startupTask?.Wait();
			_preObject = null;
			if (_executeReader.IsClosed)
			{
				return false;
			}

			if (_executeReader.Read())
			{
				return true;
			}
			return false;
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public object Current
		{
			get { return _preObject ?? (_preObject = _queryContainer.AccessLayer.SetPropertysViaReflection(_type, _executeReader)); }
		}
	}

	internal class QueryLazyEnumerator<T> : QueryLazyEnumerator, IEnumerator<T>
	{
		internal QueryLazyEnumerator(IQueryContainer queryContainer, Type type, bool async)
			: base(queryContainer, type, async)
		{
		}

		internal QueryLazyEnumerator(IQueryContainer queryContainer, bool async)
			: base(queryContainer, typeof(T), async)
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