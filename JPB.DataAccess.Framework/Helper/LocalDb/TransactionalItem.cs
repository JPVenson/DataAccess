#region

using JPB.DataAccess.Framework.DbCollection;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb
{
	internal class TransactionalItem<TEntity>
	{
		internal TransactionalItem(TEntity item, CollectionStates state)
		{
			Item = item;
			State = state;
		}

		internal TEntity Item { get; set; }
		internal CollectionStates State { get; set; }
	}
}