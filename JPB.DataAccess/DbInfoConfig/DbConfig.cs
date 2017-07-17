#region

using System;
using System.Linq;
using System.Threading;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.MetaApi;

#endregion

namespace JPB.DataAccess.DbInfoConfig
{
	/// <summary>
	/// </summary>
	public class DbConfig
		:
			MetaInfoStore
			<DbClassInfoCache, DbPropertyInfoCache, DbAttributeInfoCache, DbMethodInfoCache, DbConstructorInfoCache,
				DbMethodArgument>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DbConfig" /> class.
		/// </summary>
		/// <param name="local"></param>
		public DbConfig(bool local = false)
			: base(local)
		{
			ConstructorSettings = FactoryHelperSettings.DefaultSettings.Copy();
		}

//		/// <summary>
//		/// For Internal use Only
//		/// </summary>
//#if !DEBUG
//		[DebuggerHidden]
//#endif
//		[Browsable(false)]
//		[EditorBrowsable(EditorBrowsableState.Never)]
//		public static void Clear()
//		{
//			MetaInfoStore<DbClassInfoCache, DbPropertyInfoCache, DbAttributeInfoCache, DbMethodInfoCache, DbConstructorInfoCache, DbMethodArgument>.Clear();
//		}

		/// <summary>
		///     The settings that are used to create a DOM ctor
		/// </summary>
		public FactoryHelperSettings ConstructorSettings { get; private set; }

		/// <summary>
		///     Gets an Cache object if exists or creats one
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public new DbClassInfoCache GetOrCreateClassInfoCache(Type type)
		{
			bool isNewCreated;
			var val = base.GetOrCreateClassInfoCache(type, out isNewCreated);
			if (isNewCreated)
				val.CheckForConfig(this);
			return val;
		}

		/// <summary>
		///     Gets the or create class information cache.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public DbClassInfoCache GetOrCreateClassInfoCache(string type)
		{
			bool isNewCreated;
			var val = base.GetOrCreateClassInfoCache(type, out isNewCreated);
			if (isNewCreated)
				val.CheckForConfig(this);
			return val;
		}


		/// <summary>
		///     Allows you to alter the MetaInfoStore store that holds <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void SetConfig<T>(Action<ConfigurationResolver<T>> validator)
		{
			validator(new ConfigurationResolver<T>(this, GetOrCreateClassInfoCache(typeof(T))));
			var model = GetOrCreateClassInfoCache(typeof(T));
			model.Refresh(true);
			model.CheckCtor(this);
		}

		/// <summary>
		///     Append
		///     <typeparamref name="T" />
		///     as an Optimistic input to the store.
		///     This allows you to explicit control when the MetaInfoStore store will enumerate the type object.
		///     This will be implicit called when GetOrCreateClassInfoCache is called and the type is not known
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public new virtual DbConfig Include<T>()
		{
			return Include(typeof(T));
		}

		/// <summary>
		///     Append the type
		///     as an Optimistic input to the store.
		///     This allows you to explicit control when the MetaInfoStore store will enumerate the type object.
		///     This will be implicit called when GetOrCreateClassInfoCache is called and the type is not known
		/// </summary>
		/// <returns></returns>
		public new virtual DbConfig Include(Type type)
		{
			GetOrCreateClassInfoCache(type);
			return this;
		}

		/// <summary>
		///     Includes the specified types.
		/// </summary>
		/// <param name="t">The type.</param>
		/// <returns></returns>
		public new DbConfig Include(params Type[] t)
		{
			var isThreadSave = EnableGlobalThreadSafety || EnableGlobalThreadSafety;
			try
			{
				if (isThreadSave)
					Monitor.Enter(SClassInfoCaches);
				foreach (var type in t)
				{
					var element = SClassInfoCaches.FirstOrDefault(s => s.Equals(type));
					if (element == null)
					{
						element = new DbClassInfoCache();
						if (!type.IsAnonymousType())
							SClassInfoCaches.Add(element);
						element.Init(type, type.IsAnonymousType());
						element.CheckForConfig(this);
					}
				}
			}
			finally
			{
				if (isThreadSave)
					Monitor.Exit(SClassInfoCaches);
			}
			return this;
		}

		/// <summary>
		///     Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();
			//TODO as the file is in use we cannot delete it
			//while (this.ConstructorSettings.TempFileData.Count > 0)
			//{
			//	string item;
			//	while (this.ConstructorSettings.TempFileData.TryPeek(out item))
			//	{
			//		try
			//		{
			//			File.Delete(item);
			//		}
			//		catch (Exception)
			//		{
			//			Trace.WriteLine(string.Format("File delete Failed {0}", item));
			//		}
			//	}
			//}
		}
	}
}