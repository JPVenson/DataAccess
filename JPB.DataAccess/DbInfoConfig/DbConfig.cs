/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.MetaApi;

namespace JPB.DataAccess.DbInfoConfig
{
	/// <summary>
	/// 
	/// </summary>
	public class DbConfig : MetaInfoStore<DbClassInfoCache, DbPropertyInfoCache, DbAttributeInfoCache, DbMethodInfoCache, DbConstructorInfoCache, DbMethodArgument>
	{
		static DbConfig()
		{
			ConstructorSettings = new FactoryHelperSettings();
		}

		/// <summary>
		///     The settings that are used to create a DOM ctor
		/// </summary>
		public static FactoryHelperSettings ConstructorSettings { get; private set; }

		public new DbClassInfoCache GetOrCreateClassInfoCache(Type type)
		{
			bool isNewCreated;
			var val = base.GetOrCreateClassInfoCache(type, out isNewCreated);
			if (isNewCreated)
				val.CheckForConfig();
			return val;
		}

		public new DbClassInfoCache GetOrCreateClassInfoCache(string type)
		{
			bool isNewCreated;
			var val = base.GetOrCreateClassInfoCache(type, out isNewCreated);
			if (isNewCreated)
				val.CheckForConfig();
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
			model.CheckCtor();
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
			return this.Include(typeof(T));
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
		public new virtual DbConfig Include(Type t)
		{
			GetOrCreateClassInfoCache(t);
			return this;
		}
	}
}