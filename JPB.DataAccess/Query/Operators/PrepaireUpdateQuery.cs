using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///		Select a Target for an Update query
	/// </summary>
	public class PrepaireUpdateQuery : QueryBuilderX
	{
		/// <inheritdoc />
		public PrepaireUpdateQuery(IQueryContainer database) : base(database)
		{
		}

		/// <inheritdoc />
		public PrepaireUpdateQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     Creates a Update statement for a given type
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <returns></returns>
		public UpdateColumnSetters<TPoco> Table<TPoco>()
		{
			var dbClassInfoCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
			return new UpdateColumnSetters<TPoco>(
				this.Add(new UpdateTableWithQueryPart(
					new QueryIdentifier()
					{
						Value = dbClassInfoCache.TableName
					}, 
					ContainerObject.CreateTableAlias(dbClassInfoCache.TableName))));
		}
	}
}