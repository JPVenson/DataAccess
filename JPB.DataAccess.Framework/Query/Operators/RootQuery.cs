#region

using System;
using JetBrains.Annotations;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.Operators.Selection;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///     Defines the root for every Query
	/// </summary>
	/// <seealso cref="QueryBuilderX" />
	/// <seealso cref="IRootQuery" />
	public class RootQuery : QueryBuilderX, IRootQuery
	{
		/// <inheritdoc />
		public RootQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <inheritdoc />
		public RootQuery(DbAccessLayer database) : base(database)
		{

		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public DatabaseObjectSelector Select
		{
			get { return new DatabaseObjectSelector(this); }
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public CountElementsObjectSelector Count
		{
			get { return new CountElementsObjectSelector(this); }
		}

		/// <summary>
		/// Creates an Update Statement
		/// </summary>
		public PrepaireUpdateQuery Update
		{
			get { return new PrepaireUpdateQuery(this); }
		}

		/// <summary>
		///     Adds a Delete - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[MustUseReturnValue]
		public ConditionalEvalQuery<T> Delete<T>(T obj)
		{
			var classInfo = ContainerObject.AccessLayer.GetClassInfo(typeof(T));
			var primaryKey = classInfo.PrimaryKeyProperty.Getter.Invoke(obj);

			return Delete<T>()
				.Where
				.PrimaryKey().Is.EqualsTo(primaryKey);
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[MustUseReturnValue]
		public DeleteQuery<T> Delete<T>()
		{
			ContainerObject.Interceptors
				.Add(new EventPostProcessor(EventPostProcessor.EventType.Delete, ContainerObject.AccessLayer));
			var dbClassInfoCache = ContainerObject.AccessLayer.GetClassInfo(typeof(T));
			
			var targetAlias = ContainerObject.CreateTableAlias(dbClassInfoCache.TableName);
			var queryIdentifier = new QueryIdentifier()
			{
				Value = dbClassInfoCache.TableName,
				QueryIdType = QueryIdentifier.QueryIdTypes.Table
			};

			if (ContainerObject.AccessLayer.DbAccessType.HasFlag(DbAccessType.Experimental) ||
			    ContainerObject.AccessLayer.DbAccessType.HasFlag(DbAccessType.Unknown) ||
			    ContainerObject.AccessLayer.DbAccessType.HasFlag(DbAccessType.OleDb) ||
			    ContainerObject.AccessLayer.DbAccessType.HasFlag(DbAccessType.Obdc) ||
			    ContainerObject.AccessLayer.DbAccessType.HasFlag(DbAccessType.SqLite))
			{
				targetAlias.Value = queryIdentifier.Value;
			}

			return new DeleteQuery<T>(
				Add(new DeleteTableQueryPart(queryIdentifier,
					targetAlias, 
					dbClassInfoCache,
					ContainerObject,
					UpdateTableWithQueryPart.ColumsOfType(dbClassInfoCache, targetAlias, queryIdentifier, ContainerObject))));
		}

		/// <summary>
		///		Creates a CTE on the start of the Query
		/// </summary>
		/// <returns></returns>
		public RootQuery WithCte<T>(IElementProducer<T> commandQuery, out QueryIdentifier cteName)
		{
			IQueryBuilder newQuery = new RootQuery(this);
			cteName = newQuery.ContainerObject.CreateAlias(QueryIdentifier.QueryIdTypes.Cte);
			(commandQuery.ContainerObject as IQueryContainerValues)?.TableAlias.Clear();

			var cteQueryPart = commandQuery.ContainerObject.SearchLast<CteDefinitionQueryPart>();
			newQuery = newQuery.Add(cteQueryPart ?? (cteQueryPart = new CteDefinitionQueryPart()));

			var cteInfo = new CteDefinitionQueryPart.CteInfo();
			cteInfo.Name = cteName;
			cteInfo.CteContentParts.AddRange(commandQuery.ContainerObject.Parts);
			return new RootQuery(newQuery.Add(cteQueryPart.AddCte(cteInfo)));
		}

		/// <summary>
		///		Creates a CTE on the start of the Query
		/// </summary>
		/// <returns></returns>
		public RootQuery WithCte<T>(Func<RootQuery, IElementProducer<T>> commandQueryProducer,
			out QueryIdentifier cteName)
		{
			return WithCte(commandQueryProducer(new RootQuery(this)), out cteName);
		}
	}
}