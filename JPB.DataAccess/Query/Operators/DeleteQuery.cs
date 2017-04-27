using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;

namespace JPB.DataAccess.Query.Operators
{
	public class DeleteQuery<TPoco> : QueryBuilderX
	{
		public DeleteQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		public DeleteQuery(IQueryContainer database) : base(database)
		{
		}

		public DeleteQuery(IQueryBuilder database) : base(database)
		{
		}

		public DeleteQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		public DeleteQuery(DbAccessLayer database) : base(database)
		{
		}


		/// <summary>
		///     Gets the current identifier.
		/// </summary>
		/// <value>
		///     The current identifier.
		/// </value>
		public string CurrentIdentifier { get; private set; }

		/// <summary>
		///     Adds a SQL WHERE statement
		///     does not emit any conditional statement
		///     should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Where
		{
			get { return new ConditionalQuery<TPoco>(this.QueryText("WHERE"), new CondtionBuilderState(CurrentIdentifier)); }
		}
	}
}
