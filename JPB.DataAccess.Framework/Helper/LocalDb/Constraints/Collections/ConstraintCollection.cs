#region

using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Contracts;

#endregion

namespace JPB.DataAccess.Framework.Helper.LocalDb.Constraints.Collections
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="IConstraintCollection{TEntity}" />
	public class ConstraintCollection<TEntity> : IConstraintCollection<TEntity>
	{
		/// <summary>
		///     The local database repository
		/// </summary>
		private readonly LocalDbRepository<TEntity> _localDbRepository;

		/// <summary>
		///     Initializes a new instance of the <see cref="ConstraintCollection{TEntity}" /> class.
		/// </summary>
		/// <param name="localDbRepository">The local database repository.</param>
		/// <param name="primaryKey">The primary key.</param>
		internal ConstraintCollection(
			LocalDbRepository<TEntity> localDbRepository,
			ILocalDbPrimaryKeyConstraint primaryKey)
		{
			_localDbRepository = localDbRepository;
			PrimaryKey = primaryKey;
			Unique = new UniqueConstrains<TEntity>(localDbRepository);
			Default = new DefaultConstraints<TEntity>(localDbRepository);
			Check = new CheckConstraints<TEntity>(localDbRepository);
		}

		/// <summary>
		///     Contains a list of Constrains to ensure all Items have an Index that is Unique
		/// </summary>
		/// <value>
		///     The unique.
		/// </value>
		public IUniqueConstrains<TEntity> Unique { get; private set; }

		/// <summary>
		///     Contains a list of all Constrains to define a Default value on a certain Column
		/// </summary>
		/// <value>
		///     The default.
		/// </value>
		public IDefaultConstraints<TEntity> Default { get; private set; }

		/// <summary>
		///     Contains a list of all Constrains that checks one or more Columns
		/// </summary>
		/// <value>
		///     The check.
		/// </value>
		public ICheckConstraints<TEntity> Check { get; private set; }

		/// <summary>
		///     Gets the primary key constraints.
		/// </summary>
		/// <value>
		///     The primary key.
		/// </value>
		public ILocalDbPrimaryKeyConstraint PrimaryKey { get; private set; }
	}
}