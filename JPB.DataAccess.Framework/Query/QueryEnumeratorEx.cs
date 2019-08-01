#region

using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Framework.Query.Contracts;

#endregion

namespace JPB.DataAccess.Framework.Query
{
	/// <summary>
	///     Wrapes the QueryBuilderX element for an IEnumerable that is strongly typed
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class QueryEnumeratorEx<TPoco> : IEnumerable<TPoco>
	{
		private readonly IEnumerableQuery<TPoco> _builder;
		private readonly bool _async;

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public QueryEnumeratorEx(IEnumerableQuery<TPoco> builder, bool async)
		{
			_builder = builder;
			_async = async;
		}

		IEnumerator<TPoco> IEnumerable<TPoco>.GetEnumerator()
		{
			return _builder.GetEnumerator();
		}

		/// <summary>
		///     Returns an enumerator that contains all elements in the given QueryBuilderX
		/// </summary>
		public IEnumerator GetEnumerator()
		{
			return ((IEnumerable<TPoco>) this).GetEnumerator();
		}
	}
}