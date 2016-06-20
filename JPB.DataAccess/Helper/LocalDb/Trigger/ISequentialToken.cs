namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public interface ISequentialToken<out TEntity>
	{
		bool Canceled { get; }
		TEntity Item { get; }
		string Reason { get; }

		void Cancel(string reason);
	}
}