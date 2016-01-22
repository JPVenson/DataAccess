using System;
using JPB.DataAccess.Config;
using JPB.DataAccess.DbInfoConfig.DbInfo;

namespace JPB.DataAccess.DbInfoConfig
{
	/// <summary>
	/// 
	/// </summary>
	public class DbConfig : ConfigBase<DbClassInfoCache, DbPropertyInfoCache, DbAttributeInfoCache, DbMethodInfoCache, DbConstructorInfoCache>
	{
		static DbConfig()
		{
			ConstructorSettings = new FactoryHelperSettings();
		}

		/// <summary>
		///     The settings that are used to create a DOM ctor
		/// </summary>
		public static FactoryHelperSettings ConstructorSettings { get; private set; }

		protected internal override DbClassInfoCache GetOrCreateClassInfoCache(Type type)
		{
			bool isNewCreated;
			var val = base.GetOrCreateClassInfoCache(type, out isNewCreated);
			if (isNewCreated)
				val.CheckForConfig();
			return val;
		}

		/// <summary>
		///     Allows you to alter the ConfigBase store that holds <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void SetConfig<T>(Action<ConfigurationResolver<T>> validator)
		{
			validator(new ConfigurationResolver<T>(this, GetOrCreateClassInfoCache(typeof(T))));
			GetOrCreateClassInfoCache(typeof(T)).Refresh(true);
		}
	}
}