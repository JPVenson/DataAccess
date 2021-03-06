using JetBrains.Annotations;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///     Defines methods for setting a Column
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class UpdateValueQuery<TPoco>
	{
		private readonly UpdateTableWithQueryPart.ColumnAssignment _columnAssignment;
		private readonly IQueryBuilder _queryBuilder;

		/// <inheritdoc />
		internal UpdateValueQuery(IQueryBuilder queryBuilder,
			UpdateTableWithQueryPart.ColumnAssignment columnAssignment)
		{
			_queryBuilder = queryBuilder;
			_columnAssignment = columnAssignment;
		}

		/// <summary>
		///     Declares the value to set the given column
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[PublicAPI]
		public NextUpdateOrCondtionQuery<TPoco> Value(object value)
		{
			var paramValue = $"@setArg{_queryBuilder.ContainerObject.GetNextParameterId()}";
			var queryParameter = new QueryParameter(paramValue, value);

			return QueryValue(paramValue, queryParameter);
		}

		/// <summary>
		///     Sets the Column defined to the result of the query
		/// </summary>
		/// <param name="value"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		[PublicAPI]
		public NextUpdateOrCondtionQuery<TPoco> QueryValue(string value, params IQueryParameter[] arguments)
		{
			_columnAssignment.Value = value;
			_columnAssignment.QueryParameters.AddRange(arguments);
			_queryBuilder.ContainerObject.SearchLast<UpdateTableWithQueryPart>()
				.ColumnAssignments
				.Add(_columnAssignment);
			return new NextUpdateOrCondtionQuery<TPoco>(_queryBuilder);
		}
	}
}