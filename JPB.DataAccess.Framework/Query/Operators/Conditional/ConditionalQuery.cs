#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.EntityCollections;
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
		
		[Obsolete("Renamed to ForeignKey")]
		public ConditionalColumnQuery<TPoco> ForginKey<TFkPoco>()
		{
			return ForeignKey<TFkPoco>();
		}

		public ConditionalColumnQuery<TPoco> ForeignKey<TFkPoco>()
		{
			var tCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			var tProp = tCache.Propertys.Values
							  .Single(e =>
								  e.ForginKeyDeclarationAttribute?.Attribute.CompileInfoWith(ContainerObject.AccessLayer.Config).ForeignType == typeof(TFkPoco));
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

			var path = PropertyPath<TPoco>
				.Get(ContainerObject.AccessLayer.Config, columnName)
				.Where(e => !typeof(IDbCollection).IsAssignableFrom(e.DeclaringType))
				.Select(e =>
				{
					var dbClassInfoCache = ContainerObject.AccessLayer.GetClassInfo(e.DeclaringType);
					if (dbClassInfoCache.Propertys.ContainsKey(e.Name))
					{
						return new KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>(dbClassInfoCache, dbClassInfoCache.Propertys[e.Name]);
					}
					else
					{
						throw new InvalidOperationException($"The expected property '{e.Name}' was not found.")
						{
							Data =
							{
								{"Class", dbClassInfoCache }
							}
						};
					}
				})
				.ToArray();

			return ConditionalColumnQueryByPath(path);
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

		public ConditionalColumnQuery<TPoco> Column(QueryIdentifier columnName)
		{
			var findColumn = ContainerObject.Parts.OfType<ISelectableQueryPart>()
				.SelectMany(f => f.Columns)
				.FirstOrDefault(e => e.ColumnIdentifierEntry == columnName);
			if (findColumn == null)
			{
				throw new InvalidOperationException($"The requested Column '{columnName.Value}' was not found");
			}
			var expression = new ExpressionConditionPart(findColumn);
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
				.Where(e => !typeof(IDbCollection).IsAssignableFrom(e.DeclaringType))
				.Select(e =>
				{
					var dbClassInfoCache = ContainerObject.AccessLayer.GetClassInfo(e.DeclaringType);
					if (dbClassInfoCache.Propertys.ContainsKey(e.Name))
					{
						return new KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>(dbClassInfoCache, dbClassInfoCache.Propertys[e.Name]);
					}
					else
					{
						throw new InvalidOperationException($"The expected property '{e.Name}' was not found.")
						{
							Data =
							{
								{"Class", dbClassInfoCache }
							}
						};
					}
				})
				.ToArray();
			return ConditionalColumnQueryByPath(path);
		}
	}

	internal static class PropertyPath<TSource>
	{
		public static IReadOnlyCollection<KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>> GetFromPath(DbConfig cache, string path)
		{
			var target = cache.GetOrCreateClassInfoCache(typeof(TSource));
			var pathParts = path.Split('.');
			var pathCompiled = new List<KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>>();

			for (var index = 0; index < pathParts.Length; index++)
			{
				var pathPart = pathParts[index];
				var prop = target.Propertys.FirstOrDefault(e =>
					e.Value.PropertyName.Equals(pathPart, StringComparison.InvariantCulture));
				var nTarget = prop.Value?.DeclaringClass;

				target = nTarget ?? throw new InvalidOperationException($"Could not find part of the path '{path}'. The {index} part could not be found on {target.Name}");
				pathCompiled.Add(new KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>(nTarget, prop.Value));
			}

			return pathCompiled;
		}

		public static IReadOnlyList<MemberInfo> Get<TResult>(Expression<Func<TSource, TResult>> expression)
		{
			var visitor = new PropertyVisitor();
			visitor.Visit(expression.Body);
			visitor.Path.Reverse();
			return visitor.Path;
		}

		public static IReadOnlyList<MemberInfo> Get(DbConfig cache, string expression)
		{
			var propQueue = new Queue<MemberInfo>();
			var classInfoCache = cache.GetOrCreateClassInfoCache(typeof(TSource));

			foreach (var pathPart in expression.Split('.'))
			{
				var property = classInfoCache.Propertys.FirstOrDefault(e =>
					e.Key.Equals(pathPart, StringComparison.InvariantCultureIgnoreCase));
				propQueue.Enqueue(property.Value.PropertyInfo);
				classInfoCache = cache.GetOrCreateClassInfoCache(property.Value.PropertyType);
			}

			return propQueue.ToArray();
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