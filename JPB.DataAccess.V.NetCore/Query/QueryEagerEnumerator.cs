﻿/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License.
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Query.Contracts;

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
				Current = _queryContainer.AccessLayer.SetPropertysViaReflection(_queryContainer.AccessLayer.GetClassInfo(_type), dataRecord);
				_elements.Add(Current);

				return true;
			}
			catch (Exception)
			{
				return false;
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
			});
			_task.Start();
		}

		public void Dispose()
		{
			if (_task != null)
				_task.Dispose();
			if (_enumerateDataRecords != null)
				_enumerateDataRecords.Clear();
			_elements.Clear();
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
			get { return (T)base.Current; }
		}
	}
}