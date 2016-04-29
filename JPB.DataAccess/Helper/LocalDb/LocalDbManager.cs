using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb
{
	public class LocalDbManager
	{
		static LocalDbManager()
		{
			DefaultPkProvider = new Dictionary<Type, ILocalPrimaryKeyValueProvider>();
			DefaultPkProvider.Add(typeof(int), new LocalIntPkProvider());
			DefaultPkProvider.Add(typeof(long), new LocalLongPkProvider());
			DefaultPkProvider.Add(typeof(Guid), new LocalGuidPkProvider());
		}

		internal LocalDbManager()
		{
			_database = new Dictionary<Type, LocalDbReposetoryBase>();
			_mappings = new HashSet<ReproMappings>();
		}

		internal static readonly Dictionary<Type, ILocalPrimaryKeyValueProvider> DefaultPkProvider;
			
		[ThreadStatic]
		private static LocalDbManager _scope;

		/// <summary>
		/// Access to the current local Scope
		/// Not ThreadSave
		/// </summary>
		public static LocalDbManager Scope
		{
			get
			{
				return _scope;
			}
			internal set { _scope = value; }
		}

		/// <summary>
		/// Will be invoked when the current database is setup
		/// </summary>
		public event EventHandler SetupDone;

		internal void OnSetupDone()
		{
			var handler = SetupDone;
			if (handler != null) 
				handler(this, new EventArgs());
		}

		private readonly Dictionary<Type, LocalDbReposetoryBase> _database;
		private readonly HashSet<ReproMappings> _mappings;

		internal void AddTable(LocalDbReposetoryBase repro)
		{
			_database.Add(repro.TypeInfo.Type, repro);
		}

		internal void AddMapping(Type source, Type target)
		{
			_mappings.Add(new ReproMappings(source, target));
		}

		internal IEnumerable<LocalDbReposetoryBase> GetMappings(Type thisType)
		{
			var mapping = _mappings.Where(f => f.TargetType == thisType).ToArray();
			return _database.Where(f => mapping.Any(e => e.SourceType == f.Key)).Select(f => f.Value);
		}
	}
}