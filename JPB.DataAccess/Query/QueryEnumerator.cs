#region

using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query
{
	//[Obsolete("This type of Query Enumerator is obsolete. Use the QueryEnumeratorEx instadt", true)]
	//public class QueryEnumerator<TE> : IEnumerable
	//	where TE : IQueryElement
	//{
	//	private QueryBuilder<TE> _builder;

	//	public QueryEnumerator(QueryBuilder<TE> builder)
	//	{
	//		_builder = builder.Clone() as QueryBuilder<TE>;
	//	}

	//	public IEnumerator GetEnumerator()
	//	{
	//		return _builder.GetEnumerator();
	//	}
	//}

	//[Obsolete("This type of Query Enumerator is obsolete. Use the QueryEnumeratorEx instadt", true)]
	//public class QueryEnumerator<TE, T> : IEnumerable<T>
	//	where TE : IQueryElement
	//{
	//	private QueryBuilder<T, TE> _builder;

	//	public QueryEnumerator(QueryBuilder<T, TE> builder)
	//	{
	//		_builder = builder.Clone() as QueryBuilder<T, TE>;
	//	}

	//	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	//	{
	//		return _builder.GetEnumerator();
	//	}

	//	public IEnumerator GetEnumerator()
	//	{
	//		return _builder.GetEnumerator();
	//	}
	//}

	/// <summary>
	///     Wrapes the QueryBuilderX element for an IEnumerable that is strongly typed
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class QueryEnumeratorEx<TPoco> : IEnumerable<TPoco>
	{
		private readonly IQueryBuilder _builder;

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public QueryEnumeratorEx(IQueryBuilder builder)
		{
			_builder = builder;
		}

		IEnumerator<TPoco> IEnumerable<TPoco>.GetEnumerator()
		{
			return _builder.GetEnumerator<TPoco>();
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