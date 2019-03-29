#region

using System;
using System.Data;
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
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IRootQuery" />
	public class RootQuery : QueryBuilderX, IRootQuery
	{
		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(IQueryContainer database) : base(database)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		public RootQuery(DbAccessLayer database) : base(database)
		{
		}

		/// <summary>
		/// Changes the ResultType property in a Fluid syntax
		/// </summary>
		/// <param name="resultType"></param>
		/// <returns></returns>
		[MustUseReturnValue]
		public RootQuery ConfigType(Type resultType)
		{
			if (resultType == null)
			{
				throw new ArgumentNullException(nameof(resultType));
			}
			ContainerObject.ForType = resultType;
			return this;
		}

		/// <summary>
		/// Changes the AllowParamterRenaming flag in a Fluid syntax
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		[MustUseReturnValue]
		public RootQuery ConfigAllowParamterRenaming(bool mode)
		{
			ContainerObject.AllowParamterRenaming = mode;
			return this;
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
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create an update statement that will check for the id of the obj
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[MustUseReturnValue]
		public ConditionalEvalQuery<T> UpdateEntity<T>(T obj)
		{
			ContainerObject.PostProcessors
				.Add(new EventPostProcessor(EventPostProcessor.EventType.Update, ContainerObject.AccessLayer));
			var cache = ContainerObject.AccessLayer.GetClassInfo(typeof(T));
			var query = Add(new UpdateTableWithQueryPart(new QueryIdentifier()
			{
				Value = cache.TableName
			}, ContainerObject.CreateAlias(QueryIdentifier.QueryIdTypes.Table)));
			return new ElementProducer<T>(query).Where.PrimaryKey().Is.EqualsTo(cache.PrimaryKeyProperty.Getter.Invoke(obj));
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
			ContainerObject.PostProcessors
				.Add(new EventPostProcessor(EventPostProcessor.EventType.Delete, ContainerObject.AccessLayer));
			var cache = ContainerObject.AccessLayer.GetClassInfo(typeof(T));
			return new DeleteQuery<T>(
				Add(new DeleteTableQueryPart(new QueryIdentifier() { Value = cache.TableName },
					ContainerObject.CreateAlias(QueryIdentifier.QueryIdTypes.Table))));
		}

		/// <summary>
		///		Creates a CTE on the start of the Query
		/// </summary>
		/// <returns></returns>
		public RootQuery WithCte<T>(IElementProducer<T> commandQuery, out QueryIdentifier cteName)
		{
			IQueryBuilder newQuery = new RootQuery(this);
			cteName = newQuery.ContainerObject.CreateAlias(QueryIdentifier.QueryIdTypes.Cte);

			var cteQueryPart = commandQuery.ContainerObject.Search<CteDefinitionQueryPart>();
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

	internal class EventPostProcessor : EntityProcessorBase
	{
		private readonly EventType _handler;
		private readonly DbAccessLayer _source;

		internal enum EventType
		{
			Select,
			Insert,
			Delete,
			Update,
			Non
		}

		internal EventPostProcessor(EventType handler, DbAccessLayer source)
		{
			_handler = handler;
			_source = source;
		}

		public override IDbCommand BeforeExecution(IDbCommand command)
		{
			switch (_handler)
			{
				case EventType.Select:
					_source.RaiseSelect(command);
					break;
				case EventType.Insert:
					_source.RaiseInsert(this, command);
					break;
				case EventType.Delete:
					_source.RaiseDelete(this, command);
					break;
				case EventType.Update:
					_source.RaiseUpdate(this, command);
					break;
				case EventType.Non:
					_source.RaiseNoResult(this, command);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return base.BeforeExecution(command);
		}
	}
}