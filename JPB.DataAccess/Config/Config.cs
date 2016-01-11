using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using JPB.DataAccess.Config.Model;

#if !DEBUG
using System.Diagnostics;
#endif

namespace JPB.DataAccess.Config
{
	/// <summary>
	///     Class info Storage
	/// </summary>
#if !DEBUG
	[DebuggerStepThrough]
#endif
	public class Config
	{
		static Config()
		{
			SClassInfoCaches = new HashSet<ClassInfoCache>();
			ConstructorSettings = new FactoryHelperSettings();
		}

		/// <summary>
		///     The settings that are used to create a DOM ctor
		/// </summary>
		public static FactoryHelperSettings ConstructorSettings { get; private set; }

		/// <summary>
		///     Creates a new Instance for configuration
		/// </summary>
		public Config(bool enableReflection = true)
		{
			UseReflection = enableReflection;
		}

		/// <summary>
		///     Allows you to alter the Config store that holds <typeparamref name="T"></typeparamref>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void SetConfig<T>(Action<ConfigurationResolver<T>> validator)
		{
			validator(new ConfigurationResolver<T>(this, GetOrCreateClassInfoCache(typeof (T))));
			GetOrCreateClassInfoCache(typeof (T)).Refresh(true);
		}

		/// <summary>
		///     For Internal use Only
		/// </summary>
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Clear()
		{
			SClassInfoCaches.Clear();
		}

		/// <summary>
		///     Indicates the usage of Reflection
		/// </summary>
		public bool UseReflection { get; private set; }

		/// <summary>
		///		If Enabled the GetOrCreateClassInfoCache mehtod will be locked due usage
		/// </summary>
		public bool EnableThreadSafety { get; set; }

		/// <summary>
		///     Gets an Cache object if exists or creats one
		/// </summary>
		/// <returns></returns>
		internal PropertyInfoCache GetOrCreatePropertyInfoCache(PropertyInfo type)
		{
			//var declareingType = type.ReflectedType;
			//var name = type.Name;
			//var element = SClassInfoCaches.FirstOrDefault(s => s.Type == declareingType && s.PropertyInfoCaches.Any(e => e.PropertyName == name));

			//if (element == null)
			//{
			//	var declaringType = type.ReflectedType;
			//	SClassInfoCaches.Append(element = new ClassInfoCache(declaringType));
			//	element.CheckForConfig();
			//}

			//return element.PropertyInfoCaches.FirstOrDefault(s => s.PropertyName == type.Name);

			Type declareingType = type.ReflectedType;
			ClassInfoCache element = GetOrCreateClassInfoCache(declareingType);
			return element.PropertyInfoCaches.FirstOrDefault(s => s.Key == type.Name).Value;
		}

		/// <summary>
		///     Gets an Cache object if exists or creats one
		/// </summary>
		/// <returns></returns>
		internal ClassInfoCache GetOrCreateClassInfoCache(Type type)
		{
			if (this.EnableThreadSafety)
			{
				Monitor.Enter(SClassInfoCaches);
			}

			ClassInfoCache element = SClassInfoCaches.FirstOrDefault(s => s.ClassName == type.FullName);
			if (element == null)
			{
				SClassInfoCaches.Add(element = new ClassInfoCache(type));
				element.CheckForConfig();
			}

			if (this.EnableThreadSafety)
			{
				Monitor.Pulse(SClassInfoCaches);
				Monitor.Exit(SClassInfoCaches);
			}

			return element;
		}

		/// <summary>
		///     Gets an Cache object if exists or creats one
		/// </summary>
		/// <returns></returns>
		internal MethodInfoCache GetOrCreateMethodInfoCache(MethodInfo type)
		{
			//var declareingType = type.ReflectedType;
			//var name = type.Name;
			//var element = SClassInfoCaches.FirstOrDefault(s => s.Type == declareingType && s.MethodInfoCaches.Any(e => e.MethodName == name));
			//if (element == null)
			//{
			//	var declaringType = type.ReflectedType;
			//	SClassInfoCaches.Append(element = new ClassInfoCache(declaringType));
			//	element.CheckForConfig();
			//}

			//return element.MethodInfoCaches.FirstOrDefault(s => s.MethodName == type.Name);

			Type declareingType = type.ReflectedType;
			ClassInfoCache element = GetOrCreateClassInfoCache(declareingType);
			return element.MethodInfoCaches.FirstOrDefault(s => s.MethodName == type.Name);
		}

		internal static HashSet<ClassInfoCache> SClassInfoCaches { get; private set; }

		/// <summary>
		///     Append
		///     <typeparamref name="T"/>
		///     as an Optimistic input to the store.
		///     This allows you to explicit control when the Config store will enumerate the type object.
		///     This will be implicit called when GetOrCreateClassInfoCache is called and the type is not known
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public Config Include<T>()
		{
			GetOrCreateClassInfoCache(typeof (T));
			return this;
		}
	}
}