using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	/// Maintains a local collection of entitys simulating a basic DB Bevavior by setting PrimaryKeys in an General way. Starting with 0 incriment by 1
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class LocalDbReposetory
	{
		internal protected readonly object _lockRoot = new object();
		internal protected DbClassInfoCache TypeInfo;
		internal protected DbClassInfoCache TypeKeyInfo;
		internal protected readonly DbAccessLayer _db;
		internal protected readonly Dictionary<object, object> _base;
		internal readonly LocalDbManager _databaseScope;
		internal readonly ILocalPrimaryKeyValueProvider _keyGenerator;

		protected LocalDbReposetory(Type type, ILocalPrimaryKeyValueProvider keyGenerator)
		{
			_databaseScope = LocalDbManager.Scope;
			if (_databaseScope == null)
			{
				throw new NotSupportedException("Please define a new DatabaseScope that allows to seperate multibe tables in the same Application");
			}

			TypeInfo = new DbConfig().GetOrCreateClassInfoCache(type);
			if (TypeInfo.PrimaryKeyProperty == null)
			{
				throw new NotSupportedException(string.Format("Entitys without any PrimaryKey are not supported. Type: '{0}'", type.Name));
			}

			TypeKeyInfo = TypeInfo.PrimaryKeyProperty.PropertyType.GetClassInfo();

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
			_base = new Dictionary<object, object>(_keyGenerator);
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
		/// Creates a new, only local Reposetory
		/// </summary>
		protected LocalDbReposetory(Type type)
			: this(type, null)
		{

		}

		/// <summary>
		/// Creates a new, database as fallback using batabase
		/// </summary>
		/// <param name="db"></param>
		/// <param name="type"></param>
		protected LocalDbReposetory(DbAccessLayer db, Type type)
			: this(type)
		{
			_db = db;
		}

		protected virtual IEnumerator GetEnumerator()
		{
			CheckCreatedElseThrow();
			if (_db != null)
				return _db.Select(TypeInfo.Type).GetEnumerator();

			return _base.Values.GetEnumerator();
		}

		private object SetId(object item)
		{
			var idVal = TypeInfo.PrimaryKeyProperty.Getter.Invoke(item);
			if (idVal != DbAccessLayer.DefaultAssertionObject)
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

		private ForginKeyConstraintException CheckEnforceConstraints(object item)
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
					var fkValueForTableX = fkPropForTypeX.Getter.Invoke(item);
					if (fkValueForTableX != null && !localDbReposetory.ContainsId(fkValueForTableX))
					{
						return new ForginKeyConstraintException(TypeInfo.TableName, localDbReposetory.TypeInfo.TableName, fkValueForTableX);
					}
				}
			}

			return null;
		}

		protected virtual bool ContainsId(object fkValueForTableX)
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

		/// <summary>
		/// Adds a new Item to the Table
		/// </summary>
		/// <param name="item"></param>
		protected virtual void Add(object item)
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
					EnforceConstraints(item);
					_base.Add(SetId(item), item);
				}
			}
		}

		/// <summary>
		/// Removes all items from this Table
		/// </summary>
		protected virtual void Clear()
		{
			CheckCreatedElseThrow();
			foreach (var item in this._base)
			{
				Remove(item);
			}
		}

		protected virtual bool Contains(object item)
		{
			CheckCreatedElseThrow();
			var local = _base.ContainsValue(item);
			if (!local && _db != null)
			{
				var pk = GetId(item);
				return _db.Select(TypeInfo.Type, pk) != null;
			}

			return local;
		}

		/// <summary>
		/// Checks if the given primarykey is taken
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		protected virtual bool Contains(long item)
		{
			return ContainsId(item);
		}

		/// <summary>
		/// Checks if the given primarykey is taken
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		protected virtual bool Contains(int item)
		{
			return ContainsId(item);
		}

		protected virtual bool Remove(object item)
		{
			CheckCreatedElseThrow();
			var id = GetId(item);
			var success = _base.Remove(id);
			var hasInvalidOp = CheckEnforceConstraints(item);
			if (hasInvalidOp != null)
			{
				_base.Add(id, item);
				throw hasInvalidOp;
			}

			if (!success && _db != null)
			{
				_db.Delete(item);
				success = true;
			}
			return success;
		}		

		protected virtual void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		protected virtual int Count
		{
			get { return _base.Count; }
		}

		protected virtual object SyncRoot
		{
			get { return this._lockRoot; }
		}

		protected virtual bool IsSynchronized
		{
			get { return Monitor.IsEntered(_lockRoot); }
		}

		protected virtual bool IsReadOnly { get { return false; } }
	}

	/// <summary>
	/// Provides an wrapper for the non Generic LocalDbReposetory 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class LocalDbReposetory<T> : LocalDbReposetory, ICollection<T>
	{
		/// <summary>
		/// Creates a new LocalDB Repro by using <typeparamref name="T"/>
		/// </summary>
		public LocalDbReposetory()
			: base(typeof(T))
		{
		}
		/// <summary>
		/// Creates a new LocalDB Repro by using <typeparamref name="T"/> that uses the DbAccessLayer as fallback if the requested item was not found localy
		/// </summary>
		public LocalDbReposetory(DbAccessLayer db)
			: base(db, typeof(T))
		{
		}
		/// <summary>
		/// Creates a new LocalDB Repro by using <typeparamref name="T"/> and uses the KeyProvider to generate Primarykeys
		/// </summary>
		public LocalDbReposetory(ILocalPrimaryKeyValueProvider keyProvider)
			: base(typeof(T), keyProvider)
		{
		}

		/// <summary>
		/// Adds a new Item to the Table
		/// </summary>
		/// <param name="item"></param>
		public void Add(T item)
		{
			base.Add(item);
		}
		
		/// <summary>
		/// Removes all items from this Table
		/// </summary>
		public void Clear()
		{
			base.Clear();
		}

		/// <summary>
		/// Checks if the item is ether localy stored or on database
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(T item)
		{
			return base.Contains(item);
		}

		/// <summary>
		/// Checks if the key is known
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Contains(object key)
		{
			return base.ContainsId(key);
		}

		/// <summary>
		/// Copys the current collection the an Array
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo(T[] array, int arrayIndex)
		{
			base.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes the item from this collection
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(T item)
		{
			return base.Remove(item);
		}

		/// <summary>
		/// Returns the count of all knwon items
		/// </summary>
		public int Count
		{
			get { return base.Count; }
		}

		/// <summary>
		/// False
		/// </summary>
		public bool IsReadOnly
		{
			get { return base.IsReadOnly; }
		}

		/// <summary>
		/// Gets an enumerator
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			return _base.Values.Cast<T>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
