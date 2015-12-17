using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JPB.DataAccess.Config.Model
{
	public class MethodInfoCache
	{
		public MethodInfoCache(MethodInfo mehtodInfo)
		{
			this.AttributeInfoCaches = new List<AttributeInfoCache>();
			if (mehtodInfo != null)
			{
				MethodInfo = mehtodInfo;
				MethodName = mehtodInfo.Name;
				Delegate = ExtractDelegate(MethodInfo);
				this.AttributeInfoCaches = mehtodInfo
					.GetCustomAttributes(true)
					.Where(s => s is Attribute)
					.Select(s => new AttributeInfoCache(s as Attribute))
					.ToList();
			}
		}

		public MethodInfoCache(Delegate fakeMehtod)
		{
			this.AttributeInfoCaches = new List<AttributeInfoCache>();
			if (fakeMehtod != null)
			{
				MethodInfo = fakeMehtod.GetMethodInfo();
				MethodName = MethodInfo.Name;
				Delegate = fakeMehtod;
				this.AttributeInfoCaches = MethodInfo
					.GetCustomAttributes(true)
					.Where(s => s is Attribute)
					.Select(s => new AttributeInfoCache(s as Attribute))
					.ToList();
			}
		}

		public object Invoke(object target, params object[] param)
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

		public Delegate Delegate { get; set; }
		public MethodInfo MethodInfo { get; private set; }
		public string MethodName { get; private set; }
		public List<AttributeInfoCache> AttributeInfoCaches { get; private set; }

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
	}
}
