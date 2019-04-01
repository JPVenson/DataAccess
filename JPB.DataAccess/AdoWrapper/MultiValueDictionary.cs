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
			Collection = new List<KeyValuePair<TKey, TValue>>();
		}

		public MultiValueDictionary(MultiValueDictionary<TKey, TValue> subRecordMetaHeader)
		{
			Collection = new List<KeyValuePair<TKey, TValue>>();
			foreach (var o in subRecordMetaHeader.Collection)
			{
				Collection.Add(o);
			}
		}

		public List<KeyValuePair<TKey, TValue>> Collection { get; private set; }

		public IEnumerable<TValue> Values
		{
			get { return Collection.Select(e => e.Value); }
		}

		public int Count
		{
			get { return Collection.Count; }
		}

		public void Add(TKey key2, TValue value)
		{
			Collection.Add(new KeyValuePair<TKey, TValue>(key2, value));
		}

		public void Remove(TKey key)
		{
			Collection.RemoveAll(e => e.Key.Equals(key));
		}

		public int IndexOf(TKey key)
		{
			return Collection.FindIndex(f => f.Key.Equals(key));
		}

		public TKey KeyAt(int index)
		{
			return Collection[index].Key;

			//return Collection.IndexOfKey(key);
		}
		
		public TValue this[int index]
		{
			get
			{
				if (index <= Collection.Count)
				{
					return Collection[index].Value;
				}

				return default(TValue);
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				if (Collection.Any(e => e.Key.Equals(key)))
				{
					return Collection.FirstOrDefault(e => e.Key.Equals(key)).Value;
				}
				return default(TValue);
			}
		}

		public IList ToArray()
		{
			return Collection.Select(e => e.Value).ToArray();
		}
	}
}