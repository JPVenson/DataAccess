using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///		Defines mehtods for an UPDATE query
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class UpdateColumnSetters<TPoco>
	{
		private readonly IQueryBuilder _queryBuilder;

		/// <inheritdoc />
		public UpdateColumnSetters(IQueryBuilder queryBuilder)
		{
			_queryBuilder = queryBuilder;
		}

		/// <summary>
		///		Syntax for setting the Entitys
		/// </summary>
		public SetValueForUpdateQuery<TPoco> Set
		{
			get
			{
				return new SetValueForUpdateQuery<TPoco>(_queryBuilder);
			}
		}
	}
}