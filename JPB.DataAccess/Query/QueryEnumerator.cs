using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query
{
	public class QueryEnumerator<TE> : IEnumerable
		where TE : IQueryElement
	{
		private readonly QueryBuilder<TE> _builder;

		public QueryEnumerator(QueryBuilder<TE> builder)
		{
			_builder = builder.Clone() as QueryBuilder<TE>;
		}

		public IEnumerator GetEnumerator()
		{
			return _builder.GetEnumerator();
		}
	}

	public class QueryEnumerator<TE, T> : IEnumerable<T>
		where TE : IQueryElement
	{
		private readonly QueryBuilder<T, TE> _builder;

		public QueryEnumerator(QueryBuilder<T, TE> builder)
		{
			_builder = builder.Clone() as QueryBuilder<T, TE>;
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return _builder.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
		{
			return _builder.GetEnumerator();
		}
	}
}