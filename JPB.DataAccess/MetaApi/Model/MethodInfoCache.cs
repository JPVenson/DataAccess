using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
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
		internal MethodInfoCache(MethodBase mehtodInfo)
		{
// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Init(mehtodInfo);
		}

		internal MethodInfoCache(Func<object, object[], object> fakeMehtod, string name = null, params TAtt[] attributes) 
			: this()
		{
			Init(fakeMehtod.GetMethodInfo());
		}

		/// <summary>
		/// For internal use Only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public MethodInfoCache()
		{
			AttributeInfoCaches = new HashSet<TAtt>();
			Arguments = new HashSet<TArg>();
		}
		// ReSharper restore DoNotCallOverridableMethodsInConstructor

		/// <summary>
		/// For Internal use Only
		/// </summary>
		/// <param name="mehtodInfo"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IMethodInfoCache<TAtt, TArg> Init(MethodBase mehtodInfo)
		{
			return this.Init(mehtodInfo, mehtodInfo.DeclaringType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mehtodInfo"></param>
		/// <param name="sourceType"></param>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IMethodInfoCache<TAtt, TArg> Init(MethodBase mehtodInfo, Type sourceType)
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
			_createMethod = new Lazy<Func<object, object[], object>>(() => Wrap((MethodInfo)mehtodInfo, sourceType));
			return this;
		}

		static Func<object, object[], object> Wrap(MethodInfo method, Type declaringType)
		{
			var dm = new DynamicMethod(method.Name, typeof(object), new[] { typeof(object), typeof(object[]) }, declaringType, true);
			var il = dm.GetILGenerator();

			if (!method.IsStatic)
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Unbox_Any, declaringType);
			}
			var parameters = method.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldc_I4, i);
				il.Emit(OpCodes.Ldelem_Ref);
				il.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
			}
			il.Emit(OpCodes.Call, method);
			//il.EmitCall(method.IsStatic || declaringType.IsValueType ? OpCodes.Call : OpCodes.Callvirt, method, null);
			if (method.ReturnType == null || method.ReturnType == typeof(void))
			{
				il.Emit(OpCodes.Ldnull);
			}
			else if (method.ReturnType.IsValueType)
			{
				il.Emit(OpCodes.Box, method.ReturnType);
			}
			il.Emit(OpCodes.Ret);
			return (Func<object, object[], object>)dm.CreateDelegate(typeof(Func<object, object[], object>));
		}

		private Lazy<Func<object, object[], object>> _createMethod;
		/// <summary>
		///     if set this method does not exist so we fake it
		/// </summary>
		public virtual Func<object, object[], object> Delegate {
			get { return _createMethod.Value; } 
		}

		/// <summary>
		///     Direct Reflection
		/// </summary>
		public virtual MethodBase MethodInfo { get; protected internal set; }

		/// <summary>
		///     The name of the method
		/// </summary>
		public virtual string MethodName { get; protected internal set; }

		/// <summary>
		/// Arguments on this Method
		/// </summary>
		public virtual HashSet<TArg> Arguments { get; protected internal set; }

		/// <summary>
		///     All Attributes on this Method
		/// </summary>
		public virtual HashSet<TAtt> AttributeInfoCaches { get; protected internal set; }

		/// <summary>
		///     Easy access to the underlying delegate
		/// </summary>
		/// <returns></returns>
		public virtual object Invoke(object target, params object[] param)
		{
			if (Delegate != null)
			{
				return Delegate(target, param);
			}
			return MethodInfo.Invoke(target, param);
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