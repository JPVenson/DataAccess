using System;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public class InsteadtOfActionToken : EventArgs
	{
		public InsteadtOfActionToken(object item, LocalDbReposetoryBase table)
		{
			Item = item;
			Table = table;
		}

		public object Item { get; private set; }
		public LocalDbReposetoryBase Table { get; private set; }
	}
}