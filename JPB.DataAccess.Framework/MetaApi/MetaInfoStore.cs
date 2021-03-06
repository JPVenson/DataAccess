#if !DEBUG
using System.Diagnostics;
#endif
using System.Collections.Concurrent;

#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using JPB.DataAccess.Contacts.MetaApi;

#endregion

namespace JPB.DataAccess.MetaApi
{
	/// <summary>
	///     Class info Storage. When this is a Global config store you should may never call the dispose method because it
	///     would erase all knwon types
	/// </summary>
#if !DEBUG
	[DebuggerStepThrough]
#endif
	public class MetaInfoStore<TClass, TProp, TAttr, TMeth, TCtor, TArg> :
		IDisposable
		where TClass : class, IClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg>, new()
		where TProp : class, IPropertyInfoCache<TAttr>, new()
		where TAttr : class, IAttributeInfoCache, new()
		where TMeth : class, IMethodInfoCache<TAttr, TArg>, new()
		where TCtor : class, IConstructorInfoCache<TAttr, TArg>, new()
		where TArg : class, IMethodArgsInfoCache<TAttr>, new()
	{
		/// <summary>
		///     Is this instance mapped to the global Cache or does it only maintain its informations as long as it exists
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is global; otherwise, <c>false</c>.
		/// </value>
		public bool IsGlobal { get; }

		/// <summary>
		///     Creates a new Instance for storing class informations. Allows you to define if this is ether the global config
		///     store or a local one
		/// </summary>
		/// <param name="local">if set to <c>true</c> [local].</param>
		public MetaInfoStore(bool local)
		{
			IsGlobal = !local;
			_classInfoCaches = new ConcurrentDictionary<Type, TClass>();
		}

		/// <summary>
		///     Creates a new Instance for accessing the Global Config store
		/// </summary>
		public MetaInfoStore()
			: this(true)
		{
		}

		/// <summary>
		///     Initializes the <see cref="MetaInfoStore{TClass, TProp, TAttr, TMeth, TCtor, TArg}" /> class.
		/// </summary>
		static MetaInfoStore()
		{
			_globalClassInfoCaches = new ConcurrentDictionary<Type, TClass>();
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
			if (EnableGlobalThreadSafety)
			{
				lock (_globalClassInfoCaches)
				{
					_globalClassInfoCaches.Clear();
				}
			}
			else
			{
				_globalClassInfoCaches.Clear();
			}
		}

		/// <summary>
		///     Global or local Cache
		/// </summary>
		/// <value>
		///     The s class information caches.
		/// </value>
		protected internal virtual IDictionary<Type, TClass> SClassInfoCaches
		{
			get
			{
				if (IsGlobal)
				{
					return _globalClassInfoCaches;
				}
				return _classInfoCaches;
			}
		}

		/// <summary>
		///     The class information caches
		/// </summary>
		private static readonly IDictionary<Type, TClass> _globalClassInfoCaches;

		/// <summary>
		///     The class information caches
		/// </summary>
		private readonly IDictionary<Type, TClass> _classInfoCaches;

		/// <summary>
		///     Gets an Cache object if exists or creats one
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		protected internal virtual TClass GetOrCreateClassInfoCache(Type type)
		{
			bool buff;
			return GetOrCreateClassInfoCache(type, out buff);
		}

		/// <summary>
		///     Gets an Cache object if exists or creats one
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="newCreated">if set to <c>true</c> [new created].</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">type</exception>
		protected internal virtual TClass GetOrCreateClassInfoCache(Type type, out bool newCreated)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}
			newCreated = false;
			TClass element;
			var isThreadSave = EnableInstanceThreadSafety || EnableGlobalThreadSafety;
			try
			{
				if (isThreadSave)
				{
					Monitor.Enter(SClassInfoCaches);
				}
				
				if (!SClassInfoCaches.TryGetValue(type, out element))
				{
					element = new TClass();
					
					if (!type.IsAnonymousType())
					{
						SClassInfoCaches.Add(type, element);
					}
					element.Init(type, type.IsAnonymousType());
					newCreated = true;
				}
			}
			finally
			{
				if (isThreadSave)
				{
					Monitor.Exit(SClassInfoCaches);
				}
			}
			return element;
		}

		///// <summary>
		/////     Gets an Cache object of exists or creats one
		/////     Return value can be null
		///// </summary>
		///// <param name="typeName">Name of the type.</param>
		///// <param name="newCreated">if set to <c>true</c> [new created].</param>
		///// <returns></returns>
		///// <exception cref="ArgumentException">Value cannot be null or empty.;typeName</exception>
		//protected internal virtual TClass GetOrCreateClassInfoCache(string typeName, out bool newCreated)
		//{
		//	if (string.IsNullOrEmpty(typeName))
		//	{
		//		throw new ArgumentException("Value cannot be null or empty.", nameof(typeName));
		//	}
		//	newCreated = false;
		//	TClass element;
		//	var isThreadSave = EnableInstanceThreadSafety || EnableGlobalThreadSafety;
		//	try
		//	{
		//		if (isThreadSave)
		//		{
		//			Monitor.Enter(SClassInfoCaches);
		//		}

		//		element = SClassInfoCaches.FirstOrDefault(s => s.Name.Equals(typeName));
		//		if (element == null)
		//		{
		//			var type = Type.GetType(typeName, true, false);

		//			if (type == null)
		//			{
		//				return null;
		//			}

		//			element = new TClass();
		//			SClassInfoCaches.TryAdd(element);
		//			element.Init(type);
		//			newCreated = true;
		//		}
		//	}
		//	finally
		//	{
		//		if (isThreadSave)
		//		{
		//			Monitor.Exit(SClassInfoCaches);
		//		}
		//	}
		//	return element;
		//}
		
		/// <summary>
		///     Gets an Cache object if exists or creats one
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		protected internal virtual TMeth GetOrCreateMethodInfoCache(MethodInfo type)
		{
			var declareingType = type.ReflectedType;
			var element = GetOrCreateClassInfoCache(declareingType);
			return element.Mehtods.FirstOrDefault(s => s.MethodName == type.Name);
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
			return Include(typeof(T));
		}

		/// <summary>
		///     Append
		///     as an Optimistic input to the store.
		///     This allows you to explicit control when the MetaInfoStore store will enumerate the type object.
		///     This will be implicit called when GetOrCreateClassInfoCache is called and the type is not known
		/// </summary>
		/// <param name="t">The t.</param>
		/// <returns></returns>
		public virtual MetaInfoStore<TClass, TProp, TAttr, TMeth, TCtor, TArg> Include(Type t)
		{
			GetOrCreateClassInfoCache(t);
			return this;
		}

		/// <summary>
		///     Append
		///     as an Optimistic input to the store.
		///     This allows you to explicit control when the MetaInfoStore store will enumerate the type object.
		///     This will be implicit called when GetOrCreateClassInfoCache is called and the type is not known
		/// </summary>
		/// <param name="t">The t.</param>
		/// <returns></returns>
		public virtual MetaInfoStore<TClass, TProp, TAttr, TMeth, TCtor, TArg> Include(params Type[] t)
		{
			var isThreadSave = EnableInstanceThreadSafety || EnableGlobalThreadSafety;
			try
			{
				if (isThreadSave)
				{
					Monitor.Enter(SClassInfoCaches);
				}
				foreach (var type in t)
				{
					if (!SClassInfoCaches.TryGetValue(type, out var element))
					{
						element = new TClass();
						if (!type.IsAnonymousType())
						{
							SClassInfoCaches.Add(type, element);
						}
						element.Init(type, type.IsAnonymousType());
					}
				}
			}
			finally
			{
				if (isThreadSave)
				{
					Monitor.Exit(SClassInfoCaches);
				}
			}
			return this;
		}

		/// <summary>
		///     Append
		///     as an Optimistic input to the store.
		///     This allows you to explicit control when the MetaInfoStore store will enumerate the type object.
		///     This will be implicit called when GetOrCreateClassInfoCache is called and the type is not known
		/// </summary>
		/// <param name="existingItem">The existing item.</param>
		/// <returns></returns>
		public virtual MetaInfoStore<TClass, TProp, TAttr, TMeth, TCtor, TArg> Include(TClass existingItem)
		{
			var isThreadSave = EnableInstanceThreadSafety || EnableGlobalThreadSafety;
			try
			{
				if (isThreadSave)
				{
					Monitor.Enter(SClassInfoCaches);
				}

				SClassInfoCaches.Add(existingItem.Type, existingItem);
			}
			finally
			{
				if (isThreadSave)
				{
					Monitor.Exit(SClassInfoCaches);
				}
			}
			return this;
		}

		/// <summary>
		///     If Enabled the GetOrCreateClassInfoCache mehtod will be locked due usage
		/// </summary>
		/// <value>
		///     <c>true</c> if [enable global thread safety]; otherwise, <c>false</c>.
		/// </value>
		public static bool EnableGlobalThreadSafety { get; set; }

		/// <summary>
		///     if Enabled this can overwrite the EnableGlobalThreadSafety property
		/// </summary>
		/// <value>
		///     <c>true</c> if [enable instance thread safety]; otherwise, <c>false</c>.
		/// </value>
		public bool EnableInstanceThreadSafety { get; set; }

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
		}
	}
}