using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///		Select a Target for an Update query
	/// </summary>
	public class PrepaireUpdateQuery : QueryBuilderX
	{
		/// <inheritdoc />
		public PrepaireUpdateQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public PrepaireUpdateQuery(IQueryContainer database) : base(database)
		{
		}

		/// <inheritdoc />
		public PrepaireUpdateQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <inheritdoc />
		public PrepaireUpdateQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public PrepaireUpdateQuery(DbAccessLayer database) : base(database)
		{
		}

		/// <summary>
		///     Creates a Update statement for a given type
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <returns></returns>
		public UpdateColumnSetters<TPoco> Table<TPoco>()
		{
			var tableName = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).TableName;
			return new UpdateColumnSetters<TPoco>(this.QueryText("UPDATE {0}", tableName));
		}
	}
}