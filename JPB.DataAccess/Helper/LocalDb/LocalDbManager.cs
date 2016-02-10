using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb
{
	internal class LocalDbManager
	{
		static LocalDbManager()
		{
			DefaultPkProvider = new Dictionary<Type, ILocalPrimaryKeyValueProvider>();
			DefaultPkProvider.Add(typeof(int), new LocalIntPkProvider());
			DefaultPkProvider.Add(typeof(long), new LocalLongPkProvider());
			DefaultPkProvider.Add(typeof(Guid), new LocalGuidPkProvider());
		}

		public LocalDbManager()
		{
			_database = new Dictionary<Type, LocalDbReposetory>();
			_mappings = new HashSet<ReproMappings>();
		}

		internal readonly static Dictionary<Type, ILocalPrimaryKeyValueProvider> DefaultPkProvider;
			
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

		private readonly Dictionary<Type, LocalDbReposetory> _database;
		private readonly HashSet<ReproMappings> _mappings;

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
}