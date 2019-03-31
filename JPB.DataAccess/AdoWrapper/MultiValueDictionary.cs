using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace JPB.DataAccess.AdoWrapper
{
	internal class MultiValueDictionary<TKey, TValue>
	{
		public MultiValueDictionary()
		{
			Collection = new SortedList<TKey, TValue>();
		}

		public SortedList<TKey, TValue> Collection { get; private set; }

		public IEnumerable<TValue> Values
		{
			get { return Collection.Values; }
		}

		public int Count
		{
			get { return Collection.Count; }
		}

		public void Add(TKey key2, TValue value)
		{
			Collection.Add(key2, value);
		}

		public void Remove(TKey key)
		{
			Collection.Remove(key);
		}

		public int IndexOf(TKey key)
		{
			return Collection.IndexOfKey(key);
		}

		public TKey KeyAt(int index)
		{
			return Collection.Keys[index];

			//return Collection.IndexOfKey(key);
		}
		
		public TValue this[int index]
		{
			get
			{
				return Collection[Collection.Keys[index]];

				//if (DictA.ContainsKey(index))
				//{
				//	return DictA[index];
				//}

				//return default(TValue);
			}
		}

		public TValue this[TKey index]
		{
			get
			{
				return Collection[index];
				//if (DictB.ContainsKey(index))
				//{
				//	return DictB[index];
				//}

				//return default(TValue);
			}
		}
	}
}