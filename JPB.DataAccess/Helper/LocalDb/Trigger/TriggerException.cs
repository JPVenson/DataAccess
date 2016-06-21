using System;
using System.Runtime.Serialization;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public class TriggerException<TEntity> : Exception, ITriggerException
	{
		[OptionalField]
		private readonly LocalDbRepository<TEntity> _table;

		public TriggerException(string reason, LocalDbRepository<TEntity> table)
			: base("One trigger rejected the change. See reason.")
		{
			Reason = reason;
			_table = table;
		}

		public string Reason { get; private set; }

		public LocalDbRepository<TEntity> Table
		{
			get { return _table; }
		}
	}
}