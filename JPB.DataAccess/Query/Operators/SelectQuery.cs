#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
			ContainerObject.SearchLast<ISelectableQueryPart>().Distinct = true;
			return this;
		}
		
		/// <summary>
		///     Includes the forgin table
		/// </summary>
		public SelectQuery<TPoco> Join(string forginColumnName,
			JoinMode joinAs = null)
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
			}, joinAs);
		}

		/// <summary>
		///     Includes the forgin table
		/// </summary>
		public SelectQuery<TPoco> Join<TProp>(Expression<Func<TPoco, TProp>> forginColumnName,
			JoinMode joinAs = null)
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
			return JoinOn(path, joinAs);
		}

		private SelectQuery<TPoco> JoinOn(
			KeyValuePair<DbClassInfoCache, DbPropertyInfoCache>[] path,
			JoinMode joinAs = null)
		{
			joinAs = joinAs ?? JoinMode.Default;

			IQueryBuilder target = this;
			var targetAlias = target.ContainerObject
				.SearchLast<ISelectableQueryPart>(e => !(e is JoinTableQueryPart))
				.Alias;
			JoinParseInfo parentJoinPart = null;
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

				ColumnInfo onSourceTableKey;
				ColumnInfo selfPrimaryKey;

				var referenceColumn = keyValuePair.Value.ForginKeyAttribute?.Attribute;

				if (referenceColumn == null)
				{
					throw new InvalidOperationException("There is no known reference from table " +
					                                    $"'{keyValuePair.Key.Type}' " +
					                                    "to table " +
					                                    $"'{referenceType}'." +
					                                    "Use a ForeignKeyDeclarationAttribute to connect both");
				}

				var targetTable = target.ContainerObject.Search(targetAlias);

				var pathOfJoin = target.ContainerObject.GetPathOf(targetAlias) + "." +
				                 keyValuePair.Value.PropertyName;
				var parentAlias = target.ContainerObject
					.CreateTableAlias(pathOfJoin);

				var forginColumns = DbAccessLayer.GetSelectableColumnsOf(referencedTypeCache)
					.Select(e => new ColumnInfo(e, parentAlias, target.ContainerObject))
					.ToList();

				selfPrimaryKey 
					= targetTable.Columns.FirstOrDefault(e => e.IsEquivalentTo(referenceColumn.ForeignKey));
				onSourceTableKey 
					= forginColumns.FirstOrDefault(e => e.IsEquivalentTo(referenceColumn.ReferenceKey));

				var joinTableQueryPart = new JoinTableQueryPart(
					targetAliasOfJoin,
					targetAlias,
					parentAlias,
					keyValuePair.Key.Type,
					onSourceTableKey,
					selfPrimaryKey,
					forginColumns,
					keyValuePair.Value,
					joinAs);

				if (parentJoinPart != null)
				{
					parentJoinPart.DependingJoins.Add(joinTableQueryPart.JoinParseInfo);
				}
				else
				{
					target.ContainerObject.Joins.Add(joinTableQueryPart.JoinParseInfo);
				}

				parentJoinPart = joinTableQueryPart.JoinParseInfo;

				target.ContainerObject.SearchLast<SelectTableQueryPart>().AddJoin(joinTableQueryPart);
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

	/// <summary>
	/// </summary>
	public class JoinMode
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="JoinMode" /> class.
		/// </summary>
		/// <param name="joinType">Type of the join.</param>
		internal JoinMode(string joinType)
		{
			JoinType = joinType;
		}

		/// <summary>
		///     QueryCommand string
		/// </summary>
		/// <value>
		///     The type of the join.
		/// </value>
		public string JoinType { get; private set; }

		/// <summary>
		///		Defines the LEFT join mode
		/// </summary>
		public static readonly JoinMode Left = new JoinMode("LEFT");

		/// <summary>
		///		Defines the Default mode
		/// </summary>
		public static readonly JoinMode Default = new JoinMode("");

		/// <summary>
		///		Defines the FULL OUTER mode
		/// </summary>
		public static readonly JoinMode FullOuter = new JoinMode("FULL OUTER");

		/// <summary>
		///		Defines a SELF join mode
		/// </summary>
		public static readonly JoinMode Self = new JoinMode("SELF");

		/// <summary>
		///		Defines a SELF join mode
		/// </summary>
		public static readonly JoinMode Inner = new JoinMode("INNER");

		/// <summary>
		///		Defines a RIGHT join mode
		/// </summary>
		public static readonly JoinMode Right = new JoinMode("RIGHT");

		/// <summary>
		///     Jon modes for TSQL. This is an helper method that can be used to create JOINs by using the QueryCommand Builder
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public class TJoinMode : JoinMode
		{
			private static IEnumerable<TJoinMode> _joints;

			private TJoinMode(string joinType)
				: base(joinType)
			{
			}

			/// <summary>
			///     Returns a list of all Join values known be the system
			/// </summary>
			/// <returns></returns>
			public static IEnumerable<JoinMode> GetJoins()
			{
				if (_joints != null)
				{
					return _joints;
				}

				_joints =
					typeof(TJoinMode)
						.GetFields(BindingFlags.Static)
						.Select(s => s.GetValue(null))
						.Cast<TJoinMode>();
				return _joints;
			}
#pragma warning disable 1591
			public static readonly TJoinMode LeftOuter = new TJoinMode("LEFT OUTER");
			public static readonly TJoinMode RightOuter = new TJoinMode("RIGHT OUTER");
			public static readonly TJoinMode Outer = new TJoinMode("OUTER");
			public static readonly TJoinMode Cross = new TJoinMode("CROSS");
			public static readonly TJoinMode Full = new TJoinMode("FULL");
#pragma warning restore 1591
		}
	}
}