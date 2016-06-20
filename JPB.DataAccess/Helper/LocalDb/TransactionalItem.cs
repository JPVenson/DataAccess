using JPB.DataAccess.DbCollection;

namespace JPB.DataAccess.Helper.LocalDb
{
	internal class TransactionalItem<TEntity>
	{
		internal TEntity Item { get; set; }
		internal CollectionStates State { get; set; }

		internal TransactionalItem(TEntity item, CollectionStates state)
		{
			Item = item;
			State = state;
		}
	}
}