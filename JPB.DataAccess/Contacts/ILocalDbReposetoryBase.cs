#region

using System.Collections;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper.LocalDb;

#endregion

namespace JPB.DataAccess.Contacts
{
	/// <summary>
	/// </summary>
	/// <seealso cref="System.Collections.ICollection" />
	public interface ILocalDbReposetoryBase : ICollection
	{
		/// <summary>
		///     Gets the database attached to this Reposetory.
		/// </summary>
		/// <value>
		///     The database.
		/// </value>
		LocalDbManager Database { get; }

		/// <summary>
		///     Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		bool IsReadOnly { get; }

		/// <summary>
		///     Gets a value indicating whether the reposetory is fully created.
		/// </summary>
		/// <value>
		///     <c>true</c> if the reposetory is fully created; otherwise, <c>false</c>.
		/// </value>
		bool ReposetoryCreated { get; }

		/// <summary>
		///     Gets the type information for the Entity.
		/// </summary>
		/// <value>
		///     The type information.
		/// </value>
		DbClassInfoCache TypeInfo { get; }

		/// <summary>
		///     Adds the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		void Add(object item);

		/// <summary>
		///     Clears this instance.
		/// </summary>
		void Clear();

		/// <summary>
		///     Determines whether [contains] [the specified item].
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>
		///     <c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
		/// </returns>
		bool Contains(object item);

		/// <summary>
		///     Determines whether [contains] [the specified item].
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>
		///     <c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
		/// </returns>
		bool Contains(long item);

		/// <summary>
		///     Determines whether [contains] [the specified item].
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>
		///     <c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
		/// </returns>
		bool Contains(int item);

		/// <summary>
		///     Determines whether the specified fk value for table x contains identifier.
		/// </summary>
		/// <param name="fkValueForTableX">The fk value for table x.</param>
		/// <returns>
		///     <c>true</c> if the specified fk value for table x contains identifier; otherwise, <c>false</c>.
		/// </returns>
		bool ContainsId(object fkValueForTableX);

		/// <summary>
		///     Removes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		bool Remove(object item);

		/// <summary>
		///     Updates the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		bool Update(object item);
	}
}