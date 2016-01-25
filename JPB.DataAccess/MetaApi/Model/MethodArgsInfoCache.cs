using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace JPB.DataAccess.MetaApi.Model
{
	/// <summary>
	/// Infos about Arguments delcared on a Mehtod
	/// </summary>
	[DebuggerDisplay("{ArgumentName}")]
	[Serializable]
	public class MethodArgsInfoCache
	{
		/// <summary>
		/// For Internal use only
		/// </summary>	
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public MethodArgsInfoCache()
		{
			Attributes = new HashSet<AttributeInfoCache>();
		}

		public MethodArgsInfoCache(ParameterInfo parameterInfo)
		{
			if (!string.IsNullOrEmpty(ArgumentName))
				throw new InvalidOperationException("The object is already Initialed. A Change is not allowed");
			ParameterInfo = parameterInfo;
			ArgumentName = parameterInfo.Name;
			this.Type = parameterInfo.ParameterType;
			Attributes = new HashSet<AttributeInfoCache>(ParameterInfo
				.GetCustomAttributes(true)
				.Select(s => new AttributeInfoCache(s as Attribute)));
		}

		public string ArgumentName { get; private set; }

		public Type Type { get; private set; }

		public HashSet<AttributeInfoCache> Attributes { get; private set; }

		/// <summary>
		/// Direct reflection
		/// </summary>
		public ParameterInfo ParameterInfo { get; private set; }
	}
}
