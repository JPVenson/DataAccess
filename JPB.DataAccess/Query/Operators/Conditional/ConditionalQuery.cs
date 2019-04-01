#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.DbInfoConfig.DbInfo;
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
			return Column(tCache.PrimaryKeyProperty.PropertyName);
		}

		/// <summary>
		///		Sets all Conditional expressions in ()
		/// </summary>
		/// <param name="operation"></param>
		/// <returns></returns>
		public ConditionalEvalQuery<TPoco> InBracket(Func<ConditionalQuery<TPoco>, ConditionalEvalQuery<TPoco>> operation)
		{
			var expression = new ConditionStructurePart(ConditionStructurePart.LogicalOperator.OpenBracket);
			ContainerObject.SearchLast<ConditionStatementQueryPart>().Conditions.Add(expression);
			var result = operation(this);
			result.ContainerObject.SearchLast<ConditionStatementQueryPart>().Conditions.Add(new ConditionStructurePart(ConditionStructurePart.LogicalOperator.CloseBracket));
			return result;
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
			return Column(tProp.PropertyName);
		}

		/// <summary>
		///     Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> Column(string columnName)
		{
			var cache = ContainerObject.AccessLayer.Config.GetOrCreateClassInfoCache(typeof(TPoco));

			return ConditionalColumnQueryByPath(new[]
			{
				new KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>(cache, cache
					.Propertys[columnName]),
			});
		}

		internal static ColumnInfo TraversePropertyPathToColumn(
			IReadOnlyCollection<KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>> columnPath,
			IQueryContainer container)
		{
			var aliasPath = columnPath.Take(columnPath.Count - 1)
				.Select(e => e.Value.PropertyName)
				.Aggregate(columnPath.First().Key.TableName, (e, f) => e + "." + f);
			var dbName = columnPath.LastOrDefault().Value.DbName;

			ColumnInfo columnDefinitionPart = null;
			container
				.SearchFirst<ISelectableQueryPart>(f => !(f is JoinTableQueryPart)
				                                        && (columnDefinitionPart = f.Columns
					                                        .FirstOrDefault(e =>
						                                        e.IsEquivalentTo(
							                                        dbName) &&
						                                        e.Alias.Equals(
							                                        container
								                                        .SearchTableAlias(
									                                        aliasPath)))) !=
				                                        null);
			if (columnDefinitionPart == null)
			{
				throw new InvalidOperationException();
			}
			return columnDefinitionPart;
		}

		private ConditionalColumnQuery<TPoco> ConditionalColumnQueryByPath(
			IReadOnlyCollection<KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>> columnPath)
		{
			var expression = new ExpressionConditionPart(TraversePropertyPathToColumn(columnPath, ContainerObject));
			ContainerObject.SearchLast<ConditionStatementQueryPart>().Conditions.Add(expression);

			return new ConditionalColumnQuery<TPoco>(this, expression);
		}

		/// <summary>
		///     Creates an Conditional Query that targets an single Column
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> Column<TA>(
			Expression<Func<TPoco, TA>> columnName)
		{
			var path = PropertyPath<TPoco>
				.Get(columnName)
				.Select(e =>
				{
					var dbClassInfoCache = ContainerObject.AccessLayer.GetClassInfo(e.DeclaringType);
					return new KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>(dbClassInfoCache, dbClassInfoCache.Propertys[e.Name]);
				})
				.ToArray();
			return ConditionalColumnQueryByPath(path);
		}
	}

	internal static class PropertyPath<TSource>
	{
		public static IReadOnlyList<MemberInfo> Get<TResult>(Expression<Func<TSource, TResult>> expression)
		{
			var visitor = new PropertyVisitor();
			visitor.Visit(expression.Body);
			visitor.Path.Reverse();
			return visitor.Path;
		}

		private class PropertyVisitor : ExpressionVisitor
		{
			internal readonly List<MemberInfo> Path = new List<MemberInfo>();

			protected override Expression VisitMember(MemberExpression node)
			{
				if (!(node.Member is PropertyInfo))
				{
					throw new ArgumentException("The path can only contain properties", nameof(node));
				}

				this.Path.Add(node.Member);
				return base.VisitMember(node);
			}
		}
	}
}