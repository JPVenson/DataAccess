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
using JPB.DataAccess.Query.Contracts;

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
			_type = type.GetClassInfo();
			_accessLayer = new DbAccessLayer(queryContainer.AccessLayer.Database);
			_accessLayer.Database.Connect(IsolationLevel.ReadCommitted);
			_executeReader = queryContainer.Compile().ExecuteReader();
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

		public new T Current
		{
			get { return (T) base.Current; }
		}
	}
}