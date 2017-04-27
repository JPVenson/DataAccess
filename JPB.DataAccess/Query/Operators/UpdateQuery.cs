#region

using System;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IUpdateQuery{TPoco}" />
	public class UpdateQuery<TPoco> : QueryBuilderX, IUpdateQuery<TPoco>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="UpdateQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="currentIdentifier">The current identifier.</param>
		public UpdateQuery(IQueryBuilder database, string currentIdentifier) : base(database)
		{
			CurrentIdentifier = currentIdentifier;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="UpdateQuery{TPoco}" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public UpdateQuery(IQueryBuilder database) : base(database)
		{
			CurrentIdentifier = string.Format("{0}_{1}", ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).TableName,
				ContainerObject.GetNextParameterId());
		}

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

		//public ConditionalQuery<TPoco> Where()
		//{
		//	return new ConditionalQuery<TPoco>(this.QueryText("WHERE"), new CondtionBuilderState(CurrentIdentifier));
		//}

		/// <summary>
		///     Gets the current identifier.
		/// </summary>
		/// <value>
		///     The current identifier.
		/// </value>
		public string CurrentIdentifier { get; private set; }

		/// <summary>
		///     Changes the generated Identifier
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">alias</exception>
		public UpdateQuery<TPoco> Alias(string alias)
		{
			if (alias == null) throw new ArgumentNullException("alias");
			return new UpdateQuery<TPoco>(this, alias);
		}
	}
}