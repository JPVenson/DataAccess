#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
		private readonly DbAccessLayer _accessLayer;
		private readonly IDataReader _executeReader;
		private readonly DbClassInfoCache _type;

		internal QueryLazyEnumerator(IQueryContainer queryContainer, Type type)
		{
			_type = queryContainer.AccessLayer.GetClassInfo(type);
			_accessLayer = new DbAccessLayer(queryContainer.AccessLayer.Database);
			_accessLayer.Database.Connect(IsolationLevel.ReadCommitted);
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
			_accessLayer.Database.CloseConnection();
		}

		public bool MoveNext()
		{
			if (_executeReader.IsClosed)
			{
				Dispose();
				return false;
			}

			if (_executeReader.Read())
				return true;
			return false;
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public object Current
		{
			get { return _accessLayer.SetPropertysViaReflection(_type, _executeReader); }
		}
	}

	internal class QueryLazyEnumerator<T> : QueryLazyEnumerator, IEnumerator<T>
	{
		internal QueryLazyEnumerator(IQueryContainer queryContainer, Type type)
			: base(queryContainer, type)
		{
		}

		internal QueryLazyEnumerator(IQueryContainer queryContainer)
			: base(queryContainer, typeof(T))
		{
		}

		public new T Current
		{
			get { return (T) base.Current; }
		}
	}
}