/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.Contacts.MetaApi;

namespace JPB.DataAccess.MetaApi.Model
{
	/// <summary>
	///     Infos about Arguments delcared on a Mehtod
	/// </summary>
	[DebuggerDisplay("{ArgumentName}")]
	[Serializable]
	public class MethodArgsInfoCache<TAtt>
		: IMethodArgsInfoCache<TAtt>
		where TAtt : class, IAttributeInfoCache, new()
	{
		/// <summary>
		///     For Internal use only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public MethodArgsInfoCache()
		{
			Attributes = new HashSet<TAtt>();
		}

		public MethodArgsInfoCache(ParameterInfo info)
		{
			Init(info);
		}

		public string ArgumentName { get; private set; }

		public Type Type { get; private set; }

		public HashSet<TAtt> Attributes { get; private set; }

		/// <summary>
		///     Direct reflection
		/// </summary>
		public ParameterInfo ParameterInfo { get; private set; }

		public virtual IMethodArgsInfoCache<TAtt> Init(ParameterInfo info)
		{
			if (!string.IsNullOrEmpty(ArgumentName))
				throw new InvalidOperationException("The object is already Initialed. A Change is not allowed");
			ParameterInfo = info;
			ArgumentName = info.Name;
			Type = info.ParameterType;
			Attributes = new HashSet<TAtt>(ParameterInfo
				.GetCustomAttributes(true)
				.Select(s => new TAtt().Init(s as Attribute) as TAtt));
			return this;
		}

		public int CompareTo(IMethodArgsInfoCache<TAtt> other)
		{
			throw new NotImplementedException();
		}

		public bool Equals(IMethodArgsInfoCache<TAtt> other)
		{
			throw new NotImplementedException();
		}
	}
}