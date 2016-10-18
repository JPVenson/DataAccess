using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper.LocalDb.Constraints.Defaults;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <seealso cref="JPB.DataAccess.Helper.LocalDb.Constraints.Collections.IConstraintCollection{TEntity}" />
	public class ConstraintCollection<TEntity> : IConstraintCollection<TEntity>
	{
		/// <summary>
		/// The local database repository
		/// </summary>
		private readonly LocalDbRepository<TEntity> _localDbRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstraintCollection{TEntity}"/> class.
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
		/// Contains a list of Constrains to ensure all Items have an Index that is Unique
		/// </summary>
		/// <value>
		/// The unique.
		/// </value>
		public IUniqueConstrains<TEntity> Unique { get; private set; }

		/// <summary>
		/// Contains a list of all Constrains to define a Default value on a certain Column
		/// </summary>
		/// <value>
		/// The default.
		/// </value>
		public IDefaultConstraints<TEntity> Default { get; private set; }

		/// <summary>
		/// Contains a list of all Constrains that checks one or more Columns
		/// </summary>
		/// <value>
		/// The check.
		/// </value>
		public ICheckConstraints<TEntity> Check { get; private set; }

		/// <summary>
		/// Gets the primary key constraints.
		/// </summary>
		/// <value>
		/// The primary key.
		/// </value>
		public ILocalDbPrimaryKeyConstraint PrimaryKey { get; private set; }
	}

	//public class ConstraintCollection<TUnique, TDefault, TCheck, TEntity>
	//	where TUnique : IUniqueConstrains<TEntity>
	//	where TDefault : IDefaultConstraints<TEntity>
	//	where TCheck : ICheckConstraints<TEntity>

	//{
	//	private readonly LocalDbReposetoryBase _localDbReposetoryBase;

	//	internal ConstraintCollection(
	//		LocalDbReposetoryBase localDbReposetoryBase,
	//		TUnique uniqueConstraints,
	//		TDefault defaultCOnstrains,
	//		TCheck checkConstraints,
	//		ILocalDbPrimaryKeyConstraint primaryKey)
	//	{
	//		_localDbReposetoryBase = localDbReposetoryBase;
	//		PrimaryKey = primaryKey;
	//		Unique = uniqueConstraints;
	//		Default = defaultCOnstrains;
	//		Check = checkConstraints;
	//	}

	//	/// <summary>
	//	/// Contains a list of Constrains to ensure all Items have an Index that is Unique
	//	/// </summary>
	//	public TUnique Unique { get; private set; }

	//	/// <summary>
	//	/// Contains a list of all Constrains to define a Default value on a certain Column
	//	/// </summary>
	//	public TDefault Default { get; private set; }

	//	/// <summary>
	//	/// Contains a list of all Constrains that checks one or more Columns
	//	/// </summary>
	//	public TCheck Check { get; private set; }

	//	public ILocalDbPrimaryKeyConstraint PrimaryKey { get; private set; }
	//}

	//public class ConstraintCollectionImpl<TEntity>
	//	: ConstraintCollection<IUniqueConstrains<TEntity>, IDefaultConstraints<TEntity>, ICheckConstraints<TEntity>, TEntity>
	//{
	//	public ConstraintCollectionImpl(LocalDbReposetoryBase localDbReposetoryBase,
	//	ILocalDbPrimaryKeyConstraint primaryKey)
	//		: base(localDbReposetoryBase,
	//			  new UniqueConstrains<TEntity>(localDbReposetoryBase),
	//			  new DefaultConstraints<TEntity>(localDbReposetoryBase),
	//			  new CheckConstraints<TEntity>(localDbReposetoryBase),
	//			  primaryKey)
	//	{
	//	}
	//}

	//public class ConstraintCollection
	//	: ConstraintCollection<IUniqueConstrains<object>, IDefaultConstraints<object>, ICheckConstraints<object>, object>
	//{
	//	public ConstraintCollection(LocalDbReposetoryBase localDbReposetoryBase,
	//		ILocalDbPrimaryKeyConstraint primaryKey)
	//		: base(localDbReposetoryBase,
	//			  new UniqueConstrains<object>(localDbReposetoryBase),
	//			  new DefaultConstraints<object>(localDbReposetoryBase),
	//			  new CheckConstraints<object>(localDbReposetoryBase),
	//			  primaryKey)
	//	{
	//	}
	//}
}