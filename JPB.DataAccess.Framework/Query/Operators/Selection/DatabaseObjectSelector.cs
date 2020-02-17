#region

using System;
using System.Linq;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.Operators.Orders;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators.Selection
{
	/// <summary>
	/// </summary>
	/// <seealso cref="QueryBuilderX" />
	/// <seealso cref="IDbElementSelector" />
	public class DatabaseObjectSelector : QueryBuilderX, IDbElementSelector
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseObjectSelector" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public DatabaseObjectSelector(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///		Returns a Query that is will skip N items and return M items
		/// </summary>
		/// <returns></returns>
		public SelectQuery<TPoco> AsPagedTable<TPoco>(int page, int pageSize, 
			Func<OrderStatementQuery<TPoco>, OrderByColumn<TPoco>> ordering)
		{
			var withCte = new RootQuery(this)
				.WithCte(f =>
				{
					var orderByColumn = ordering(f.Select.Table<TPoco>().Order);
					var query = orderByColumn.Add(new PaginationPart()
					{
						Page = page,
						PageSize = pageSize
					});
					return query;
				}, out var pagedCte);
			return withCte.Select.Identifier<TPoco>(pagedCte);

			//return new SelectQuery<TPoco>(Add(new SelectTableQueryPart(
			//	classInfo.TableName,
			//	classInfo,
			//	ContainerObject.CreateTableAlias(classInfo.TableName), 
			//	ContainerObject)));



			//var rootSelect = ContainerObject.SearchFirst<SelectTableQueryPart>(f => true);
			//var orderPart = ContainerObject.SearchLast<OrderByColumnQueryPart>();

			//QueryIdentifier rnAlias = null;

			//var rootQuery = new RootQuery(ContainerObject.AccessLayer);
			//rootQuery = rootQuery.WithCte(e =>
			//{
			//	return e.Select.Table<TPoco>()
			//		.SynteticColumn(
			//			"ROW_NUMBER() OVER (ORDER BY " + orderPart.Columns.First().ColumnSourceAlias() + ")",
			//			out rnAlias);
			//}, out var pagedCte);

			//var q = rootQuery
			//	.WithCte(this, out var valueCte)
			//	.Select
			//	.Identifier<TPoco>(valueCte)
			//	.Join(pagedCte, JoinMode.Inner)
			//	.Where
			//	.Column(rnAlias).Is.Between((page - 1) * pageSize, (page) * pageSize)
			//	.Order.By(rnAlias);
			//return rootQuery;
			//return new OrderByColumn<TPoco>(Add(new PaginationPart()
			//{
			//	Page = page,
			//	PageSize = pageSize
			//}));
		}

		/// <summary>
		///     Creates a Select statement for a given Poco
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">argumentsForFactory</exception>
		public SelectQuery<TPoco> Table<TPoco>()
		{
			ContainerObject.Interceptors
				.Add(new EventPostProcessor(EventPostProcessor.EventType.Select, ContainerObject.AccessLayer));
			var classInfo = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			return new SelectQuery<TPoco>(Add(new SelectTableQueryPart(
				classInfo.TableName,
				classInfo,
				ContainerObject.CreateTableAlias(classInfo.TableName), 
				ContainerObject)));
		}

		/// <summary>
		///		Selects all columns from the given Identifier
		/// </summary>
		/// <returns></returns>
		public SelectQuery<TPoco> Identifier<TPoco>(QueryIdentifier identifier)
		{
			ContainerObject.Interceptors.Add(new EventPostProcessor(EventPostProcessor.EventType.Select, ContainerObject.AccessLayer));
			var classInfo = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			return new SelectQuery<TPoco>(Add(new SelectTableQueryPart(
				ContainerObject.Search(identifier),
				classInfo,
				ContainerObject.CreateTableAlias(classInfo.TableName), ContainerObject)));
		}
	}
}