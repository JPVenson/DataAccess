using System;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public class ForActionCancelToken : EventArgs
	{
		public object Item { get; private set; }
		public bool Canceled { get; private set; }
		public string Reason { get; private set; }

		public ForActionCancelToken(object item)
		{
			Item = item;
		}

		public void Cancel(string reason)
		{
			this.Reason = reason;
			this.Canceled = true;
		}
	}
}