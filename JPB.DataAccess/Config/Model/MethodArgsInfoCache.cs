using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace JPB.DataAccess.Config.Model
{
	/// <summary>
	/// Infos about Arguments delcared on a Mehtod
	/// </summary>
	[DebuggerDisplay("{ArgumentName}")]
	[Serializable]
	public class MethodArgsInfoCache
	{

		/// <summary>
		/// 
		/// </summary>
		public MethodArgsInfoCache()
		{
			Attributes = new HashSet<AttributeInfoCache>();
		}

		public MethodArgsInfoCache(ParameterInfo parameterInfo)
		{
			ParameterInfo = parameterInfo;
			ArgumentName = parameterInfo.Name;
			this.Type = parameterInfo.ParameterType;
			Attributes = new HashSet<AttributeInfoCache>(ParameterInfo
				.GetCustomAttributes(true)
				.Select(s => new AttributeInfoCache(s as Attribute)));
		}
		/// <summary>
		/// The name of this Param
		/// </summary>
		public string ArgumentName { get; private set; }
		/// <summary>
		/// The type of this Param
		/// </summary>
		public Type Type { get; private set; }

		/// <summary>
		/// All Attached Attributes
		/// </summary>
		public HashSet<AttributeInfoCache> Attributes { get; private set; }

		/// <summary>
		/// Direct reflection
		/// </summary>
		public ParameterInfo ParameterInfo { get; private set; }
	}
}
