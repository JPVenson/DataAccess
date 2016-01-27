using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
	public class ClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg>
		: IClassInfoCache<TProp, TAttr, TMeth, TCtor, TArg>
		where TProp : class, IPropertyInfoCache<TAttr>, new()
		where TAttr : class, IAttributeInfoCache, new()
		where TMeth : class, IMethodInfoCache<TAttr, TArg>, new()
		where TCtor : class, IConstructorInfoCache<TAttr, TArg>, new()
		where TArg : class, IMethodArgsInfoCache<TAttr>, new()
	{
		internal ClassInfoCache(Type type, bool anon = false)
		{
			Init(type, anon);
		}

		/// <summary>
		/// For internal use Only
		/// </summary>
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ClassInfoCache()
		{

		}

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
				.GetProperties()
				.Select(s => new TProp().Init(s, anon) as TProp)
				.ToDictionary(s => s.PropertyName, s => s));
			MethodInfoCaches = new HashSet<TMeth>(type
				.GetMethods()
				.Select(s => new TMeth().Init(s) as TMeth));
			ConstructorInfoCaches = new HashSet<TCtor>(type
				.GetConstructors()
				.Select(s => new TCtor().Init(s) as TCtor));
			return this;
		}

		/// <summary>
		///     The .net ClassName
		/// </summary>
		public string ClassName { get; private set; }

		/// <summary>
		///     The .net Type instance
		/// </summary>
		public Type Type { get; private set; }

		/// <summary>
		///     All Propertys
		/// </summary>
		public Dictionary<string, TProp> PropertyInfoCaches { get; private set; }

		/// <summary>
		///     All Attributes on class level
		/// </summary>
		public HashSet<TAttr> AttributeInfoCaches { get; private set; }

		/// <summary>
		///     All Mehtods
		/// </summary>
		public HashSet<TMeth> MethodInfoCaches { get; private set; }

		/// <summary>
		///     All Constructors
		/// </summary>
		public HashSet<TCtor> ConstructorInfoCaches { get; private set; }

		[MethodImpl(MethodImplOptions.ForwardRef)]
		public bool Equals(IClassInfoCache other)
		{
			return new ClassInfoEquatableComparer().Equals(this, other);
		}

		[MethodImpl(MethodImplOptions.ForwardRef)]
		public int CompareTo(IClassInfoCache other)
		{
			return new ClassInfoEquatableComparer().Compare(this, other);
		}
	}
}