#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.Contacts.MetaApi;

#endregion

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

		/// <summary>
		///     Initializes a new instance of the <see cref="MethodArgsInfoCache{TAtt}" /> class.
		/// </summary>
		/// <param name="info">The information.</param>
		public MethodArgsInfoCache(ParameterInfo info)
		{
			Init(info);
		}

		/// <summary>
		///     The name of this Param
		/// </summary>
		public string ArgumentName { get; private set; }

		/// <summary>
		///     The type of this Param
		/// </summary>
		public Type Type { get; private set; }

		/// <summary>
		///     All Attached Attributes
		/// </summary>
		public HashSet<TAtt> Attributes { get; private set; }

		/// <summary>
		///     Direct reflection
		/// </summary>
		public ParameterInfo ParameterInfo { get; private set; }

		/// <summary>
		///     For Internal Usage only
		/// </summary>
		/// <param name="info">The information.</param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">The object is already Initialed. A Change is not allowed</exception>
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

		/// <summary>
		///     Compares the current instance with another object of the same type and returns an integer that indicates whether
		///     the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="other">An object to compare with this instance.</param>
		/// <returns>
		///     A value that indicates the relative order of the objects being compared. The return value has these meanings: Value
		///     Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  Zero This instance
		///     occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows
		///     <paramref name="other" /> in the sort order.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public int CompareTo(IMethodArgsInfoCache<TAtt> other)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool Equals(IMethodArgsInfoCache<TAtt> other)
		{
			throw new NotImplementedException();
		}
	}
}