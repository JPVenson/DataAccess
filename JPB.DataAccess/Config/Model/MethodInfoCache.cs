using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.Config.Contract;

namespace JPB.DataAccess.Config.Model
{
	/// <summary>
	///     Infos about the Method
	/// </summary>
	[DebuggerDisplay("{MethodName}")]
	[Serializable]
	public class MethodInfoCache : IComparable<MethodInfoCache>, IMethodInfoCache
	{
		internal MethodInfoCache(MethodInfo mehtodInfo)
		{
			Init(mehtodInfo);
		}

		internal MethodInfoCache(Delegate fakeMehtod, string name = null, params AttributeInfoCache[] attributes)
		{
			if (fakeMehtod == null)
				throw new ArgumentNullException("fakeMehtod");
			MethodInfo = fakeMehtod.GetMethodInfo();
			MethodName = string.IsNullOrEmpty(name) ? MethodInfo.Name : name;
			Delegate = fakeMehtod;
			AttributeInfoCaches = new HashSet<AttributeInfoCache>(MethodInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new AttributeInfoCache(s as Attribute)).Concat(attributes));
		}

		public MethodInfoCache()
		{
			AttributeInfoCaches = new HashSet<AttributeInfoCache>();
		}

		public int CompareTo(MethodInfoCache other)
		{
			return String.Compare(MethodName, other.MethodName, StringComparison.Ordinal);
		}

		public IMethodInfoCache Init(MethodInfo mehtodInfo)
		{
			if (mehtodInfo == null)
				throw new ArgumentNullException("mehtodInfo");
			MethodInfo = mehtodInfo;
			MethodName = mehtodInfo.Name;
			AttributeInfoCaches = new HashSet<AttributeInfoCache>(mehtodInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new AttributeInfoCache(s as Attribute)));
			ArgumentInfoCaches = new HashSet<MethodArgsInfoCache>(mehtodInfo.GetParameters().Select(s => new MethodArgsInfoCache(s)));
			return this;
		}

		/// <summary>
		///     if set this method does not exist so we fake it
		/// </summary>
		public Delegate Delegate { get; set; }

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
		public HashSet<MethodArgsInfoCache> ArgumentInfoCaches { get; set; }

		/// <summary>
		///     All Attributes on this Method
		/// </summary>
		public HashSet<AttributeInfoCache> AttributeInfoCaches { get; private set; }

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
	}
}