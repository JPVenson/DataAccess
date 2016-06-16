using System;
using System.Runtime.Serialization;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public class TriggerException : Exception
	{
		[OptionalField]
		private readonly LocalDbReposetoryBase _table;

		public TriggerException(string reason, LocalDbReposetoryBase table)
			: base("One trigger rejected the change. See reason.")
		{
			Reason = reason;
			_table = table;
		}

		public string Reason { get; private set; }

		public LocalDbReposetoryBase Table
		{
			get { return _table; }
		}
	}
}