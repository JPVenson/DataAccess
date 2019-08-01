#region

using JPB.DataAccess.Framework.Contacts;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Contracts
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="ILocalDbConstraint" />
	public interface ILocalDbDefaultConstraint<in TEntity> : ILocalDbConstraint
	{
		/// <summary>
		///     Defaults the value.
		/// </summary>
		/// <param name="item">The item.</param>
		void DefaultValue(TEntity item);
	}
}