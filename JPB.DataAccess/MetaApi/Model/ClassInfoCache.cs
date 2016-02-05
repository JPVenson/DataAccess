using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using JPB.DataAccess.MetaApi.Contract;
using JPB.DataAccess.MetaApi.Model.Equatable;

namespace JPB.DataAccess.MetaApi.Model
{
	/// <summary>
	///     for internal use only
	/// </summary>
	[DebuggerDisplay("{ClassName}")]
	[Serializable]
	public abstract class ClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg>
		: IClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg>
		where TProp : class, IPropertyInfoCache<TAttr>, new()
		where TAttr : class, IAttributeInfoCache, new()
		where TMeth : class, IMethodInfoCache<TAttr, TArg>, new()
		where TCtor : class, IConstructorInfoCache<TAttr, TArg>, new()
		where TArg : class, IMethodArgsInfoCache<TAttr>, new()
	{
		internal ClassInfoCache(Type type, bool anon = false)
		{
			//this is ok.
			//init is used to be called from an other MetaApi part after the instanciation
			//so as this constructor is internal we know what we do ;-)
			Init(type, anon);
		}

		/// <summary>
		/// For internal use Only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected ClassInfoCache()
		{

		}

		/// <summary>
		/// For interal use Only
		/// </summary>
		/// <param name="type"></param>
		/// <param name="anon"></param>
		/// <returns></returns>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg> Init(Type type, bool anon = false)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (!string.IsNullOrEmpty(ClassName))
				throw new InvalidOperationException("The object is already Initialed. A Change is not allowed");

			ClassName = type.Name;
			Type = type;
			AttributeInfoCaches = new HashSet<TAttr>(type
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new TAttr().Init(s as Attribute) as TAttr));
			PropertyInfoCaches = new Dictionary<string, TProp>(type
				.GetProperties(BindingFlags.Public | BindingFlags.Static |
		   BindingFlags.NonPublic | BindingFlags.Instance)
				.Select(s => new TProp().Init(s, anon) as TProp)
				.ToDictionary(s => s.PropertyName, s => s));
			MethodInfoCaches = new HashSet<TMeth>(type
				.GetMethods(BindingFlags.Public | BindingFlags.Static |
		   BindingFlags.NonPublic | BindingFlags.Instance)
				.Select(s => new TMeth().Init(s) as TMeth));
			ConstructorInfoCaches = new HashSet<TCtor>(type
				.GetConstructors(BindingFlags.Public | BindingFlags.Static |
		   BindingFlags.NonPublic | BindingFlags.Instance)
				.Select(s => new TCtor().Init(s) as TCtor));
			var defaultConstructor = ConstructorInfoCaches.FirstOrDefault(f => !f.Arguments.Any());

			if (type.IsValueType || defaultConstructor != null)
			{
				var dm = new DynamicMethod("InvokeDefaultCtorFor" + ClassName, Type, System.Type.EmptyTypes, Type, true);
				var il = dm.GetILGenerator();
				il.Emit(OpCodes.Newobj);
				var ctorDelegate= dm.CreateDelegate(typeof (Func<>).MakeGenericType(type));
				DefaultFactory = new TMeth().Init(ctorDelegate.Method, type) as TMeth;
			}

			return this;
		}

		/// <summary>
		/// The default constructor that takes no arguments if known
		/// </summary>
		public TMeth DefaultFactory { get; protected internal set; }

		/// <summary>
		///     The .net ClassName
		/// </summary>
		public string ClassName { get; protected internal set; }

		/// <summary>
		///     The .net Type instance
		/// </summary>
		public Type Type { get; protected internal set; }

		/// <summary>
		///     All Propertys
		/// </summary>
		public Dictionary<string, TProp> PropertyInfoCaches { get; protected internal set; }

		/// <summary>
		///     All Attributes on class level
		/// </summary>
		public HashSet<TAttr> AttributeInfoCaches { get; protected internal set; }

		/// <summary>
		///     All Mehtods
		/// </summary>
		public HashSet<TMeth> MethodInfoCaches { get; protected internal set; }

		/// <summary>
		///     All Constructors
		/// </summary>
		public HashSet<TCtor> ConstructorInfoCaches { get; protected internal set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public bool Equals(IClassInfoCache other)
		{
			return new ClassInfoEquatableComparer().Equals(this, other);
		}

		public int CompareTo(IClassInfoCache other)
		{
			return new ClassInfoEquatableComparer().Compare(this, other);
		}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}