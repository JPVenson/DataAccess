using System;
using System.Collections.Generic;
using System.Linq;

namespace JPB.DataAccess.Helper.LocalDb
{
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
}