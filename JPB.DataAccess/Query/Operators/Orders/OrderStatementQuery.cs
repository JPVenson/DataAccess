#region

using System;
using System.Linq.Expressions;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;

#endregion

namespace JPB.DataAccess.Query.Operators.Orders
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IOrderdElementProducer{TPoco}" />
	public class OrderStatementQuery<TPoco> : QueryBuilderX, IOrderdElementProducer<TPoco>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="OrderStatementQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public OrderStatementQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     Uses
		/// </summary>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public OrderByColumn<TPoco> By(string columnName)
		{
			return new OrderByColumn<TPoco>(this.QueryText(columnName));
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="TA">The type of a.</typeparam>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public OrderByColumn<TPoco> By<TA>(
			Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetPropertyInfoFromLabda();
			var propName = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			return By(propName.DbName);
		}
	}
}