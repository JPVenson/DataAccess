using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.QueryBuilder
{
	/// <summary>
	/// </summary>
	internal class QueryLazyEnumerator : IEnumerator, IDisposable
	{
		private readonly DbAccessLayer _accessLayer;
		private readonly IDataReader _executeReader;
		private readonly DbClassInfoCache _type;

		internal QueryLazyEnumerator(QueryBuilder queryBuilder, Type type)
		{
			_type = type.GetClassInfo();
			_accessLayer = new DbAccessLayer(queryBuilder.Database);
			_accessLayer.Database.Connect(IsolationLevel.ReadCommitted);
			_executeReader = queryBuilder.Compile().ExecuteReader();
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
			get { return _type.SetPropertysViaReflection(_executeReader); }
		}
	}

	internal class QueryLazyEnumerator<T> : QueryLazyEnumerator, IEnumerator<T>
	{
		internal QueryLazyEnumerator(QueryBuilder queryBuilder, Type type)
			: base(queryBuilder, type)
		{
		}

		public new T Current
		{
			get { return (T) base.Current; }
		}
	}
}