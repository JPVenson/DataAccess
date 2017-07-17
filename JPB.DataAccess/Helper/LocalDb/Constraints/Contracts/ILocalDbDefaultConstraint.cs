#region

using JPB.DataAccess.Contacts;

#endregion

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Contracts
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="JPB.DataAccess.Contacts.ILocalDbConstraint" />
	public interface ILocalDbDefaultConstraint<in TEntity> : ILocalDbConstraint
	{
		/// <summary>
		///     Defaults the value.
		/// </summary>
		/// <param name="item">The item.</param>
		void DefaultValue(TEntity item);
	}
}