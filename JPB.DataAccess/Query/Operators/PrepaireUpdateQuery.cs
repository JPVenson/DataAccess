using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	public class PrepaireUpdateQuery : QueryBuilderX
	{
		public PrepaireUpdateQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		public PrepaireUpdateQuery(IQueryContainer database) : base(database)
		{
		}

		public PrepaireUpdateQuery(IQueryBuilder database) : base(database)
		{
		}

		public PrepaireUpdateQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		public PrepaireUpdateQuery(DbAccessLayer database) : base(database)
		{
		}

		/// <summary>
		///     Creates a Select statement for a given Poco
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <param name="argumentsForFactory">The arguments for factory.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">argumentsForFactory</exception>
		public UpdateColumnSetters<TPoco> Table<TPoco>()
		{
			var tableName = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).TableName;
			return new UpdateColumnSetters<TPoco>(this.QueryText("UPDATE {0}", tableName));
		}
	}
}