#region

using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IElementProducer{TPoco}" />
	/// <seealso cref="System.Collections.Generic.IEnumerable{TPoco}" />
	public class ElementProducer<TPoco> : ElementResultQuery<TPoco>
	{
		/// <inheritdoc />
		public ElementProducer(IQueryBuilder database, string currentIdentifier) : base(database, currentIdentifier)
		{
		}

		/// <inheritdoc />
		public ElementProducer(IQueryBuilder database) : base(database)
		{
		}


		/// <summary>
		///     Adds a SQL WHERE statement
		///     does not emit any conditional statement
		///     should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public virtual ConditionalQuery<TPoco> Where
		{
			get { return new ConditionalQuery<TPoco>(this.QueryText("WHERE"), new CondtionBuilderState(CurrentIdentifier)); }
		}
	}
}