using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.Config.Contract;

namespace JPB.DataAccess.Config.Model
{
	/// <summary>
	///     for internal use only
	/// </summary>
	[DebuggerDisplay("{ClassName}")]
	[Serializable]
	public class ClassInfoCache<TProp, TAttr, TMeth, TCtor>
		: IClassInfoCache<TProp, TAttr, TMeth, TCtor>
		where TProp : class, IPropertyInfoCache, new()
		where TAttr : class, IAttributeInfoCache, new()
		where TMeth : class, IMethodInfoCache, new()
		where TCtor : class, IConstructorInfoCache, new()
	{
		internal ClassInfoCache(Type type, bool anon = false)
		{
			Init(type, anon);
		}

		public ClassInfoCache()
		{
			
		}

		public virtual IClassInfoCache<TProp, TAttr, TMeth, TCtor> Init(Type type, bool anon = false)
		{
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
	}
}