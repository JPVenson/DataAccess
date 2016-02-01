using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.MetaApi.Contract;

namespace JPB.DataAccess.MetaApi.Model
{
	/// <summary>
	///     Infos about the Method
	/// </summary>
	[DebuggerDisplay("{MethodName}")]
	[Serializable]
	public class MethodInfoCache<TAtt, TArg> :
		IMethodInfoCache<TAtt, TArg> 
		where TAtt : class, IAttributeInfoCache, new() 
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
		internal MethodInfoCache(MethodInfo mehtodInfo)
		{
			Init(mehtodInfo);
		}

		internal MethodInfoCache(Delegate fakeMehtod, string name = null, params TAtt[] attributes) 
			: this()
		{
			if (fakeMehtod == null)
				throw new ArgumentNullException("fakeMehtod");
			MethodInfo = fakeMehtod.GetMethodInfo();
			MethodName = string.IsNullOrEmpty(name) ? MethodInfo.Name : name;
			Delegate = fakeMehtod;
			AttributeInfoCaches = new HashSet<TAtt>(MethodInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new TAtt().Init(s as Attribute) as TAtt).Concat(attributes));
			Arguments = new HashSet<TArg>(MethodInfo.GetParameters().Select(s => new TArg().Init(s) as TArg));
		}

		/// <summary>
		/// For internal use Only
		/// </summary>
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public MethodInfoCache()
		{
			AttributeInfoCaches = new HashSet<TAtt>();
			Arguments = new HashSet<TArg>();
		}

		public virtual IMethodInfoCache<TAtt, TArg> Init(MethodInfo mehtodInfo)
		{
			if (!string.IsNullOrEmpty(MethodName))
				throw new InvalidOperationException("The object is already Initialed. A Change is not allowed");

			if (mehtodInfo == null)
				throw new ArgumentNullException("mehtodInfo");
			MethodInfo = mehtodInfo;
			MethodName = mehtodInfo.Name;
			AttributeInfoCaches = new HashSet<TAtt>(mehtodInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new TAtt().Init(s as Attribute) as TAtt));
			Arguments = new HashSet<TArg>(mehtodInfo.GetParameters().Select(s => new TArg().Init(s) as TArg));
			return this;
		}

		/// <summary>
		///     if set this method does not exist so we fake it
		/// </summary>
		public Delegate Delegate { get; private set; }

		/// <summary>
		///     Direct Reflection
		/// </summary>
		public MethodInfo MethodInfo { get; private set; }

		/// <summary>
		///     The name of the method
		/// </summary>
		public string MethodName { get; private set; }

		/// <summary>
		/// Arguments on this Method
		/// </summary>
		public HashSet<TArg> Arguments { get; private set; }

		/// <summary>
		///     All Attributes on this Method
		/// </summary>
		public HashSet<TAtt> AttributeInfoCaches { get; private set; }

		/// <summary>
		///     Easy access to the underlying delegate
		/// </summary>
		/// <returns></returns>
		public virtual object Invoke(object target, params object[] param)
		{
			if (Delegate != null)
			{
				var para = new object[param.Length + 1];
				para[0] = target;
				for (var i = 0; i < param.Length; i++)
				{
					para[1 + i] = param[i];
				}
				return Delegate.DynamicInvoke(para);
			}
			return MethodInfo.Invoke(target, param);
		}

		internal static Delegate ExtractDelegate(MethodInfo method)
		{
			var args = new List<Type>(
				method.GetParameters().Select(p => p.ParameterType));
			Type delegateType;
			if (method.ReturnType == typeof (void))
			{
				delegateType = Expression.GetActionType(args.ToArray());
			}
			else
			{
				args.Add(method.ReturnType);
				delegateType = Expression.GetFuncType(args.ToArray());
			}
			return Delegate.CreateDelegate(delegateType, null, method);
		}

		public bool Equals(IMethodInfoCache<TAtt, TArg> other)
		{
			return new MethodInfoCacheEquatableComparer<TAtt, TArg>().Equals(this, other);
		}

		public int CompareTo(IMethodInfoCache<TAtt, TArg> other)
		{
			return new MethodInfoCacheEquatableComparer<TAtt, TArg>().Compare(this, other);
		}
	}
}