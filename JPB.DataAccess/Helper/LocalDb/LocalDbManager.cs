using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper.LocalDb.PrimaryKeyProvider;
using JPB.DataAccess.Helper.LocalDb.Scopes;

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

		internal Dictionary<Type, LocalDbReposetoryBase> Database
		{
			get { return _database; }
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

		/// <summary>
		/// allowes to Add or remove tabels from this Database. 
		/// If you try to use the tables before calling dispose on the returned Scope an InvalidOperationException will be thrown
		/// </summary>
		/// <returns></returns>
		public DatabaseScope Alter()
		{
			_mappings.Clear();
			foreach (var localDbReposetoryBase in _database)
			{
				localDbReposetoryBase.Value.ReposetoryCreated = false;
			}
			return new DatabaseScope(Scope);
		}

		/// <summary>
		/// Creates a new Class that supportes the <c>IXmlSerializable</c> interface. It is linked to this 
		/// database and can be used to read or write all content in this database.
		/// To Write all elements
		/// <example>
		/// <code>
		/// using (var memStream = new MemoryStream())
		///	{
		///		new XmlSerializer(typeof(DataContent)).Serialize(memStream, LocalDbManager.Scope.GetSerializableContent());
		///		var xml = Encoding.ASCII.GetString(memStream.ToArray());
		///	}
		/// </code> 
		/// When reading the data the database creation has to be in progress. You must execute the statement inside the DatabaseScope you want to fill
		/// <code>
		/// using (new DatabaseScope())
		///	{
		///		//Table creation ...
		///		//new LocalDbReposetory&lt;T&gt;(new DbConfig())
		///		using (var memStream = new MemoryStream(Encoding.ASCII.GetBytes("xml")))
		///		{
		///			new XmlSerializer(typeof(DataContent)).Deserialize(memStream);
		///		}
		///	}
		/// </code> 
		/// </example>
		/// </summary>
		/// <returns></returns>
		public DataContent GetSerializableContent()
		{
			return new DataContent(this);
		}
	}
}