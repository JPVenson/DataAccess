#region

using System;
using System.Linq;
using System.Linq.Expressions;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators.Conditional
{
	/// <summary>
	///     Creates an Conditional Query that allows you to filter the Previus query
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class ConditionalQuery<TPoco> : QueryBuilderX, IConditionalQuery<TPoco>
	{
		/// <summary>
		///     Creates a new Instance based on the previus query
		/// </summary>
		/// <param name="queryText"></param>
		public ConditionalQuery(IQueryBuilder queryText) : base(queryText)
		{
		}
		
		/// <summary>
		///		Selects the current PrimaryKey
		/// </summary>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> PrimaryKey()
		{
			var tCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			return Column(tCache.PrimaryKeyProperty.DbName);
		}

		/// <summary>
		///		Selects the ForginKey to the table.
		/// </summary>
		/// <exception cref="InvalidOperationException">If there are 0 or more then 1 forginKeys</exception>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> ForginKey<TFkPoco>()
		{
			var tCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			var tProp = tCache.Propertys.Values
			                  .Single(e =>
				                  e.ForginKeyDeclarationAttribute != null &&
				                  e.ForginKeyDeclarationAttribute.Attribute.ForeignType == typeof(TFkPoco));
			return Column(tProp.DbName);
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> Column(string columnName)
		{
			var currentAlias = ContainerObject.Search<IIdentifiableQueryPart>().Alias;
			var columnInfos = ContainerObject.Search<ISelectableQueryPart>()
				.Columns.ToArray();

			var columnDefinitionPart = columnInfos.FirstOrDefault(e => e.IsEquivalentTo(columnName));
			if (columnDefinitionPart == null)
			{
				throw new InvalidOperationException($"You have tried to create an expression for the column '{columnName}' on table '{typeof(TPoco)}' that does not exist.");
			}
			var expression = new ExpressionConditionPart(currentAlias, columnDefinitionPart);
			ContainerObject.Search<ConditionStatementQueryPart>().Conditions.Add(expression);

			return new ConditionalColumnQuery<TPoco>(this, expression);
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> Column<TA>(
			Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetPropertyInfoFromLamdba();
			var propName = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			return Column(propName.DbName);
		}
	}
}