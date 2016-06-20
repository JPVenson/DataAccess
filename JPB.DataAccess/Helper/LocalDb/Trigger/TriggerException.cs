using System;
using System.Runtime.Serialization;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public class TriggerException<TEntity> : Exception, ITriggerException
	{
		[OptionalField]
		private readonly LocalDbReposetory<TEntity> _table;

		public TriggerException(string reason, LocalDbReposetory<TEntity> table)
			: base("One trigger rejected the change. See reason.")
		{
			Reason = reason;
			_table = table;
		}

		public string Reason { get; private set; }

		public LocalDbReposetory<TEntity> Table
		{
			get { return _table; }
		}
	}
}