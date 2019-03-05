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
		private DbAccessLayer _accessLayer;
		private IDataReader _executeReader;
		private readonly DbClassInfoCache _type;
		private object _preObject = null;
		private Task _startupTask;

		internal QueryLazyEnumerator(IQueryContainer queryContainer, Type type, bool async)
		{
			_type = queryContainer.AccessLayer.GetClassInfo(type);
			_accessLayer = queryContainer.AccessLayer;
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
			_accessLayer.Database.Connect();
			var command = queryContainer.Compile();
			queryContainer.AccessLayer.RaiseSelect(command);
			try
			{
				_executeReader = command.ExecuteReader();
			}
			catch (Exception ex)
			{
				_accessLayer.RaiseFailedQuery(this, command, ex);
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

			_accessLayer.Database.CloseConnection(true);
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
			get { return _preObject ?? (_preObject = _accessLayer.SetPropertysViaReflection(_type, _executeReader)); }
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
			get { return (T) base.Current; }
		}
	}
}