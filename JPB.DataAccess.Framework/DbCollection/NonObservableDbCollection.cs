using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using JPB.DataAccess.Framework.AdoWrapper;
using JPB.DataAccess.Framework.DbInfoConfig;
#if !DEBUG
using System.Diagnostics;
#endif

namespace JPB.DataAccess.Framework.DbCollection
{
	/// <summary>
	///     For internal use only
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NonObservableDbCollection<T> : IEnumerable<T>
	{
		private readonly List<T> _base;

		/// <summary>
		///     Internal use only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public NonObservableDbCollection(IEnumerable enumerable)
		{
			_base = new List<T>();
			foreach (T item in enumerable)
			{
				_base.Add(item);
			}
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     An enumerator that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			return _base.GetEnumerator();
		}

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _base.GetEnumerator();
		}

		/// <summary>
		///     Creates a DbCollection that contains the XML elements
		/// </summary>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		[UsedImplicitly]
		public static NonObservableDbCollection<T> FromXml(string xml)
		{
			return new NonObservableDbCollection<T>(
			XmlDataRecord.TryParse(xml,
			             typeof(T), false)
			             .CreateListOfItems()
			             .Select(item => typeof(T)
			                             .GetClassInfo()
			                             .SetPropertysViaReflection(EagarDataRecord.WithExcludedFields(item))));
		}
	}
}