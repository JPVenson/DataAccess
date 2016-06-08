using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbCollection;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	/// Maintains a local collection of entitys simulating a basic DB Bevavior by setting PrimaryKeys in an General way. Starting with 0 incriment by 1
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public abstract class LocalDbReposetoryBase : ICollection
	{
		private readonly DbConfig _config;
		protected internal readonly object _lockRoot = new object();
		protected internal readonly DbClassInfoCache TypeInfo;
		protected internal readonly DbClassInfoCache TypeKeyInfo;
		protected internal readonly DbAccessLayer _db;
		protected internal readonly IDictionary<object, object> _base;
		protected internal readonly LocalDbManager _databaseScope;
		protected internal readonly ILocalPrimaryKeyValueProvider _keyGenerator;
		protected internal readonly HashSet<ILocalDbConstraint> _constraints;
		private Transaction _currentTransaction;
		private List<TransactionalItem> _transactionalItems = new List<TransactionalItem>();

		internal class TransactionalItem
		{
			internal object Item { get; set; }
			internal CollectionStates State { get; set; }

			internal TransactionalItem(object item, CollectionStates state)
			{
				Item = item;
				State = state;
			}
		}

		/// <summary>
		/// Creates a new Instance that is bound to <paramref name="type"/> and uses <paramref name="keyGenerator"/> for generation of PrimaryKeys
		/// </summary>
		protected LocalDbReposetoryBase(Type type,
			ILocalPrimaryKeyValueProvider keyGenerator,
			DbConfig config,
			params ILocalDbConstraint[] constraints)
		{
			_config = config;
			_constraints = new HashSet<ILocalDbConstraint>(constraints);
			_databaseScope = LocalDbManager.Scope;
			if (_databaseScope == null)
			{
				throw new NotSupportedException("Please define a new DatabaseScope that allows to seperate multibe tables in the same Application");
			}

			TypeInfo = _config.GetOrCreateClassInfoCache(type);
			if (TypeInfo.PrimaryKeyProperty == null)
			{
				throw new NotSupportedException(string.Format("Entitys without any PrimaryKey are not supported. Type: '{0}'", type.Name));
			}

			TypeKeyInfo = _config.GetOrCreateClassInfoCache(TypeInfo.PrimaryKeyProperty.PropertyType);

			if (TypeKeyInfo == null)
			{
				throw new NotSupportedException(string.Format("Entitys without any PrimaryKey are not supported. Type: '{0}'", type.Name));
			}

			if (!TypeKeyInfo.Type.IsValueType)
			{
				throw new NotSupportedException(string.Format("Entitys without any PrimaryKey that is of type of any value type cannot be used. Type: '{0}'", type.Name));
			}

			if (keyGenerator != null)
			{
				_keyGenerator = keyGenerator;
			}
			else
			{
				ILocalPrimaryKeyValueProvider defaultKeyGen;
				if (LocalDbManager.DefaultPkProvider.TryGetValue(TypeKeyInfo.Type, out defaultKeyGen))
				{
					_keyGenerator = defaultKeyGen.Clone() as ILocalPrimaryKeyValueProvider;
				}
				else
				{
					throw new NotSupportedException(
						string.Format("You must specify ether an Primary key that is of one of this types " +
									  "({1}) " +
									  "or invoke the ctor with an proper keyGenerator. Type: '{0}'",
							type.Name,
							LocalDbManager
								.DefaultPkProvider
								.Keys
								.Select(f => f.Name)
								.Aggregate((e, f) => e + "," + f)));
				}
			}
			_base = new ConcurrentDictionary<object, object>(_keyGenerator);
			_databaseScope.SetupDone += DatabaseScopeOnSetupDone;
		}

		private void DatabaseScopeOnSetupDone(object sender, EventArgs eventArgs)
		{
			_databaseScope.AddTable(this);

			foreach (var dbPropertyInfoCach in TypeInfo.Propertys)
			{
				if (dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute != null && dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType != null)
				{
					_databaseScope.AddMapping(TypeInfo.Type, dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType);
				}
			}

			ReposetoryCreated = true;
		}

		/// <summary>
		/// Returns an value that indicates a proper DatabaseScope usage. 
		/// If true the creation was successfull and all tables for the this table are mapped
		/// The Reposetory cannot operate if the reposetory is not created!
		/// </summary>
		public bool ReposetoryCreated { get; private set; }

		private void CheckCreatedElseThrow()
		{
			if (!ReposetoryCreated)
			{
				throw new InvalidOperationException("The database must be completly created until this Table is operational");
			}
		}

		/// <summary>
		/// Creates a new, only local Reposetory by using one of the Predefined KeyGenerators
		/// </summary>
		protected LocalDbReposetoryBase(Type type)
			: this(type, null, new DbConfig())
		{

		}

		/// <summary>
		/// Creates a new, database as fallback using batabase
		/// </summary>
		/// <param name="db"></param>
		/// <param name="type"></param>
		protected LocalDbReposetoryBase(DbAccessLayer db, Type type)
			: this(type)
		{
			_db = db;
		}

		public IEnumerator GetEnumerator()
		{
			CheckCreatedElseThrow();
			if (_db != null)
				return _db.Select(TypeInfo.Type).GetEnumerator();

			return _base.Values.GetEnumerator();
		}

		private object SetId(object item)
		{
			var idVal = TypeInfo.PrimaryKeyProperty.Getter.Invoke(item);
			if (idVal != _keyGenerator.GetUninitilized())
			{
				lock (_lockRoot)
				{
					object newId = _keyGenerator.GetNextValue();
					TypeInfo.PrimaryKeyProperty.Setter.Invoke(item, Convert.ChangeType(newId, TypeInfo.PrimaryKeyProperty.PropertyType));
					return newId;
				}
			}

			return idVal;
		}

		private object GetId(object item)
		{
			return TypeInfo.PrimaryKeyProperty.Getter.Invoke(item);
		}

		private void EnforceConstraints(object item)
		{
			var ex = CheckEnforceConstraints(item);
			if (ex != null)
				throw ex;
		}

		private ConstraintException CheckEnforceConstraints(object refItem)
		{
			var refTables = _databaseScope.GetMappings(TypeInfo.Type);

			foreach (var localDbReposetory in refTables)
			{
				var fkPropForTypeX =
					TypeInfo.Propertys.FirstOrDefault(
						s =>
							s.Value.ForginKeyDeclarationAttribute != null &&
							s.Value.ForginKeyDeclarationAttribute.Attribute.ForeignTable == localDbReposetory.TypeInfo.TableName)
						.Value;

				if (fkPropForTypeX != null)
				{
					var fkValueForTableX = fkPropForTypeX.Getter.Invoke(refItem);
					if (fkValueForTableX != null && !localDbReposetory.ContainsId(fkValueForTableX))
					{
						return new ForginKeyConstraintException(TypeInfo.TableName, localDbReposetory.TypeInfo.TableName, fkValueForTableX);
					}
				}
			}

			foreach (var item in _constraints)
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
			var local = _base.ContainsKey(fkValueForTableX);
			if (!local)
			{
				//try upcasting
				local = _base.ContainsKey(Convert.ChangeType(fkValueForTableX, TypeInfo.PrimaryKeyProperty.PropertyType));
			}

			if (!local && _db != null)
			{
				return _db.Select(TypeInfo.Type, fkValueForTableX) != null;
			}
			return local;
		}

		private ConstraintException AttachTransactionIfSet(object changedItem, CollectionStates action,  bool throwInstant = false)
		{
			if (Transaction.Current != null)
			{
				if (this._currentTransaction == null)
				{
					this._currentTransaction = Transaction.Current;
					this._currentTransaction.TransactionCompleted += _currentTransaction_TransactionCompleted;
				}

				var hasElement = this._transactionalItems.FirstOrDefault(s => s.Item == changedItem);
				if (hasElement != null)
				{
					if (hasElement.State == CollectionStates.Added)
					{
						if (action == CollectionStates.Removed)
						{
							this._transactionalItems.Remove(hasElement);
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
					this._transactionalItems.Add(new TransactionalItem(changedItem, action));
				}

				return null;
			}
			else
			{
				var ex = this.CheckEnforceConstraints(changedItem);
				
				if (throwInstant && ex != null)
				{
					throw ex;
				}
				return ex;
			}
		}

		private void _currentTransaction_TransactionCompleted(object sender, TransactionEventArgs e)
		{
			try
			{
				foreach (var transactionalItem in _transactionalItems)
				{
					var checkEnforceConstraints = this.CheckEnforceConstraints(transactionalItem.Item);
					if (checkEnforceConstraints != null)
					{
						switch (transactionalItem.State)
						{
							case CollectionStates.Unknown:
								break;
							case CollectionStates.Unchanged:
								break;
							case CollectionStates.Added:
								_base.Remove(transactionalItem.Item);
								break;
							case CollectionStates.Changed:
								break;
							case CollectionStates.Removed:
								_base.Add(GetId(transactionalItem.Item), transactionalItem.Item);
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
						throw checkEnforceConstraints;
					}
				}
			}
			finally
			{
				_transactionalItems.Clear();
			}
		}

		/// <summary>
		/// Adds a new Item to the Table
		/// </summary>
		/// <param name="item"></param>
		public virtual void Add(object item)
		{
			CheckCreatedElseThrow();
			if (_db != null)
			{
				_db.Insert(item);
			}
			else
			{
				if (!Contains(item))
				{
					AttachTransactionIfSet(item, CollectionStates.Added, true);
					_base.Add(SetId(item), item);
				}
			}
		}

		/// <summary>
		/// Removes all items from this Table
		/// </summary>
		public virtual void Clear()
		{
			CheckCreatedElseThrow();
			lock (this._lockRoot)
			{
				foreach (var item in this._base)
				{
					Remove(item);
				}
			}
		}

		public virtual bool Contains(object item)
		{
			CheckCreatedElseThrow();
			var pk = GetId(item);
			var local = _base.Contains(new KeyValuePair<object, object>(pk, item));
			if (!local && _db != null)
			{
				return _db.Select(TypeInfo.Type, pk) != null;
			}

			return local;
		}

		/// <summary>
		/// Checks if the given primarykey is taken
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool Contains(long item)
		{
			return ContainsId(item);
		}

		/// <summary>
		/// Checks if the given primarykey is taken
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
			bool success;
			lock (this._lockRoot)
			{
				success = _base.Remove(id);
				var hasInvalidOp = AttachTransactionIfSet(item, CollectionStates.Removed);
				if (hasInvalidOp != null)
				{
					_base.Add(id, item);
					throw hasInvalidOp;
				}
			}

			if (!success && _db != null)
			{
				_db.Delete(item);
				success = true;
			}
			return success;
		}

		/// <summary>
		/// Returns an object with the given Primarykey
		/// </summary>
		/// <param name="primaryKey"></param>
		/// <returns></returns>
		public object this[object primaryKey]
		{
			get
			{
				object value;
				if (_base.TryGetValue(primaryKey, out value))
					return value;
				return null;
			}
		}

		/// <summary>
		/// Thread save
		/// </summary>
		/// <returns></returns>
		public object[] ToArray()
		{
			lock (SyncRoot)
			{
				return _base.Values.ToArray();
			}
		}

		/// <summary>
		/// Thread save
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public virtual void CopyTo(Array array, int index)
		{
			lock (SyncRoot)
			{
				var values = _base.Values.ToArray();
				values.CopyTo(array, index);
			}
		}

		public virtual int Count
		{
			get { return _base.Count; }
		}

		public virtual object SyncRoot
		{
			get { return this._lockRoot; }
		}

		public virtual bool IsSynchronized
		{
			get { return Monitor.IsEntered(_lockRoot); }
		}

		public virtual bool IsReadOnly
		{
			get { return false; }
		}
	}
}