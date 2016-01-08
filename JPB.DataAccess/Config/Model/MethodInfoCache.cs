using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JPB.DataAccess.Config.Model
{
	/// <summary>
	///     Infos about the Method
	/// </summary>
	public class MethodInfoCache : IComparable<MethodInfoCache>
	{
		internal MethodInfoCache(MethodInfo mehtodInfo)
		{
			if (mehtodInfo == null)
				throw new ArgumentNullException("mehtodInfo");
			MethodInfo = mehtodInfo;
			MethodName = mehtodInfo.Name;
			AttributeInfoCaches = new HashSet<AttributeInfoCache>(mehtodInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new AttributeInfoCache(s as Attribute)));
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

		protected internal MethodInfoCache()
		{
			AttributeInfoCaches = new HashSet<AttributeInfoCache>();
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
		///     All Attributes on this Method
		/// </summary>
		public HashSet<AttributeInfoCache> AttributeInfoCaches { get; private set; }

		public int CompareTo(MethodInfoCache other)
		{
			return String.Compare(MethodName, other.MethodName, StringComparison.Ordinal);
		}

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
				for (int i = 0; i < param.Length; i++)
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