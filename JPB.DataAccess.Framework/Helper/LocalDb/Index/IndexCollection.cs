using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Framework.Helper.LocalDb.Trigger;

namespace JPB.DataAccess.Framework.Helper.LocalDb.Index
{
	internal class IndexCollection<T> : IIndexCollectionInteralUsage<T>
	{
		public IndexCollection()
		{
			 _base = new List<IDbIndex<T>>();
		}

		private ICollection<IDbIndex<T>> _base;
		public IEnumerator<IDbIndex<T>> GetEnumerator()
		{
			return _base.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _base).GetEnumerator();
		}

		public void Add(IDbIndex<T> item)
		{
			_base.Add(item);
		}

		public void Clear()
		{
			_base.Clear();
		}

		public bool Contains(IDbIndex<T> item)
		{
			return _base.Contains(item);
		}

		public void CopyTo(IDbIndex<T>[] array, int arrayIndex)
		{
			_base.CopyTo(array, arrayIndex);
		}

		public bool Remove(IDbIndex<T> item)
		{
			return _base.Remove(item);
		}

		public int Count
		{
			get { return _base.Count; }
		}

		public bool IsReadOnly
		{
			get { return _base.IsReadOnly; }
		}

		public void Add(T item)
		{
			foreach (var index in _base)
			{
				index.Add(item);
			}
		}

		public void Delete(T item)
		{
			foreach (var index in _base)
			{
				index.Delete(item);
			}
		}

		public void Update(T item)
		{
			foreach (var index in _base)
			{
				index.Update(item);
			}
		}
	}
}
