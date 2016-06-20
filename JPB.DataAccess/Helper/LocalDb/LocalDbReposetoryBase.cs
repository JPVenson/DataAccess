using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Transactions;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Helper.LocalDb.Constraints;
using JPB.DataAccess.Helper.LocalDb.Constraints.Collections;
using JPB.DataAccess.Helper.LocalDb.Scopes;
using JPB.DataAccess.Helper.LocalDb.Trigger;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	///     Maintains a local collection of entitys simulating a basic DB Bevavior by setting PrimaryKeys in an General way.
	///     Starting with 0 incriment by 1.
	///     When enumerating the Repro you will only receive the Current state as it is designed to be thread save
	/// </summary>
	[Serializable]
	public class LocalDbReposetory<TEntity> : ICollection<TEntity>, ILocalDbReposetoryBaseInternalUsage
	{
		private readonly List<TransactionalItem<TEntity>> _transactionalItems = new List<TransactionalItem<TEntity>>();
		protected internal readonly DbAccessLayer Db;
		protected internal readonly object LockRoot = new object();
		private DbConfig _config;
		private IdentityInsertScope _currentIdentityInsertScope;
		private Transaction _currentTransaction;
		private LocalDbManager _databaseDatabase;
		private bool _isMigrating;
		private bool _keepOriginalObject;

		private
			ITriggerForTableCollectionInternalUsage<TEntity>
			_triggers;

		private DbClassInfoCache _typeInfo;
		protected internal IDictionary<object, TEntity> Base;
		protected internal DbClassInfoCache TypeKeyInfo;


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
		/// <param name="triggerProto">The given trigger collection</param>
		public LocalDbReposetory(
			DbConfig config,
			bool useOrignalObjectInMemory = true,
			ILocalDbPrimaryKeyConstraint keyGenerator = null)
		{
			Init(typeof(TEntity), keyGenerator, config, useOrignalObjectInMemory);
		}

		/// <summary>
		///     Creates a new, only local Reposetory by using one of the Predefined KeyGenerators
		/// </summary>
		protected LocalDbReposetory()
			: this(new DbConfig())
		{
		}

		/// <summary>
		///     Creates a new, database as fallback using batabase
		/// </summary>
		/// <param name="db"></param>
		/// <param name="type"></param>
		protected LocalDbReposetory(DbAccessLayer db)
			: this()
		{
			Db = db;
		}

		/// <summary>
		///     Returns an object with the given Primarykey
		/// </summary>
		/// <param name="primaryKey"></param>
		/// <returns></returns>
		public TEntity this[object primaryKey]
		{
			get
			{
				TEntity value;
				if (Base.TryGetValue(primaryKey, out value))
					return value;
				return default(TEntity);
			}
		}

		private
			ITriggerForTableCollectionInternalUsage<TEntity>
			TriggersUsage
		{
			get { return _triggers; }
		}

		/// <summary>
		///     Contains acccess to INSERT/DELETE/UPDATE(WIP) Triggers
		/// </summary>
		public virtual ITriggerForTableCollection<TEntity>
			Triggers
		{
			get { return _triggers; }
		}

		/// <summary>
		///     Access to a collection of Constraints valid for this Table
		/// </summary>
		public virtual ConstraintCollection<TEntity> Constraints { get; private set; }

		public virtual bool IsReadOnly
		{
			get { return false; }
		}


		public virtual int Count
		{
			get { return Base.Count; }
		}

		/// <summary>
		///     Adds a new Item to the Table
		/// </summary>
		/// <param name="item"></param>
		public virtual void Add(TEntity item)
		{
			var elementToAdd = item;
			CheckCreatedElseThrow();
			if (Db != null)
			{
				TriggersUsage.For.OnInsert(elementToAdd);
				if (!TriggersUsage.InsteadOf.OnInsert(elementToAdd))
					Db.Insert(elementToAdd);
				TriggersUsage.After.OnInsert(elementToAdd);
			}
			else
			{
				if (!Contains(elementToAdd))
				{
					AttachTransactionIfSet(elementToAdd,
						CollectionStates.Added,
						true);
					Constraints.Check.Enforce(elementToAdd);
					Constraints.Unique.Enforce(elementToAdd);
					TriggersUsage.For.OnInsert(elementToAdd);
					var id = SetNextId(elementToAdd);
					Constraints.Default.Enforce(elementToAdd);
					if (!_keepOriginalObject)
					{
						bool fullyLoaded;
						elementToAdd = (TEntity) DbAccessLayer.CreateInstance(
							_typeInfo,
							new ObjectDataRecord(item, _config, 0),
							out fullyLoaded,
							DbAccessType.Unknown);
						if (!fullyLoaded)
						{
							throw new InvalidOperationException(string.Format("The given type did not provide a Full ado.net constructor " +
							                                                  "and the setting of the propertys did not succeed. " +
							                                                  "Type: '{0}'", elementToAdd.GetType()));
						}
					}
					if (!TriggersUsage.InsteadOf.OnInsert(elementToAdd))
					{
						Base.Add(id, elementToAdd);
					}
					try
					{
						TriggersUsage.After.OnInsert(elementToAdd);
					}
					catch (Exception e)
					{
						Base.Remove(id);
						throw e;
					}
					Constraints.Unique.ItemAdded(elementToAdd);
				}
			}
		}

		/// <summary>
		///     Removes all items from this Table
		/// </summary>
		public virtual void Clear()
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

		public bool Contains(TEntity item)
		{
			CheckCreatedElseThrow();
			var pk = GetId(item);
			var local = Base.Contains(new KeyValuePair<object, TEntity>(pk, (TEntity) item));
			if (!local && Db != null)
			{
				return Db.Select(_typeInfo.Type, pk) != null;
			}

			return local;
		}

		public void CopyTo(TEntity[] array, int arrayIndex)
		{
			lock (SyncRoot)
			{
				var values = ToArray();
				values.CopyTo(array, arrayIndex);
			}
		}

		public bool Remove(TEntity item)
		{
			CheckCreatedElseThrow();
			var id = GetId(item);
			var success = true;
			lock (LockRoot)
			{
				TriggersUsage.For.OnDelete(item);
				if (!TriggersUsage.InsteadOf.OnDelete(item))
					success = Base.Remove(id);
				var hasInvalidOp = AttachTransactionIfSet(item, CollectionStates.Removed);
				try
				{
					TriggersUsage.After.OnDelete(item);
				}
				catch (Exception e)
				{
					hasInvalidOp = ExceptionDispatchInfo.Capture(e);
				}
				if (hasInvalidOp != null)
				{
					Base.Add(id, item);
					hasInvalidOp.Throw();
				}
				Constraints.Unique.ItemRemoved(item);
			}

			if (!success && Db != null)
			{
				TriggersUsage.For.OnDelete(item);
				Db.Delete(item);
				TriggersUsage.After.OnDelete(item);
				success = true;
			}
			return success;
		}

		IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
		{
			CheckCreatedElseThrow();
			if (Db != null)
				return Db.Select<TEntity>().Cast<TEntity>().GetEnumerator();

			return Base.Values.Select(s =>
			{
				if (_keepOriginalObject)
					return s;
				bool fullyLoaded;
				return (TEntity)DbAccessLayer.CreateInstance(
					_typeInfo,
					new ObjectDataRecord(s, _config, 0),
					out fullyLoaded,
					DbAccessType.Unknown);
			}).GetEnumerator();
		}

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

		bool ILocalDbReposetoryBaseInternalUsage.ReposetoryCreated
		{
			get { return ReposetoryCreated; }
			set { ReposetoryCreated = value; }
		}

		public LocalDbManager Database
		{
			get { return _databaseDatabase; }
		}

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
		public virtual void CopyTo(Array array, int index)
		{
			lock (LockRoot)
			{
				lock (SyncRoot)
				{
					var values = ToArray();
					values.CopyTo(array, index);
				}
			}
		}

		public virtual object SyncRoot
		{
			get { return LockRoot; }
		}

		public virtual bool IsSynchronized
		{
			get { return Monitor.IsEntered(LockRoot); }
		}

		public virtual bool ContainsId(object fkValueForTableX)
		{
			var local = Base.ContainsKey(fkValueForTableX);
			if (!local)
			{
				//try upcasting
				local = Base.ContainsKey(Convert.ChangeType(fkValueForTableX, _typeInfo.PrimaryKeyProperty.PropertyType));
			}

			if (!local && Db != null)
			{
				return Db.Select(_typeInfo.Type, fkValueForTableX) != null;
			}
			return local;
		}

		public void Add(object item)
		{
			Add((TEntity) item);
		}

		public virtual bool Contains(object item)
		{
			return Contains((TEntity) item);
		}

		/// <summary>
		///     Checks if the given primarykey is taken
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool Contains(long item)
		{
			return ContainsId(item);
		}

		/// <summary>
		///     Checks if the given primarykey is taken
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool Contains(int item)
		{
			return ContainsId(item);
		}

		public virtual bool Remove(object item)
		{
			return Remove((TEntity) item)
				;
		}

		public bool Update(object item)
		{
			return Update((TEntity) item);
		}

		public DbClassInfoCache TypeInfo
		{
			get { return _typeInfo; }
			set { _typeInfo = value; }
		}

		protected void Init(Type type, ILocalDbPrimaryKeyConstraint keyGenerator, DbConfig config, bool useOrignalObjectInMemory)
		{
			if (_config != null)
				throw new InvalidOperationException("Multibe calls of Init are not supported");

			_keepOriginalObject = useOrignalObjectInMemory;
			_config = config;
			_databaseDatabase = LocalDbManager.Scope;
			if (_databaseDatabase == null)
			{
				throw new NotSupportedException("Please define a new DatabaseScope that allows to seperate" +
				                                " multibe tables in the same Application");
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
				                                              "type of any value type cannot be used. Type: '{0}'", type.Name));
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
					primaryKeyConstraint = defaultKeyGen.Clone() as ILocalDbPrimaryKeyConstraint;
				}
				else
				{
					throw new NotSupportedException(
						string.Format("You must specify ether an Primary key that is of one of this types " +
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
			if (IdentityInsertScope.Current != null && _currentIdentityInsertScope == null)
			{
				_currentIdentityInsertScope = IdentityInsertScope.Current;
			}

			if (_currentIdentityInsertScope != null)
			{
				if (idVal.Equals(Constraints.PrimaryKey.GetUninitilized()) && !_currentIdentityInsertScope.RewriteDefaultValues)
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
							s.Value.ForginKeyDeclarationAttribute.Attribute.ForeignTable == localDbReposetory.TypeInfo.TableName)
						.Value;

				if (fkPropForTypeX == null)
					continue;

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

			//foreach (var item in Constraints.Check)
			//{
			//	if (!item.CheckConstraint(refItem))
			//	{
			//		return new ConstraintException(string.Format("The Check Constraint '{0}' has detected an invalid object", item.Name));
			//	}
			//}

			//foreach (var item in Constraints.Unique)
			//{
			//	if (!item.CheckConstraint(refItem))
			//	{
			//		return new ConstraintException(string.Format("The Unique Constraint '{0}' has detected an invalid object", item.Name));
			//	}
			//}

			return null;
		}

		private ExceptionDispatchInfo AttachTransactionIfSet(TEntity changedItem, CollectionStates action,
			bool throwInstant = false)
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

				return null;
			}
			var ex = EnforceCheckConstraints(changedItem);

			if (throwInstant && ex != null)
			{
				throw ex;
			}
			if (ex != null)
				return ExceptionDispatchInfo.Capture(ex);
			return null;
		}

		private void _currentTransaction_TransactionCompleted(object sender, TransactionEventArgs e)
		{
			lock (LockRoot)
			{
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
						if (checkEnforceConstraints != null)
						{
							try
							{
								throw checkEnforceConstraints;
							}
							finally
							{
								_currentTransaction_Rollback();
							}
						}
					}
				}
				finally
				{
					_transactionalItems.Clear();
				}
			}
		}

		/// <summary>
		///     When using the KeepOriginalObject option set to false you can update any element by using this function
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool Update(TEntity item)
		{
			TriggersUsage.For.OnUpdate(item);
			bool op;
			if (Db != null)
			{
				if (!TriggersUsage.InsteadOf.OnUpdate(item))
				{
					op = Db.Update(item);
				}
				else
				{
					op = true;
				}
				TriggersUsage.After.OnUpdate(item);
				return op;
			}
			Constraints.Check.Enforce(item);
			Constraints.Unique.Enforce(item);
			var getElement = this[GetId(item)];
			if (getElement == null)
				return false;
			if (ReferenceEquals(item, getElement))
				return true;
			if (!TriggersUsage.InsteadOf.OnUpdate(item))
			{
				DataConverterExtensions.CopyPropertys(item, getElement, _config);
			}
			TriggersUsage.After.OnUpdate(item);
			Constraints.Unique.ItemUpdated(item);
			return true;
		}

		public Contacts.Pager.IDataPager<TEntity> CreatePager()
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
						return s;
					bool fullyLoaded;
					return (TEntity)DbAccessLayer.CreateInstance(
						_typeInfo,
						new ObjectDataRecord(s, _config, 0),
						out fullyLoaded,
						DbAccessType.Unknown);
				}).ToArray();
			}
		}
	}
}