namespace JPB.DataAccess.Helper.LocalDb.Trigger
{
	public interface IInsteadtOfActionToken<TEntity>
	{
		TEntity Item { get; }
		LocalDbRepository<TEntity> Table { get; }
	}
}