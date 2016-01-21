using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.Config.Contract;

namespace JPB.DataAccess.Config.Model
{
	/// <summary>
	///     Infos about the Ctor
	/// </summary>
	[Serializable]
	public class ConstructorInfoCache : IConstructorInfoCache
	{
		public ConstructorInfoCache()
		{
		}

		internal ConstructorInfoCache(ConstructorInfo ctorInfo)
		{
			Init(ctorInfo);
		}

		public IConstructorInfoCache Init(ConstructorInfo ctorInfo)
		{
			MethodInfo = ctorInfo;
			MethodName = ctorInfo.Name;
			AttributeInfoCaches = new HashSet<AttributeInfoCache>(ctorInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new AttributeInfoCache(s as Attribute)));
			return this;
		}

		/// <summary>
		///     Direct Reflection
		/// </summary>
		public ConstructorInfo MethodInfo { get; private set; }

		/// <summary>
		///     The name of the constructor
		/// </summary>
		public string MethodName { get; private set; }

		/// <summary>
		///     All Attributes
		/// </summary>
		public HashSet<AttributeInfoCache> AttributeInfoCaches { get; private set; }
	}
}