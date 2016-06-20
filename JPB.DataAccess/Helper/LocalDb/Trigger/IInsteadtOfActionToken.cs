namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public interface IInsteadtOfActionToken<TEntity>
	{
		TEntity Item { get; }
		LocalDbReposetory<TEntity> Table { get; }
	}
}