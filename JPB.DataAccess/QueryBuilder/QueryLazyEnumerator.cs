/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
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
			_accessLayer = new DbAccessLayer(queryBuilder.AccessLayer);
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
			get { return _type.SetPropertysViaReflection(_accessLayer.Database, _executeReader); }
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