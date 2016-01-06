using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JPB.DataAccess.Config.Model
{
	/// <summary>
	/// Infos about the Method
	/// </summary>
	public class MethodInfoCache : IComparable<MethodInfoCache>
	{
		internal MethodInfoCache(MethodInfo mehtodInfo)
		{
			if (mehtodInfo == null) 
				throw new ArgumentNullException("mehtodInfo");
			MethodInfo = mehtodInfo;
			MethodName = mehtodInfo.Name;
			this.AttributeInfoCaches = new HashSet<AttributeInfoCache>(mehtodInfo
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
			this.AttributeInfoCaches = new HashSet<AttributeInfoCache>(MethodInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new AttributeInfoCache(s as Attribute)).Concat(attributes));
		}

		internal protected MethodInfoCache()
		{
			this.AttributeInfoCaches = new HashSet<AttributeInfoCache>();
		}

		/// <summary>
		/// Easy access to the underlying delegate
		/// </summary>
		/// <param name="target"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		public virtual object Invoke(object target, params object[] param)
		{
			if (this.Delegate != null)
			{
				var para = new object[param.Length + 1];
				para[0] = target;
				for (int i = 0; i < param.Length; i++)
				{
					para[1 + i] = param[i];
				}
				return this.Delegate.DynamicInvoke(para);
			}
			else
			{
				return this.MethodInfo.Invoke(target, param);
			}
		}

		/// <summary>
		/// if set this method does not exist so we fake it
		/// </summary>
		public Delegate Delegate { get; set; }

		/// <summary>
		/// Direct Reflection 
		/// </summary>
		public MethodInfo MethodInfo { get; private set; }

		/// <summary>
		/// The name of the method
		/// </summary>
		public string MethodName { get; private set; }

		/// <summary>
		/// All Attributes on this Method
		/// </summary>
		public HashSet<AttributeInfoCache> AttributeInfoCaches { get; private set; }

		internal static Delegate ExtractDelegate(MethodInfo method)
		{
			List<Type> args = new List<Type>(
				method.GetParameters().Select(p => p.ParameterType));
			Type delegateType;
			if (method.ReturnType == typeof(void))
			{
				delegateType = Expression.GetActionType(args.ToArray());
			}
			else {
				args.Add(method.ReturnType);
				delegateType = Expression.GetFuncType(args.ToArray());
			}
			return Delegate.CreateDelegate(delegateType, null, method);           
		}

		public int CompareTo(MethodInfoCache other)
		{
			return System.String.Compare(this.MethodName, other.MethodName, System.StringComparison.Ordinal);
		}
	}
}
