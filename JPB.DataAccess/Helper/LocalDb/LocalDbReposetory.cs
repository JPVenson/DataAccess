using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		: ICollection
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
									  .Aggregate((e,f) => e + "," + f)));
				}
			}

			_base = new Dictionary<object, object>(_keyGenerator);
			_databaseScope.AddTable(this);

			foreach (var dbPropertyInfoCach in TypeInfo.PropertyInfoCaches)
			{
				if (dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute != null && dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType != null)
				{
					_databaseScope.AddMapping(TypeInfo.Type, dbPropertyInfoCach.Value.ForginKeyDeclarationAttribute.Attribute.ForeignType);
				}
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

		public IEnumerator GetEnumerator()
		{
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
			var refTables = _databaseScope.GetMappings(TypeInfo.Type);

			foreach (var localDbReposetory in refTables)
			{
				var fkPropForTypeX =
					TypeInfo.PropertyInfoCaches.FirstOrDefault(
						s =>
							s.Value.ForginKeyDeclarationAttribute != null &&
							s.Value.ForginKeyDeclarationAttribute.Attribute.ForeignTable == localDbReposetory.TypeInfo.TableName)
						.Value;

				if (fkPropForTypeX != null)
				{
					var fkValueForTableX = fkPropForTypeX.Getter.Invoke(item);
					if (fkValueForTableX != null && !localDbReposetory.ContainsId(fkValueForTableX))
					{
						throw new ForginKeyConstraintException(TypeInfo.TableName, localDbReposetory.TypeInfo.TableName, fkValueForTableX);
					}
				}
			}
		}

		private bool ContainsId(object fkValueForTableX)
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

		public void Add(object item)
		{
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

		public void Clear()
		{
			_base.Clear();
		}

		public bool Contains(object item)
		{
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
		public bool Contains(long item)
		{
			return ContainsId(item);
		}

		/// <summary>
		/// Checks if the given primarykey is taken
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(int item)
		{
			return ContainsId(item);
		}

		public bool Remove(object item)
		{
			var id = GetId(item);
			var success = _base.Remove(id);
			if (!success && _db != null)
			{
				_db.Delete(item);
				success = true;
			}
			return success;
		}

		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { return _base.Count; }
		}

		public object SyncRoot { get; private set; }
		public bool IsSynchronized { get; private set; }

		public bool IsReadOnly { get { return false; } }
	}

	/// <summary>
	/// Provides an wrapper for the non Generic LocalDbReposetory 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class LocalDbReposetory<T> : LocalDbReposetory, ICollection<T>
	{
		public LocalDbReposetory()
			: base(typeof(T))
		{
		}

		public LocalDbReposetory(DbAccessLayer db)
			: base(db, typeof(T))
		{
		}

		public void Add(T item)
		{
			base.Add(item);
		}

		public bool Contains(T item)
		{
			return base.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			base.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			return base.Remove(item);
		}

		public new IEnumerator<T> GetEnumerator()
		{
			return _base.Values.Cast<T>().GetEnumerator();
		}
	}
}
