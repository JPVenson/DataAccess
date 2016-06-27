using System;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;

namespace JPB.DataAccess.Query.Operators
{
	public class UpdateQuery<TPoco> : QueryBuilderX, IUpdateQuery<TPoco>
	{
		public UpdateQuery(IQueryBuilder database, string currentIdentifier) : base(database)
		{
			CurrentIdentifier = currentIdentifier;
		}

		public UpdateQuery(IQueryBuilder database) : base(database)
		{
			CurrentIdentifier = string.Format("{0}_{1}", this.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).TableName, base.ContainerObject.GetNextParameterId());
		}

		public UpdateQuery<TPoco> Alias(string alias)
		{
			if (alias == null) throw new ArgumentNullException("alias");
			return new UpdateQuery<TPoco>(this, alias);
		}

		/// <summary>
		///     Adds a SQL WHERE statement
		///		does not emit any conditional statement
		///		should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Where()
		{
			return new ConditionalQuery<TPoco>(this.QueryText("WHERE"), new CondtionBuilderState(CurrentIdentifier));
		}

		public string CurrentIdentifier { get; private set; }
	}
}