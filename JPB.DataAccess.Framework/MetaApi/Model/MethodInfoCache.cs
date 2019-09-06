#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model.Equatable;

#endregion

namespace JPB.DataAccess.MetaApi.Model
{
	/// <summary>
	///     Infos about the Method on an Class. The given Delegate to the Function is not stored.
	///     This IL Body will be extracted and a new Function will be created on runtime for each calling function.
	///     Use the <code>FakeMethodInfoCache</code> to create a direct delgate cache that will reuse the delegate pointer and
	///     the declaring class
	/// </summary>
	[DebuggerDisplay("{" + nameof(MethodName) + "}")]
	[Serializable]
	public class MethodInfoCache<TAtt, TArg> :
		IMethodInfoCache<TAtt, TArg>
		where TAtt : class, IAttributeInfoCache, new()
		where TArg : class, IMethodArgsInfoCache<TAtt>, new()
	{
		private Lazy<Func<object, object[], object>> _createMethod;

		/// <summary>
		///     Contains the C# Return type of this Instance if known.
		///     Can Be null
		/// </summary>
		public virtual Type ReturnType { get; protected internal set; }

		/// <summary>
		///     For Internal use Only
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
			return Init(mehtodInfo, mehtodInfo.DeclaringType);
		}

		/// <summary>
		///     For Internal use Only
		/// </summary>
		/// <param name="mehtodInfo"></param>
		/// <param name="sourceType"></param>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IMethodInfoCache<TAtt, TArg> Init(MethodBase mehtodInfo, Type sourceType)
		{
			return Init(mehtodInfo, sourceType, null);
		}

		/// <summary>
		///     if set this method does not exist so we fake it
		/// </summary>
		public virtual Func<object, object[], object> Delegate
		{
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
		///     Arguments on this Method
		/// </summary>
		public virtual HashSet<TArg> Arguments { get; protected internal set; }

		/// <summary>
		///     All Attributes on this Method
		/// </summary>
		public virtual HashSet<TAtt> Attributes { get; protected internal set; }

		/// <summary>
		///     Does not use the Original Delegate. Instad uses IL injection to create a new Delegate
		/// </summary>
		public bool UseILWrapper { get; set; }

		/// <summary>
		///     Easy access to the underlying delegate
		/// </summary>
		/// <returns></returns>
		public virtual object Invoke(object target, params object[] param)
		{
			if (UseILWrapper && Delegate != null)
			{
				return Delegate(target, param);
			}

			return MethodInfo.Invoke(target, param);
		}

		/// <summary>
		///     Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(IMethodInfoCache<TAtt, TArg> other)
		{
			return new MethodInfoCacheEquatableComparer<TAtt, TArg>().Equals(this, other);
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
		public int CompareTo(IMethodInfoCache<TAtt, TArg> other)
		{
			return new MethodInfoCacheEquatableComparer<TAtt, TArg>().Compare(this, other);
		}

		/// <summary>
		///     For Internal use Only
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual IMethodInfoCache<TAtt, TArg> Init(Func<object, object[], object> fakeMehtod)
		{
			return Init(fakeMehtod.GetMethodInfo());
		}

		/// <summary>
		///     For Internal use Only
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual IMethodInfoCache<TAtt, TArg> Init(Func<object, object[], object> fakeMehtod, Type declaringType, string name = null)
		{
			return Init(fakeMehtod.GetMethodInfo(), declaringType, name);
		}

		/// <summary>
		///     For Internal use Only
		/// </summary>
		/// <param name="mehtodInfo"></param>
		/// <param name="sourceType"></param>
		/// <param name="name">User name for this Mehtod.</param>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		// ReSharper disable once MethodOverloadWithOptionalParameter
		public virtual IMethodInfoCache<TAtt, TArg> Init(MethodBase mehtodInfo, Type sourceType, string name = null)
		{
			UseILWrapper = false;

			if (!string.IsNullOrEmpty(MethodName))
			{
				throw new InvalidOperationException("The object is already Initialed. A Change is not allowed");
			}

			if (mehtodInfo == null)
			{
				throw new ArgumentNullException(nameof(mehtodInfo));
			}

			MethodInfo = mehtodInfo;
			if (mehtodInfo is MethodInfo)
			{
				ReturnType = (mehtodInfo as MethodInfo).ReturnType;
			}

			if (string.IsNullOrEmpty(name))
			{
				MethodName = mehtodInfo.Name;
			}
			else
			{
				MethodName = name;
			}

			Attributes = new HashSet<TAtt>(mehtodInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new TAtt().Init(s as Attribute) as TAtt));
			Arguments = new HashSet<TArg>(mehtodInfo.GetParameters().Select(s => new TArg().Init(s) as TArg));
			_createMethod = new Lazy<Func<object, object[], object>>(() => Wrap((MethodInfo) mehtodInfo, sourceType));
			return this;
		}

		private static Func<object, object[], object> Wrap(MethodBase method, Type declaringType)
		{
			return (instance, args) => method.Invoke(instance, args);

			//var dm = new DynamicMethod(method.Name, typeof(object), new[] {typeof(object), typeof(object[])}, declaringType, true);
			//var il = dm.GetILGenerator();

			//if (!method.IsStatic)
			//{
			//	il.Emit(OpCodes.Ldarg_0);
			//	il.Emit(OpCodes.Unbox_Any, declaringType);
			//}
			//var parameters = method.GetParameters();
			//for (var i = 0; i < parameters.Length; i++)
			//{
			//	il.Emit(OpCodes.Ldarg_1);
			//	il.Emit(OpCodes.Ldc_I4, i);
			//	il.Emit(OpCodes.Ldelem_Ref);
			//	il.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
			//}
			//if (method is MethodInfo)
			//{
			//	var methodInfo = method as MethodInfo;
			//	il.EmitCall(method.IsStatic || declaringType.IsValueType ? OpCodes.Call : OpCodes.Callvirt, methodInfo, null);
			//	if (methodInfo.ReturnType == null || methodInfo.ReturnType == typeof(void))
			//	{
			//		il.Emit(OpCodes.Ldnull);
			//	}
			//	else if (methodInfo.ReturnType.IsValueType)
			//	{
			//		il.Emit(OpCodes.Box, methodInfo.ReturnType);
			//	}
			//}
			//else if (method is ConstructorInfo)
			//{
			//	var ctorInfo = method as ConstructorInfo;
			//	il.Emit(OpCodes.Newobj, ctorInfo);
			//}

			//il.Emit(OpCodes.Ret);
			//return (Func<object, object[], object>) dm.CreateDelegate(typeof(Func<object, object[], object>));
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode()
		{
			return new MethodInfoCacheEquatableComparer<TAtt, TArg>().GetHashCode(this);
		}

		/// <summary>
		///     For internal use Only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public MethodInfoCache()
		{
			Attributes = new HashSet<TAtt>();
			Arguments = new HashSet<TArg>();
		}

		// ReSharper restore DoNotCallOverridableMethodsInConstructor
	}
}