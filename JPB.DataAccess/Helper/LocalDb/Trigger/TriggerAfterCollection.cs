namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public class TriggerAfterCollection : TriggerForCollection
	{
		public TriggerAfterCollection(LocalDbReposetoryBase tabel, TriggerForCollection duplication = null) 
			: base(tabel, duplication)
		{
		}
	}
}