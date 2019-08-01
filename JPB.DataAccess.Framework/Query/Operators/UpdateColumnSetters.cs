using JetBrains.Annotations;
using JPB.DataAccess.Framework.Query.Contracts;

namespace JPB.DataAccess.Framework.Query.Operators
{
	/// <summary>
	///     Defines methods for an UPDATE query
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
		///     Syntax for setting the Entitys
		/// </summary>
		[PublicAPI]
		public SetValueForUpdateQuery<TPoco> Set
		{
			get { return new SetValueForUpdateQuery<TPoco>(_queryBuilder); }
		}
	}
}