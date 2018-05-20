#region

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Transactions;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Contacts.Pager;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper.LocalDb.Constraints;
using JPB.DataAccess.Helper.LocalDb.Constraints.Collections;
using JPB.DataAccess.Helper.LocalDb.Constraints.Contracts;
using JPB.DataAccess.Helper.LocalDb.Index;
using JPB.DataAccess.Helper.LocalDb.Scopes;
using JPB.DataAccess.Helper.LocalDb.Trigger;
using JPB.DataAccess.Manager;

#endregion

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	///     Maintains a local collection of entitys simulating a basic DB Bevavior
	///     When enumerating the Repro you will only receive the Current state as it is designed to be thread save
	/// </summary>
	/// <remarks>
	///     All Static and Instance member are Thread Save
	/// </remarks>
	[Serializable]
	public sealed class LocalDbRepository<TEntity> : ICollection<TEntity>, ILocalDbReposetoryBaseInternalUsage
	{
		/// <summary>
		///     Creates a new Instance that is bound to &lt;paramref name="type"/&gt; and uses &lt;paramref name="keyGenerator"/
		///     &gt; for generation of PrimaryKeys
		///     Must created inside an DatabaseScope
		/// </summary>
		/// <param name="keyGenerator">The Strategy to generate an uniqe PrimaryKey that matches the PrimaryKey Property</param>
		/// <param name="config">The Config store to use</param>
		/// <param name="useOrignalObjectInMemory">
		///     If enabled the given object referance will be used (Top performance).
		///     if Disabled each object has to be define an Valid Ado.Net constructor to allow a copy (Can be slow)
		/// </param>
		public LocalDbRepository(
			DbConfig config,
			bool useOrignalObjectInMemory = false,
			ILocalDbPrimaryKeyConstraint keyGenerator = null)
		{
			Init(typeof(TEntity), keyGenerator, config, useOrignalObjectInMemory);
		}


		/// <summary>
		///     Creates a new Instance that is bound to &lt;paramref name="type"/&gt; and uses &lt;paramref name="keyGenerator"/
		///     &gt; for generation of PrimaryKeys
		///     Must created inside an DatabaseScope
		/// </summary>
		/// <param name="keyGenerator">The Strategy to generate an uniqe PrimaryKey that matches the PrimaryKey Property</param>
		/// <param name="containedType">The type that overwrites the Generic type. Must use object as Generic type arugment</param>
		/// <param name="config">The Config store to use</param>
		/// <param name="useOrignalObjectInMemory">
		///     If enabled the given object referance will be used (Top performance).
		///     if Disabled each object has to be define an Valid Ado.Net constructor to allow a copy (Can be slow)
		/// </param>
		public LocalDbRepository(
			Type containedType,
			DbConfig config,
			bool useOrignalObjectInMemory = false,
			ILocalDbPrimaryKeyConstraint keyGenerator = null)
		{
			if (typeof(TEntity) != typeof(object))
			{
				throw new InvalidOperationException("When using an contained type argument you must use object as generic type");
			}

			Init(containedType, keyGenerator, config, useOrignalObjectInMemory);
		}

		/// <summary>
		///     Creates a new, only local Reposetory by using one of the Predefined KeyGenerators
		/// </summary>
		public LocalDbRepository()
			: this(new DbConfig(true))
		{
		}

		private readonly List<TransactionalItem<TEntity>> _transactionalItems = new List<TransactionalItem<TEntity>>();
		internal readonly object LockRoot = new object();
		private DbConfig _config;
		private DbReposetoryIdentityInsertScope _currentDbReposetoryIdentityInsertScope;
		private Transaction _currentTransaction;
		private LocalDbManager _databaseDatabase;
		private bool _isMigrating;
		private bool _keepOriginalObject;

		private ITriggerForTableCollectionInternalUsage<TEntity> _triggers;

		private DbClassInfoCache _typeInfo;
		internal IDictionary<object, TEntity> Base;
		internal DbClassInfoCache TypeKeyInfo;
		private IIndexCollectionInteralUsage<TEntity> _indexes;

		/// <summary>
		///     Returns an object with the given Primarykey
		/// </summary>
		/// <param name="primaryKey"></param>
		/// <returns></returns>
		public TEntity this[object primaryKey]
		{
			get
			{
				if (primaryKey == null)
				{
					throw new ArgumentNullException("primaryKey");
				}
				TEntity value;
				if (Base.TryGetValue(primaryKey, out value))
				{
					return value;
				}
				return default(TEntity);
			}
		}

		private ITriggerForTableCollectionInternalUsage<TEntity> TriggersUsage
		{
			get { return _triggers; }
		}

		/// <summary>
		///     Contains acccess to INSERT/DELETE/UPDATE Triggers
		/// </summary>
		public ITriggerForTableCollection<TEntity> Triggers
		{
			get { return _triggers; }
		}

		/// <summary>
		///     Access to a collection of Constraints valid for this Table
		/// </summary>
		public IConstraintCollection<TEntity> Constraints { get; private set; }

		/// <summary>
		/// List of all Indexes on this Table
		/// </summary>
		public IIndexCollection<TEntity> Indexes
		{
			get { return _indexes; }
		}

		private IIndexCollectionInteralUsage<TEntity> IndexesUsage
		{
			get { return _indexes; }
		}

		/// <summary>
		///     The used Config Store
		/// </summary>
		/// <value>
		///     The configuration.
		/// </value>
		public DbConfig Config
		{
			get { return _config; }
		}

		/// <summary>
		///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <value>
		///     Allways false
		/// </value>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public int Count
		{
			get { return Base.Count; }
		}

		/// <summary>
		///     Adds a new Item to the Table
		/// </summary>
		/// <param name="item"></param>
		public void Add(TEntity item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			var elementToAdd = item;
			CheckCreatedElseThrow();
			if (!Contains(elementToAdd))
			{
				var hasTransaction = AttachTransactionIfSet(elementToAdd,
				                                            CollectionStates.Added);
				Constraints.Check.Enforce(elementToAdd);
				Constraints.Unique.Enforce(elementToAdd);
				var id = SetNextId(elementToAdd);

				//Check Data integrity
				if (!hasTransaction)
				{
					var ex = EnforceCheckConstraints(elementToAdd);

					if (ex != null)
					{
						throw ex;
					}
				}
				TriggersUsage.For.OnInsert(elementToAdd);
				Constraints.Default.Enforce(elementToAdd);

				if (!_keepOriginalObject)
				{
					bool fullyLoaded;
					elementToAdd = (TEntity) DbAccessLayer.CreateInstance(
					                                                      _typeInfo,
					                                                      new ObjectDataRecord(item, _config, 0),
					                                                      out fullyLoaded,
					                                                      DbAccessType.Unknown);
					if (!fullyLoaded || elementToAdd == null)
					{
						throw new InvalidOperationException(string.Format("The given type did not provide a Full ado.net constructor " +
						                                                  "and the setting of the propertys did not succeed. " +
						                                                  "Type: '{0}'", typeof(TEntity)));
					}
				}

				if (!TriggersUsage.InsteadOf.OnInsert(elementToAdd))
				{
					Base.Add(id, elementToAdd);
					IndexesUsage.Add(item);
				}
				try
				{
					TriggersUsage.After.OnInsert(elementToAdd);
				}
				catch (Exception)
				{
					Base.Remove(id);
					throw;
				}
				Constraints.Unique.ItemAdded(elementToAdd);
			}
		}

		/// <summary>
		///     Removes all items from this Table
		/// </summary>
		public void Clear()
		{
			CheckCreatedElseThrow();
			lock (LockRoot)
			{
				foreach (var item in Base)
				{
					Remove(item);
				}
			}
		}

		/// <summary>
		///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		///     Does not work if <c>useOrignalObjectInMemory</c> was used to create this Reposetory
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
		///     otherwise, false.
		/// </returns>
		/// <exception cref="ArgumentNullException">item</exception>
		public bool Contains(TEntity item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			CheckCreatedElseThrow();
			var pk = GetId(item);
			var local = Base.Contains(new KeyValuePair<object, TEntity>(pk, item));
			return local;
		}

		/// <summary>
		///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
		///     <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
		/// </summary>
		/// <param name="array">
		///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
		///     from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have
		///     zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="ArgumentNullException">array</exception>
		public void CopyTo(TEntity[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			lock (SyncRoot)
			{
				var values = ToArray();
				values.CopyTo(array, arrayIndex);
			}
		}

		/// <summary>
		///     Removes the given Item based on its PrimaryKey
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(TEntity item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			CheckCreatedElseThrow();
			var success = true;
			lock (LockRoot)
			{
				var id = GetId(item);
				var hasTransaction = AttachTransactionIfSet(item, CollectionStates.Removed);
				TriggersUsage.For.OnDelete(item);
				if (!TriggersUsage.InsteadOf.OnDelete(item))
				{
					success = Base.Remove(id);
				}

				Exception hasInvalidOp = null;
				if (!hasTransaction)
				{
					hasInvalidOp = EnforceCheckConstraints(item);
				}

				try
				{
					if (hasInvalidOp != null)
					{
						throw hasInvalidOp;
					}
					TriggersUsage.After.OnDelete(item);
				}
				catch (Exception)
				{
					Base.Add(id, item);
					throw;
				}
				Constraints.Unique.ItemRemoved(item);
			}
			return success;
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     An enumerator that can be used to iterate through the collection.
		/// </returns>
		IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
		{
			CheckCreatedElseThrow();
			return Base.Values.ToArray().Select(s =>
			                          {
				                          if (_keepOriginalObject)
				                          {
					                          return s;
				                          }
				                          bool fullyLoaded;
				                          return (TEntity) DbAccessLayer.CreateInstance(
				                                                                        _typeInfo,
				                                                                        new ObjectDataRecord(s, _config, 0),
				                                                                        out fullyLoaded,
				                                                                        DbAccessType.Unknown);
			                          }).GetEnumerator();
		}

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator GetEnumerator()
		{
			return ((IEnumerable<TEntity>) this).GetEnumerator();
		}

		/// <summary>
		///     Returns an value that indicates a proper DatabaseScope usage.
		///     If true the creation was successfull and all tables for the this table are mapped
		///     The Reposetory cannot operate if the reposetory is not created!
		/// </summary>
		public bool ReposetoryCreated { get; set; }

		[DebuggerHidden]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ILocalDbReposetoryBaseInternalUsage.ReposetoryCreated
		{
			get { return ReposetoryCreated; }
			set { ReposetoryCreated = value; }
		}

		/// <summary>
		///     Gets the database attached to this Reposetory.
		/// </summary>
		/// <value>
		///     The database.
		/// </value>
		public LocalDbManager Database
		{
			get { return _databaseDatabase; }
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is migrating.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is migrating; otherwise, <c>false</c>.
		/// </value>
		public bool IsMigrating
		{
			get { return _isMigrating; }
			set { _isMigrating = value; }
		}

		/// <summary>
		///     Thread save
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			lock (LockRoot)
			{
				lock (SyncRoot)
				{
					var values = ToArray();
					values.CopyTo(array, index);
				}
			}
		}

		/// <summary>
		///     Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
		/// </summary>
		public object SyncRoot
		{
			get { return LockRoot; }
		}

		/// <summary>
		///     Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized
		///     (thread safe).
		/// </summary>
		public bool IsSynchronized
		{
			get { return Monitor.IsEntered(LockRoot); }
		}

		/// <summary>
		///     Determines whether the specified fk value for table x contains identifier.
		/// </summary>
		/// <param name="fkValueForTableX">The fk value for table x.</param>
		/// <returns>
		///     <c>true</c> if the specified fk value for table x contains identifier; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">fkValueForTableX</exception>
		public bool ContainsId(object fkValueForTableX)
		{
			if (fkValueForTableX == null)
			{
				throw new ArgumentNullException("fkValueForTableX");
			}

			var local = Base.ContainsKey(fkValueForTableX);
			if (!local)
			{
				local = Base.ContainsKey(Convert.ChangeType(fkValueForTableX, _typeInfo.PrimaryKeyProperty.PropertyType));
			}

			return local;
		}

		/// <summary>
		///     Adds the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Add(object item)
		{
			Add((TEntity) item);
		}

		/// <summary>
		///     Determines whether [contains] [the specified item].
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>
		///     <c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(object item)
		{
			return Contains((TEntity) item);
		}

		/// <summary>
		///     Checks if the given primarykey is taken
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(long item)
		{
			return ContainsId(item);
		}

		/// <summary>
		///     Checks if the given primarykey is taken
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(int item)
		{
			return ContainsId(item);
		}

		/// <summary>
		///     Removes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public bool Remove(object item)
		{
			return Remove((TEntity) item);
		}

		/// <summary>
		///     Updates the Entity in memory. Only applies to LocalDbReposetorys that uses Object copys
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Update(object item)
		{
			return Update((TEntity) item);
		}

		/// <summary>
		///     Gets the Generated Type Cache
		/// </summary>
		public DbClassInfoCache TypeInfo
		{
			get { return _typeInfo; }
		}

		/// <summary>
		///     Internal Usage
		/// </summary>
		/// <param name="type"></param>
		/// <param name="keyGenerator"></param>
		/// <param name="config"></param>
		/// <param name="useOrignalObjectInMemory"></param>
		private void Init(Type type,
		                  ILocalDbPrimaryKeyConstraint keyGenerator,
		                  DbConfig config,
		                  bool useOrignalObjectInMemory)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}
			if (_config != null)
			{
				throw new InvalidOperationException("Multibe calls of Init are not supported");
			}

			_keepOriginalObject = useOrignalObjectInMemory;
			_config = config;
			_databaseDatabase = LocalDbManager.Scope;
			if (_databaseDatabase == null)
			{
				throw new NotSupportedException("Please define a new DatabaseScope that allows to seperate" +
				                                " multibe tables of the same type in the same Application");
			}

			_typeInfo = _config.GetOrCreateClassInfoCache(type);
			if (_typeInfo.PrimaryKeyProperty == null)
			{
				throw new NotSupportedException(string.Format("Entitys without any PrimaryKey are not supported. " +
				                                              "Type: '{0}'", type.Name));
			}

			TypeKeyInfo = _config.GetOrCreateClassInfoCache(_typeInfo.PrimaryKeyProperty.PropertyType);

			if (TypeKeyInfo == null)
			{
				throw new NotSupportedException(string.Format("Entitys without any PrimaryKey are not supported. " +
				                                              "Type: '{0}'", type.Name));
			}

			if (!TypeKeyInfo.Type.IsValueType)
			{
				throw new NotSupportedException(string.Format("Entitys without any PrimaryKey that is of " +
				                                              "type of any value-type cannot be used. Type: '{0}'", type.Name));
			}

			if (!_keepOriginalObject)
			{
				if (!TypeInfo.FullFactory)
				{
					TypeInfo.CreateFactory(config);
					if (!TypeInfo.FullFactory)
					{
						throw new NotSupportedException(string.Format("The given type did not provide a Full ado.net constructor " +
						                                              "Type: '{0}'", TypeInfo));
					}
				}
			}

			ILocalDbPrimaryKeyConstraint primaryKeyConstraint;

			if (keyGenerator != null)
			{
				primaryKeyConstraint = keyGenerator;
			}
			else
			{
				ILocalDbPrimaryKeyConstraint defaultKeyGen;
				if (LocalDbManager.DefaultPkProvider.TryGetValue(TypeKeyInfo.Type, out defaultKeyGen))
				{
					primaryKeyConstraint = defaultKeyGen.Clone();
				}
				else
				{
					throw new NotSupportedException(
					string.Format(
					"You must specify ether an Primary key that is of one of this types " +
					"({1}) " +
					"or invoke the ctor with an proper keyGenerator. " +
					"Type: '{0}'",
					type.Name,
					LocalDbManager
							.DefaultPkProvider
							.Keys
							.Select(f => f.Name)
							.Aggregate((e, f) => e + "," + f)));
				}
			}

			Constraints = new ConstraintCollection<TEntity>(this, primaryKeyConstraint);
			_triggers = new TriggerForTableCollection<TEntity>(this);
			_indexes = new IndexCollection<TEntity>();
			Base = new ConcurrentDictionary<object, TEntity>();
			_databaseDatabase.AddTable(this);
			_databaseDatabase.SetupDone += DatabaseDatabaseOnSetupDone;
		}

		private void DatabaseDatabaseOnSetupDone(object sender, EventArgs eventArgs)
		{
			ReposetoryCreated = false;
			lock (LockRoot)
			{
				foreach (var dbPropertyInfoCach in _typeInfo.Propertys)
				{
					if (dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute != null &&
					    dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType != null)
					{
						_databaseDatabase.AddMapping(_typeInfo.Type,
						dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType);
					}
				}

				ReposetoryCreated = true;
			}
		}

		private void CheckCreatedElseThrow()
		{
			if (!ReposetoryCreated && !IsMigrating)
			{
				throw new InvalidOperationException("The database must be completly created until this Table is operational");
			}
		}

		private object SetNextId(object item)
		{
			var idVal = GetId(item);
			if (DbReposetoryIdentityInsertScope.Current != null && _currentDbReposetoryIdentityInsertScope == null)
			{
				_currentDbReposetoryIdentityInsertScope = DbReposetoryIdentityInsertScope.Current;
				_currentDbReposetoryIdentityInsertScope.EnsureTransaction();
			}

			if (_currentDbReposetoryIdentityInsertScope != null)
			{
				if (idVal.Equals(Constraints.PrimaryKey.GetUninitilized()) && !_currentDbReposetoryIdentityInsertScope.RewriteDefaultValues)
				{
					return idVal;
				}
				if (!idVal.Equals(Constraints.PrimaryKey.GetUninitilized()))
				{
					return idVal;
				}
				lock (LockRoot)
				{
					var newId = Constraints.PrimaryKey.GetNextValue();
					_typeInfo.PrimaryKeyProperty.Setter.Invoke(item,
					                                           Convert.ChangeType(newId, _typeInfo.PrimaryKeyProperty.PropertyType));
					return newId;
				}
			}

			if (idVal.Equals(Constraints.PrimaryKey.GetUninitilized()))
			{
				lock (LockRoot)
				{
					var newId = Constraints.PrimaryKey.GetNextValue();
					_typeInfo.PrimaryKeyProperty.Setter.Invoke(item,
					Convert.ChangeType(newId, _typeInfo.PrimaryKeyProperty.PropertyType));
					return newId;
				}
			}

			var exception =
					new InvalidOperationException(string.Format("Cannot insert explicit value for identity column in table '{0}' " +
					                                            "when no IdentityInsertScope exists.", _typeInfo.Name));
			throw exception;
		}

		private object GetId(object item)
		{
			var key = _typeInfo.PrimaryKeyProperty.Getter.Invoke(item);
			if (key == null)
			{
				var exception = new InvalidOperationException(string.Format("The PrimaryKey value '{0}' is null.", key));
				throw exception;
			}
			if (key.GetType() != TypeKeyInfo.Type)
			{
				var exception = new InvalidOperationException(string.Format("The PrimaryKey value '{0}' is invalid.", key));
				throw exception;
			}
			return key;
		}

		private ConstraintException EnforceCheckConstraints(object refItem)
		{
			var refTables = _databaseDatabase.GetMappings(_typeInfo.Type);

			foreach (var localDbReposetory in refTables)
			{
				var fkPropForTypeX =
						_typeInfo.Propertys.FirstOrDefault(
						                                   s =>
							                                   s.Value.ForginKeyDeclarationAttribute != null &&
							                                   s.Value.ForginKeyDeclarationAttribute.Attribute.ForeignTable ==
							                                   localDbReposetory.TypeInfo.TableName)
						         .Value;

				if (fkPropForTypeX == null)
				{
					continue;
				}

				var fkValueForTableX = fkPropForTypeX.Getter.Invoke(refItem);
				if (fkValueForTableX != null && !localDbReposetory.ContainsId(fkValueForTableX))
				{
					return new ForginKeyConstraintException(
					"ForginKey",
					_typeInfo.TableName,
					localDbReposetory.TypeInfo.TableName,
					fkValueForTableX,
					_typeInfo.PrimaryKeyProperty.PropertyName,
					fkPropForTypeX.PropertyName);
				}
			}
			return null;
		}

		private bool AttachTransactionIfSet(TEntity changedItem, CollectionStates action)
		{
			if (Transaction.Current != null)
			{
				if (_currentTransaction == null)
				{
					_currentTransaction = Transaction.Current;
					_currentTransaction.TransactionCompleted += _currentTransaction_TransactionCompleted;
				}

				var hasElement = _transactionalItems.FirstOrDefault(s => s.Item.Equals(changedItem));
				if (hasElement != null)
				{
					if (hasElement.State == CollectionStates.Added)
					{
						if (action == CollectionStates.Removed)
						{
							_transactionalItems.Remove(hasElement);
						}
					}

					if (hasElement.State == CollectionStates.Removed)
					{
						if (action == CollectionStates.Added)
						{
							hasElement.State = CollectionStates.Unchanged;
						}
					}
				}
				else
				{
					_transactionalItems.Add(new TransactionalItem<TEntity>(changedItem, action));
				}

				return true;
			}
			return false;
		}

		private void _currentTransaction_TransactionCompleted(object sender, TransactionEventArgs e)
		{
			lock (LockRoot)
			{
				_currentTransaction = null;
				_currentDbReposetoryIdentityInsertScope = null;
				if (e.Transaction.TransactionInformation.Status == TransactionStatus.Aborted)
				{
					_currentTransaction_Rollback();
				}
				else
				{
					_currentTransaction_TransactionCompleted();
				}
			}
		}

		private void _currentTransaction_Rollback()
		{
			foreach (var transactionalItem in _transactionalItems)
			{
				switch (transactionalItem.State)
				{
					case CollectionStates.Unknown:
					case CollectionStates.Unchanged:
					case CollectionStates.Changed:
						break;
					case CollectionStates.Added:
						Base.Remove(transactionalItem.Item);
						break;
					case CollectionStates.Removed:
						Base.Add(GetId(transactionalItem.Item), transactionalItem.Item);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private void _currentTransaction_TransactionCompleted()
		{
			lock (LockRoot)
			{
				try
				{
					foreach (var transactionalItem in _transactionalItems)
					{
						var checkEnforceConstraints = EnforceCheckConstraints(transactionalItem.Item);
						if (checkEnforceConstraints == null)
						{
							continue;
						}
						try
						{
							throw checkEnforceConstraints;
						}
						finally
						{
							_currentTransaction_Rollback();
						}
					}

					Constraints.PrimaryKey.UpdateIndex(_transactionalItems.Count);
				}
				finally
				{
					_transactionalItems.Clear();
				}
			}
		}

		/// <summary>
		///     Updates the Entity in memory. Only applies to LocalDbReposetorys that uses Object copys
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Update(TEntity item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			TriggersUsage.For.OnUpdate(item);
			Constraints.Check.Enforce(item);
			Constraints.Unique.Enforce(item);
			var getElement = this[GetId(item)];
			if (getElement == null)
			{
				return false;
			}
			if (ReferenceEquals(item, getElement))
			{
				return false;
			}
			if (!TriggersUsage.InsteadOf.OnUpdate(item))
			{
				DataConverterExtensions.CopyPropertys(item, getElement, _config);
			}
			TriggersUsage.After.OnUpdate(item);
			Constraints.Unique.ItemUpdated(item);
			return true;
		}

		/// <summary>
		///     Creates a pager object that can be used to page this collection
		/// </summary>
		/// <returns></returns>
		public IDataPager<TEntity> CreatePager()
		{
			return new LocalDataPager<TEntity>(this);
		}

		/// <summary>
		///     Thread save
		/// </summary>
		/// <returns></returns>
		public TEntity[] ToArray()
		{
			lock (SyncRoot)
			{
				return Base.Values.Select(s =>
				                          {
					                          if (_keepOriginalObject)
					                          {
						                          return s;
					                          }
					                          bool fullyLoaded;
					                          return (TEntity) DbAccessLayer.CreateInstance(
					                                                                        _typeInfo,
					                                                                        new ObjectDataRecord(s, _config, 0),
					                                                                        out fullyLoaded,
					                                                                        DbAccessType.Unknown);
				                          }).ToArray();
			}
		}
	}
}