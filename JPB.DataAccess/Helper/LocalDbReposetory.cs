using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Helper
{
	public class DatabaseScope : IDisposable
	{
		public DatabaseScope()
		{
			if (LocalDbManager.Scope != null)
				throw new NotSupportedException("Nested DatabaseScopes are not allowed");

			LocalDbManager.Scope = new LocalDbManager();
		}

		public void Dispose()
		{
			LocalDbManager.Scope = null;
		}
	}

	internal class LocalDbManager
	{
		public LocalDbManager()
		{
			_database = new Dictionary<Type, LocalDbReposetory>();
			_mappings = new HashSet<ReproMappings>();
		}

		[ThreadStatic]
		private static LocalDbManager _scope;

		public static LocalDbManager Scope
		{
			get
			{
				return _scope;
			}
			internal set { _scope = value; }
		}

		private Dictionary<Type, LocalDbReposetory> _database;
		private HashSet<ReproMappings> _mappings;

		internal void AddTable(LocalDbReposetory repro)
		{
			_database.Add(repro.TypeInfo.Type, repro);
		}

		internal void AddMapping(Type source, Type target)
		{
			_mappings.Add(new ReproMappings(source, target));
		}

		internal IEnumerable<LocalDbReposetory> GetMappings(Type thisType)
		{
			var mapping = _mappings.Where(f => f.TargetType == thisType).ToArray();
			return _database.Where(f => mapping.Any(e => e.SourceType == f.Key)).Select(f => f.Value);
		}
	}

	internal struct ReproMappings : IEquatable<ReproMappings>
	{
		private Type _sourceType;
		private Type _targetType;

		public ReproMappings(Type targetType, Type sourceType)
		{
			_sourceType = sourceType;
			_targetType = targetType;
		}

		public Type SourceType
		{
			get { return _sourceType; }
		}

		public Type TargetType
		{
			get { return _targetType; }
		}

		public bool Equals(ReproMappings other)
		{
			return SourceType == other.SourceType && TargetType == other.TargetType;
		}
	}


	/// <summary>
	/// Maintains a local collection of entitys simulating a basic DB Bevavior by setting PrimaryKeys in an General way. Starting with 0 incriment by 1
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class LocalDbReposetory
		: ICollection
	{
		internal protected readonly object _lockRoot = new object();
		internal protected DbClassInfoCache TypeInfo;
		internal long IdCounter;
		internal protected readonly DbAccessLayer _db;
		internal protected readonly Dictionary<long, object> _base;
		internal readonly LocalDbManager _databaseScope;

		/// <summary>
		/// Creates a new, only local Reposetory
		/// </summary>
		protected LocalDbReposetory(Type type)
		{
			_databaseScope = LocalDbManager.Scope;
			if (_databaseScope == null)
			{
				throw new NotSupportedException("Please define a new DatabaseScope that allows to seperate multibe tables in the same Application");
			}

			if (TypeInfo == null)
			{
				TypeInfo = new DbConfig().GetOrCreateClassInfoCache(type);
				if (TypeInfo.PrimaryKeyProperty == null)
				{
					throw new NotSupportedException(string.Format("Entitys without any PrimaryKey are not supported. Type: '{0}'", type.Name));
				}
			}

			_base = new Dictionary<long, object>();
			IdCounter = 1;
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

		private long SetId(object item)
		{
			var idVal = TypeInfo.PrimaryKeyProperty.Getter.Invoke(item);
			if (idVal != DbAccessLayer.DefaultAssertionObject)
			{
				lock (_lockRoot)
				{
					var newId = IdCounter++;

					if (TypeInfo.PrimaryKeyProperty.PropertyType != typeof(long))
					{
						TypeInfo.PrimaryKeyProperty.Setter.Invoke(item, Convert.ChangeType(newId, TypeInfo.PrimaryKeyProperty.PropertyType));
					}
					else
					{
						TypeInfo.PrimaryKeyProperty.Setter.Invoke(item, newId);
					}

					return newId;
				}
			}

			return (long)idVal;
		}

		private long GetId(object item)
		{
			return (long)TypeInfo.PrimaryKeyProperty.Getter.Invoke(item);
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
					if (!localDbReposetory.ContainsId(fkValueForTableX))
					{
						throw new ForginKeyConstraintException(TypeInfo.TableName, localDbReposetory.TypeInfo.TableName, fkValueForTableX);
					}
				}
			}
		}

		private bool ContainsId(object fkValueForTableX)
		{
			var local = _base.ContainsKey((long)Convert.ChangeType(fkValueForTableX, typeof(long)));
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
