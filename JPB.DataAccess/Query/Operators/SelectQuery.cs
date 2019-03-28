#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Operators.ElementProducer{TPoco}" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.ISelectQuery{TPoco}" />
	public class SelectQuery<TPoco> : ElementProducer<TPoco>, ISelectQuery<TPoco>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SelectQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public SelectQuery(IQueryBuilder database) : base(database)
		{
			CreateNewIdentifier();
		}

		/// <summary>
		///     Selects items Distinct
		/// </summary>
		/// <returns></returns>
		public SelectQuery<TPoco> Distinct()
		{
			ContainerObject.Search<ISelectableQueryPart>().Distinct = true;
			return this;
		}

		/// <summary>
		///     Includes the forgin table
		/// </summary>
		/// <param name="forginColumnName"></param>
		/// <returns></returns>
		public SelectQuery<TPoco> Join(string forginColumnName)
		{
			var teCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			var forginColumn = teCache.Propertys.FirstOrDefault(e => e.Value.PropertyName.Equals(forginColumnName));
			if (forginColumn.Value == null)
			{
				return this;
			}

			return JoinOn(new[]
			{
				new KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>(teCache, forginColumn.Value)
			});
		}

		/// <summary>
		///     Includes the forgin table
		/// </summary>
		/// <param name="forginColumnName"></param>
		/// <returns></returns>
		public SelectQuery<TPoco> Join<TProp>(Expression<Func<TPoco, TProp>> forginColumnName)
			where TProp : class
		{
			var path = PropertyPath<TPoco>
				.Get(forginColumnName)
				.Where(e => !typeof(IDbCollection).IsAssignableFrom(e.DeclaringType))
				.Select(e =>
				{
					var dbClassInfoCache = ContainerObject.AccessLayer.GetClassInfo(e.DeclaringType);
					return new KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>(dbClassInfoCache,
						dbClassInfoCache.Propertys[e.Name]);
				})
				.ToArray();
			return JoinOn(path);
		}

		private SelectQuery<TPoco> JoinOn(
			KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>[] path)
		{
			IQueryBuilder target = this;
			var targetAlias = target.ContainerObject.Search<ISelectableQueryPart>().Alias;
			JoinTableQueryPart parentJoinPart = null;
			foreach (var keyValuePair in path)
			{
				Type referenceType;
				if (keyValuePair.Value.CheckForListInterface())
				{
					referenceType = keyValuePair.Value.PropertyType.GetElementType();
					if (referenceType == null)
					{
						referenceType = keyValuePair.Value.PropertyType.GetGenericArguments().FirstOrDefault();
					}
				}
				else
				{
					referenceType = keyValuePair.Value.PropertyType;
				}

				var referencedTypeCache = target.ContainerObject.AccessLayer.GetClassInfo(referenceType);

				var targetAliasOfJoin = new QueryIdentifier
				{
					QueryIdType = QueryIdentifier.QueryIdTypes.Table,
					Value = referencedTypeCache.TableName
				};

				string onSourceTableKey;
				string selfPrimaryKey;

				var referenceColumn = keyValuePair.Value.ForginKeyAttribute?.Attribute;

				if (referenceColumn == null)
				{
					throw new InvalidOperationException("There is no known reference from table " +
					                                    $"'{keyValuePair.Key.Type}' " +
					                                    "to table " +
					                                    $"'{referenceType}'." +
					                                    "Use a ForeignKeyDeclarationAttribute to connect both");
				}

				onSourceTableKey = referenceColumn.ReferenceKey;
				selfPrimaryKey = referenceColumn.ForeignKey;

				var forginColumns = DbAccessLayer.GetSelectableColumnsOf(referencedTypeCache, null);
				var pathOfJoin = target.ContainerObject.GetPathOf(targetAlias) + "." +
				        keyValuePair.Value.PropertyName;
				var parentAlias = target.ContainerObject
					.CreateTableAlias(pathOfJoin);

				var joinTableQueryPart = new JoinTableQueryPart(
					targetAliasOfJoin,
					targetAlias,
					parentAlias,
					keyValuePair.Key.Type,
					onSourceTableKey,
					selfPrimaryKey,
					forginColumns, 
					ContainerObject);

				if (parentJoinPart != null)
				{
					parentJoinPart.DependingJoins.Add(joinTableQueryPart);
				}
				else
				{
					target.ContainerObject.Joins.Add(pathOfJoin, joinTableQueryPart);
				}

				parentJoinPart = joinTableQueryPart;

				target.ContainerObject.Search<SelectTableQueryPart>().AddJoin(joinTableQueryPart);
				target = target.Add(joinTableQueryPart);
				targetAlias = parentAlias;
			}

			return new SelectQuery<TPoco>(target);
		}

		/// <summary>
		///     Retuns a collection of all Entites that are referenced by element
		///     Needs a proper ForginKeyDeclartaion
		/// </summary>
		/// <typeparam name="TEPoco"></typeparam>
		/// <param name="element"></param>
		/// <returns></returns>
		public ConditionalEvalQuery<TPoco> In<TEPoco>(TEPoco element)
		{
			var teCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TEPoco));
			var pkValue = teCache.PrimaryKeyProperty.Getter.Invoke(element);
			return In<TEPoco>(pkValue);
		}

		/// <summary>
		///     Retuns a collection of all Entites that are referenced by element
		///     Needs a proper ForginKeyDeclartaion
		/// </summary>
		/// <typeparam name="TEPoco"></typeparam>
		/// <returns></returns>
		public ConditionalEvalQuery<TPoco> In<TEPoco>(object id)
		{
			var teCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TEPoco));
			var fkPropertie = Cache.Propertys
				.SingleOrDefault(s =>
					s.Value.ForginKeyDeclarationAttribute != null &&
					(s.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType == typeof(TEPoco) ||
					 s.Value.ForginKeyDeclarationAttribute.Attribute.ForeignTable == teCache.TableName))
				.Value;

			if (fkPropertie == null)
			{
				throw new NotSupportedException(
					string.Format("No matching Column was found for Forgin key declaration for table {0}",
						teCache.TableName));
			}

			return Where.Column(fkPropertie.DbName).Is.EqualsTo(id);
		}
	}
}