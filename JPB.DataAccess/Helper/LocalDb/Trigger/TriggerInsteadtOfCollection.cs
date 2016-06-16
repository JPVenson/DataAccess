using System;

namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public class TriggerInsteadtOfCollection
	{
		private readonly LocalDbReposetoryBase _tabel;

		public TriggerInsteadtOfCollection(LocalDbReposetoryBase tabel)
		{
			_tabel = tabel;
		}

		public TriggerInsteadtOfCollection()
		{
		}

		public event EventHandler<InsteadtOfActionToken> Insert;
		public event EventHandler<InsteadtOfActionToken> Update;
		public event EventHandler<InsteadtOfActionToken> Delete;

		[ThreadStatic]
		internal static bool AsInsteadtOf;

		private bool InvokeTrigger(EventHandler<InsteadtOfActionToken> trigger, object obj)
		{
			if(_tabel == null)
				return false;

			var token = new InsteadtOfActionToken(obj, _tabel);
			if (trigger != null)
			{
				try
				{
					AsInsteadtOf = true;
					trigger(this, token);
				}
				finally
				{
					AsInsteadtOf = false;
				}
				return true;
			}
			return false;
		}

		internal virtual bool OnInsert(object obj)
		{
			return InvokeTrigger(Insert, obj);
		}

		internal virtual bool OnUpdate(object obj)
		{
			return InvokeTrigger(Update, obj);
		}

		internal virtual bool OnDelete(object obj)
		{
			return InvokeTrigger(Delete, obj);
		}

		public static TriggerInsteadtOfCollection Empty()
		{
			return new TriggerInsteadtOfCollection();
		}
	}
}