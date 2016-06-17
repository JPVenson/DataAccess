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
	public abstract class LocalDbReposetoryBase : ICollection
	{
		private readonly DbConfig _config;
		private readonly LocalDbManager _databaseDatabase;
		private readonly bool _keepOriginalObject;
		private readonly List<TransactionalItem> _transactionalItems = new List<TransactionalItem>();
		protected internal readonly IDictionary<object, object> Base;
		protected internal readonly HashSet<ILocalDbConstraint> Constraints;
		protected internal readonly DbAccessLayer Db;
		protected internal readonly ILocalPrimaryKeyValueProvider KeyGenerator;
		protected internal readonly object LockRoot = new object();
		protected internal readonly DbClassInfoCache TypeInfo;
		protected internal readonly DbClassInfoCache TypeKeyInfo;
		private IdentityInsertScope _currentIdentityInsertScope;
		private Transaction _currentTransaction;
		private bool _isMigrating;


		/// <summary>
		///     Creates a new Instance that is bound to &lt;paramref name="type"/&gt; and uses &lt;paramref name="keyGenerator"/
		///     &gt; for generation of PrimaryKeys
		///     Must created inside an DatabaseScope
		/// </summary>
		/// <param name="type">The type of an Valid Poco</param>
		/// <param name="keyGenerator">The Strategy to generate an uniqe PrimaryKey that matches the PrimaryKey Property</param>
		/// <param name="config">The Config store to use</param>
		/// <param name="useOrignalObjectInMemory">
		///     If enabled the given object referance will be used (Top performance).
		///     if Disabled each object has to be define an Valid Ado.Net constructor to allow a copy (Can be slow)
		/// </param>
		/// <param name="constraints">Additonal Constrains to ensure database like Data Integrity</param>
		protected LocalDbReposetoryBase(Type type,
			ILocalPrimaryKeyValueProvider keyGenerator,
			DbConfig config,
			bool useOrignalObjectInMemory,
			params ILocalDbConstraint[] constraints)
		{
			_keepOriginalObject = useOrignalObjectInMemory;
			_config = config;
			Constraints = new HashSet<ILocalDbConstraint>(constraints);
			_databaseDatabase = LocalDbManager.Scope;
			if (_databaseDatabase == null)
			{
				throw new NotSupportedException("Please define a new DatabaseScope that allows to seperate" +
												" multibe tables in the same Application");
			}

			TypeInfo = _config.GetOrCreateClassInfoCache(type);
			if (TypeInfo.PrimaryKeyProperty == null)
			{
				throw new NotSupportedException(string.Format("Entitys without any PrimaryKey are not supported. " +
															  "Type: '{0}'", type.Name));
			}

			TypeKeyInfo = _config.GetOrCreateClassInfoCache(TypeInfo.PrimaryKeyProperty.PropertyType);

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

			if (keyGenerator != null)
			{
				KeyGenerator = keyGenerator;
			}
			else
			{
				ILocalPrimaryKeyValueProvider defaultKeyGen;
				if (LocalDbManager.DefaultPkProvider.TryGetValue(TypeKeyInfo.Type, out defaultKeyGen))
				{
					KeyGenerator = defaultKeyGen.Clone() as ILocalPrimaryKeyValueProvider;
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
			Triggers = new TriggerForTableCollection(this);
			Base = new ConcurrentDictionary<object, object>();
			_databaseDatabase.AddTable(this);
			_databaseDatabase.SetupDone += DatabaseDatabaseOnSetupDone;
		}

		/// <summary>
		///     Creates a new, only local Reposetory by using one of the Predefined KeyGenerators
		/// </summary>
		protected LocalDbReposetoryBase(Type type)
			: this(type, null, new DbConfig(), true)
		{
		}

		/// <summary>
		///     Creates a new, database as fallback using batabase
		/// </summary>
		/// <param name="db"></param>
		/// <param name="type"></param>
		protected LocalDbReposetoryBase(DbAccessLayer db, Type type)
			: this(type)
		{
			Db = db;
		}

		/// <summary>
		///     Returns an value that indicates a proper DatabaseScope usage.
		///     If true the creation was successfull and all tables for the this table are mapped
		///     The Reposetory cannot operate if the reposetory is not created!
		/// </summary>
		public bool ReposetoryCreated { get; internal set; }

		/// <summary>
		///     Returns an object with the given Primarykey
		/// </summary>
		/// <param name="primaryKey"></param>
		/// <returns></returns>
		public object this[object primaryKey]
		{
			get
			{
				object value;
				if (Base.TryGetValue(primaryKey, out value))
					return value;
				return null;
			}
		}

		public virtual bool IsReadOnly
		{
			get { return false; }
		}

		public LocalDbManager Database
		{
			get { return _databaseDatabase; }
		}

		internal bool IsMigrating
		{
			get { return _isMigrating; }
			set { _isMigrating = value; }
		}

		public IEnumerator GetEnumerator()
		{
			CheckCreatedElseThrow();
			if (Db != null)
				return Db.Select(TypeInfo.Type).GetEnumerator();

			return Base.Values.Select(s =>
			{
				if (_keepOriginalObject)
					return s;
				bool fullyLoaded;
				return DbAccessLayer.CreateInstance(
					TypeInfo,
					new ObjectDataRecord(s, _config, 0),
					out fullyLoaded,
					DbAccessType.Unknown);
			}).GetEnumerator();
		}

		/// <summary>
		///     Thread save
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public virtual void CopyTo(Array array, int index)
		{
			lock (SyncRoot)
			{
				var values = this.ToArray();
				values.CopyTo(array, index);
			}
		}

		public virtual int Count
		{
			get { return Base.Count; }
		}

		public virtual object SyncRoot
		{
			get { return LockRoot; }
		}

		public virtual bool IsSynchronized
		{
			get { return Monitor.IsEntered(LockRoot); }
		}

		private void DatabaseDatabaseOnSetupDone(object sender, EventArgs eventArgs)
		{
			ReposetoryCreated = false;
			lock (LockRoot)
			{
				foreach (var dbPropertyInfoCach in TypeInfo.Propertys)
				{
					if (dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute != null &&
						dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType != null)
					{
						_databaseDatabase.AddMapping(TypeInfo.Type,
							dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType);
					}
				}

				ReposetoryCreated = true;
				IsMigrating = false;
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
				if (idVal.Equals(KeyGenerator.GetUninitilized()) && !_currentIdentityInsertScope.RewriteDefaultValues)
				{
					return idVal;
				}
				if (!idVal.Equals(KeyGenerator.GetUninitilized()))
				{
					return idVal;
				}
				lock (LockRoot)
				{
					var newId = KeyGenerator.GetNextValue();
					TypeInfo.PrimaryKeyProperty.Setter.Invoke(item, Convert.ChangeType(newId, TypeInfo.PrimaryKeyProperty.PropertyType));
					return newId;
				}
			}

			if (idVal.Equals(KeyGenerator.GetUninitilized()))
			{
				lock (LockRoot)
				{
					var newId = KeyGenerator.GetNextValue();
					TypeInfo.PrimaryKeyProperty.Setter.Invoke(item, Convert.ChangeType(newId, TypeInfo.PrimaryKeyProperty.PropertyType));
					return newId;
				}
			}

			var exception =
				new InvalidOperationException(string.Format("Cannot insert explicit value for identity column in table '{0}' " +
															"when no IdentityInsertScope exists.", TypeInfo.Name));
			throw exception;
		}

		private object GetId(object item)
		{
			var key = TypeInfo.PrimaryKeyProperty.Getter.Invoke(item);
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

		private ConstraintException CheckEnforceConstraints(object refItem)
		{
			var refTables = _databaseDatabase.GetMappings(TypeInfo.Type);

			foreach (var localDbReposetory in refTables)
			{
				var fkPropForTypeX =
					TypeInfo.Propertys.FirstOrDefault(
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
						TypeInfo.TableName,
						localDbReposetory.TypeInfo.TableName,
						fkValueForTableX,
						TypeInfo.PrimaryKeyProperty.PropertyName,
						fkPropForTypeX.PropertyName);
				}
			}

			foreach (var item in Constraints)
			{
				if (!item.CheckConstraint(refItem))
				{
					return new ConstraintException(string.Format("The Constraint '{0}' has detected an invalid object", item.Name));
				}
			}

			return null;
		}

		public virtual bool ContainsId(object fkValueForTableX)
		{
			var local = Base.ContainsKey(fkValueForTableX);
			if (!local)
			{
				//try upcasting
				local = Base.ContainsKey(Convert.ChangeType(fkValueForTableX, TypeInfo.PrimaryKeyProperty.PropertyType));
			}

			if (!local && Db != null)
			{
				return Db.Select(TypeInfo.Type, fkValueForTableX) != null;
			}
			return local;
		}

		private ExceptionDispatchInfo AttachTransactionIfSet(object changedItem, CollectionStates action,
			bool throwInstant = false)
		{
			if (Transaction.Current != null)
			{
				if (_currentTransaction == null)
				{
					_currentTransaction = Transaction.Current;
					_currentTransaction.TransactionCompleted += _currentTransaction_TransactionCompleted;
				}

				var hasElement = _transactionalItems.FirstOrDefault(s => s.Item == changedItem);
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
					_transactionalItems.Add(new TransactionalItem(changedItem, action));
				}

				return null;
			}
			var ex = CheckEnforceConstraints(changedItem);

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
						var checkEnforceConstraints = CheckEnforceConstraints(transactionalItem.Item);
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
		/// When using the KeepOriginalObject option set to false you can update any element by using this function
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool Update(object item)
		{
			Triggers.For.OnUpdate(item);
			bool op;
			if (Db != null)
			{
				if (!this.Triggers.InsteadOf.OnUpdate(item))
				{
					op = Db.Update(item);
				}
				else
				{
					op = true;
				}
				this.Triggers.After.OnUpdate(item);
				return op;
			}

			var getElement = this[GetId(item)];
			if(getElement == null)
				return false;
			if (object.ReferenceEquals(item, getElement))
				return true;

			if (!this.Triggers.InsteadOf.OnUpdate(item))
			{
				DataConverterExtensions.CopyPropertys(item, getElement, _config);
			}
			this.Triggers.After.OnUpdate(item);
			return true;
		}

		/// <summary>
		///     Adds a new Item to the Table
		/// </summary>
		/// <param name="item"></param>
		public virtual void Add(object item)
		{
			var elementToAdd = item;
			CheckCreatedElseThrow();
			if (Db != null)
			{
				this.Triggers.For.OnInsert(item);
				if (!this.Triggers.InsteadOf.OnInsert(item))
					Db.Insert(elementToAdd);
				this.Triggers.After.OnInsert(item);
			}
			else
			{
				if (!Contains(elementToAdd))
				{
					this.Triggers.For.OnInsert(item);
					AttachTransactionIfSet(elementToAdd,
						CollectionStates.Added,
						true);
					var id = SetNextId(elementToAdd);
					if (!_keepOriginalObject)
					{
						bool fullyLoaded;
						elementToAdd = DbAccessLayer.CreateInstance(
							TypeInfo,
							new ObjectDataRecord(item, _config, 0),
							out fullyLoaded,
							DbAccessType.Unknown);
						if (!fullyLoaded)
						{
							throw new InvalidOperationException(string.Format("The given type did not provide a Full ado.net constructor " +
																			  "and the setting of the propertys did not succeed. " +
																			  "Type: '{0}'", item.GetType()));
						}
					}
					if (!this.Triggers.InsteadOf.OnInsert(item))
					{
						Base.Add(id, elementToAdd);
					}
					try
					{
						this.Triggers.After.OnInsert(item);
					}
					catch (Exception e)
					{
						Base.Remove(id);
						throw e;
					}
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

		public virtual bool Contains(object item)
		{
			CheckCreatedElseThrow();
			var pk = GetId(item);
			var local = Base.Contains(new KeyValuePair<object, object>(pk, item));
			if (!local && Db != null)
			{
				return Db.Select(TypeInfo.Type, pk) != null;
			}

			return local;
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
			CheckCreatedElseThrow();
			var id = GetId(item);
			bool success = true;
			lock (LockRoot)
			{
				this.Triggers.For.OnDelete(item);
				if (!this.Triggers.InsteadOf.OnDelete(item))
					success = Base.Remove(id);
				var hasInvalidOp = AttachTransactionIfSet(item, CollectionStates.Removed);
				try
				{
					this.Triggers.After.OnDelete(item);
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
			}

			if (!success && Db != null)
			{
				this.Triggers.For.OnDelete(item);
				Db.Delete(item);
				this.Triggers.After.OnDelete(item);
				success = true;
			}
			return success;
		}

		/// <summary>
		///     Thread save
		/// </summary>
		/// <returns></returns>
		public object[] ToArray()
		{
			lock (SyncRoot)
			{
				return Base.Values.Select(s =>
				{
					if (_keepOriginalObject)
						return s;
					bool fullyLoaded;
					return DbAccessLayer.CreateInstance(
							TypeInfo,
							new ObjectDataRecord(s, _config, 0),
							out fullyLoaded,
							DbAccessType.Unknown);
				}).ToArray();
			}
		}

		#region Triggers

		/// <summary>
		/// Contains acccess to INSERT/DELETE/UPDATE(WIP) Triggers
		/// </summary>
		public TriggerForTableCollection Triggers { get; private set; }

		#endregion
	}
}