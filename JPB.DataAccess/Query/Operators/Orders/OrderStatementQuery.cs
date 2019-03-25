﻿#region

using System;
using System.Linq;
using System.Linq.Expressions;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;

#endregion

namespace JPB.DataAccess.Query.Operators.Orders
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IOrderdElementProducer{TPoco}" />
	public class OrderStatementQuery<TPoco> : IOrderdElementProducer<TPoco>
	{
		private readonly IQueryBuilder _queryBuilder;

		internal OrderStatementQuery(IQueryBuilder queryBuilder)
		{
			_queryBuilder = queryBuilder;
		}

		/// <summary>
		///     Uses
		/// </summary>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public OrderByColumn<TPoco> By(string columnName)
		{
			var columnInfos = _queryBuilder.ContainerObject.Search<ISelectableQueryPart>()
				.Columns.ToArray();
			var columnDefinitionPart = columnInfos.FirstOrDefault(e => e.IsEquivalentTo(columnName));
			if (columnDefinitionPart == null)
			{
				throw new InvalidOperationException($"You have tried to create an expression for the column '{columnName}' on table '{typeof(TPoco)}' that does not exist.");
			}

			_queryBuilder.ContainerObject.Search<OrderByColumnQueryPart>().Columns.Add(columnDefinitionPart);
			return new OrderByColumn<TPoco>(_queryBuilder);
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
			var member = columnName.GetPropertyInfoFromLamdba();
			var propName = _queryBuilder.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			return By(propName.DbName);
		}
	}
}