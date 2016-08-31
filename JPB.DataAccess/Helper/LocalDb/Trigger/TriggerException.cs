using System;
using System.Runtime.Serialization;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	/// <summary>
	/// Is used to indicate the negative value of an trigger check
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="System.Exception" />
	/// <seealso cref="JPB.DataAccess.Helper.LocalDb.Trigger.ITriggerException" />
	public class TriggerException<TEntity> : Exception, ITriggerException
	{
		[OptionalField]
		private readonly LocalDbRepository<TEntity> _table;

		/// <summary>
		/// Initializes a new instance of the <see cref="TriggerException{TEntity}"/> class.
		/// </summary>
		/// <param name="reason">The reason.</param>
		/// <param name="table">The table.</param>
		public TriggerException(string reason, LocalDbRepository<TEntity> table)
			: base("One trigger rejected the change. See reason.")
		{
			Reason = reason;
			_table = table;
		}

		/// <summary>
		/// Gets the reason.
		/// </summary>
		/// <value>
		/// The reason or null.
		/// </value>
		public string Reason { get; private set; }


		/// <summary>
		/// Gets the attached table to this exception.
		/// </summary>
		/// <value>
		/// The table.
		/// </value>
		public LocalDbRepository<TEntity> Table
		{
			get { return _table; }
		}
	}
}