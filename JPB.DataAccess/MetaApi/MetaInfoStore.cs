/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
#if !DEBUG
using System.Diagnostics;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using JPB.DataAccess.MetaApi.Contract;

namespace JPB.DataAccess.MetaApi
{
	/// <summary>
	///     Class info Storage. When this is a Global config store you should may never call the dispose method because it would erase all knwon types
	/// </summary>
#if !DEBUG
	[DebuggerStepThrough]
#endif
	public class MetaInfoStore<TClass, TProp, TAttr, TMeth, TCtor, TArg> : IDisposable
		where TClass : class, IClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg>, new()
		where TProp : class, IPropertyInfoCache<TAttr>, new()
		where TAttr : class, IAttributeInfoCache, new()
		where TMeth /*HeHeHe*/ : class, IMethodInfoCache<TAttr, TArg>, new()
		where TCtor : class, IConstructorInfoCache<TAttr, TArg>, new() 
		where TArg : class, IMethodArgsInfoCache<TAttr>, new()
	{
		/// <summary>
		/// Is this instance mapped to the global Cache or does it only maintain its informations as long as it exists
		/// </summary>
		public bool IsGlobal { get; private set; }

		/// <summary>
		/// Creates a new Instance for storing class informations. Allows you to define if this is ether the global config store or a local one
		/// </summary>
		public MetaInfoStore(bool isGlobal)
		{
			IsGlobal = isGlobal;
			_classInfoCaches = new HashSet<TClass>();
		}

		/// <summary>
		/// Creates a new Instance for accessing the Global Config store
		/// </summary>
		public MetaInfoStore() 
			: this(true)
		{
			
		}

		/// <summary>
		/// 
		/// </summary>
		static MetaInfoStore()
		{
			ClassInfoCaches = new HashSet<TClass>();
		}

		/// <summary>
		///     For Internal use Only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Clear()
		{
			ClassInfoCaches.Clear();
		}

		protected internal virtual HashSet<TClass> SClassInfoCaches
		{
			get
			{
				if (IsGlobal)
					return ClassInfoCaches;
				return _classInfoCaches;
			}
		}

		private static readonly HashSet<TClass> ClassInfoCaches;
		private readonly HashSet<TClass> _classInfoCaches;

		/// <summary>
		///     Gets an Cache object if exists or creats one
		/// </summary>
		/// <returns></returns>
		protected internal virtual TProp GetOrCreatePropertyInfoCache(PropertyInfo type)
		{
			var declareingType = type.ReflectedType;
			var element = GetOrCreateClassInfoCache(declareingType);
			TProp cache;
			element.PropertyInfoCaches.TryGetValue(type.Name, out cache);
			return cache;
		}

		/// <summary>
		///     Gets an Cache object if exists or creats one
		/// </summary>
		/// <returns></returns>
		protected internal virtual TClass GetOrCreateClassInfoCache(Type type)
		{
			bool buff;
			return GetOrCreateClassInfoCache(type, out buff);
		}

		/// <summary>
		///     Gets an Cache object if exists or creats one
		/// </summary>
		/// <returns></returns>
		protected internal virtual TClass GetOrCreateClassInfoCache(Type type, out bool newCreated)
		{
			newCreated = false;
			TClass element;
			try
			{
				if (EnableThreadSafety)
				{
					Monitor.Enter(SClassInfoCaches);
				}

				element = SClassInfoCaches.FirstOrDefault(s => s.ClassName == type.Name);
				if (element == null)
				{
					element = new TClass();
					SClassInfoCaches.Add(element);
					element.Init(type);
					newCreated = true;
				}
			}
			finally
			{
				if (EnableThreadSafety)
				{
					Monitor.Exit(SClassInfoCaches);
				}
			}
			return element;
		}

		/// <summary>
		///     Gets an Cache object if exists or creats one
		/// </summary>
		/// <returns></returns>
		protected internal virtual TMeth GetOrCreateMethodInfoCache(MethodInfo type)
		{
			var declareingType = type.ReflectedType;
			var element = GetOrCreateClassInfoCache(declareingType);
			return element.MethodInfoCaches.FirstOrDefault(s => s.MethodName == type.Name);
		}

		/// <summary>
		///     Append
		///     as an Optimistic input to the store.
		///     This allows you to explicit control when the MetaInfoStore store will enumerate the type object.
		///     This will be implicit called when GetOrCreateClassInfoCache is called and the type is not known
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual MetaInfoStore<TClass, TProp, TAttr, TMeth, TCtor, TArg> Include<T>()
		{
			return this.Include(typeof(T));
		}

		/// <summary>
		///     Append
		///     as an Optimistic input to the store.
		///     This allows you to explicit control when the MetaInfoStore store will enumerate the type object.
		///     This will be implicit called when GetOrCreateClassInfoCache is called and the type is not known
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual MetaInfoStore<TClass, TProp, TAttr, TMeth, TCtor, TArg> Include(Type t)
		{
			GetOrCreateClassInfoCache(t);
			return this;
		}

		/// <summary>
		///     If Enabled the GetOrCreateClassInfoCache mehtod will be locked due usage
		/// </summary>
		public bool EnableThreadSafety { get; set; }

		public virtual void Dispose()
		{
			ClassInfoCaches.Clear();
		}
	}
}