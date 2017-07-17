#region

using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///     For Internal Usage only
	/// </summary>
	public class QueryElement : QueryBuilderX, IQueryElement
	{
		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public QueryElement(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public QueryElement(IQueryContainer database) : base(database)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public QueryElement(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public QueryElement(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public QueryElement(DbAccessLayer database) : base(database)
		{
		}
	}
}