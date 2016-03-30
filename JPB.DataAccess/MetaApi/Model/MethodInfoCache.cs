﻿/*
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
using System.Reflection.Emit;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model.Equatable;

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
		private Lazy<Func<object, object[], object>> _createMethod;

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
			UseILWrapper = true;
			return Init(mehtodInfo, mehtodInfo.DeclaringType);
		}

		/// <summary>
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
			Attributes = new HashSet<TAtt>(mehtodInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new TAtt().Init(s as Attribute) as TAtt));
			Arguments = new HashSet<TArg>(mehtodInfo.GetParameters().Select(s => new TArg().Init(s) as TArg));
			_createMethod = new Lazy<Func<object, object[], object>>(() => Wrap((MethodInfo) mehtodInfo, sourceType));
			return this;
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

		public bool Equals(IMethodInfoCache<TAtt, TArg> other)
		{
			return new MethodInfoCacheEquatableComparer<TAtt, TArg>().Equals(this, other);
		}

		public int CompareTo(IMethodInfoCache<TAtt, TArg> other)
		{
			return new MethodInfoCacheEquatableComparer<TAtt, TArg>().Compare(this, other);
		}

		//static Func<object, object[], object> Wrap(MethodBase method, Type declaringType)
		//{
		//	var dynAssmName = new AssemblyName("DynamicMethodCacheAssam" + declaringType.Name);
		//	var dynAssm = AppDomain.CurrentDomain.DefineDynamicAssembly(dynAssmName, AssemblyBuilderAccess.Run);
		//	var dynModule = dynAssm.DefineDynamicModule("DynamicMethodModule" + declaringType.Name);

		//	var dynType = dynModule.DefineType("DynamicWrapperType" + declaringType.Name, TypeAttributes.Public);
		//	var dm = dynType.DefineMethod("InvokeWrapper" + declaringType.Name, MethodAttributes.Public);
		//	var il = dm.GetILGenerator();

		//	if (method.ContainsGenericParameters)
		//	{
		//		for (int index = 0; index < method.GetGenericArguments().Length; index++)
		//		{
		//			var genericArgument = method.GetGenericArguments()[index];
		//			var genericTypeParameterBuilders = dm.DefineGenericParameters(genericArgument.Name);
		//			il.DeclareLocal(genericTypeParameterBuilders[0]);
		//		}
		//	}

		//	if (!method.IsStatic)
		//	{
		//		il.Emit(OpCodes.Ldarg_0);
		//		il.Emit(OpCodes.Unbox_Any, declaringType);
		//	}
		//	var parameters = method.GetParameters();
		//	for (int i = 0; i < parameters.Length; i++)
		//	{
		//		il.Emit(OpCodes.Ldarg_1);
		//		il.Emit(OpCodes.Ldc_I4, i);
		//		il.Emit(OpCodes.Ldelem_Ref);
		//		il.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
		//	}
		//	if (method is MethodInfo)
		//	{
		//		var methodInfo = method as MethodInfo;
		//		il.EmitCall(method.IsStatic || declaringType.IsValueType ? OpCodes.Call : OpCodes.Callvirt, methodInfo, null);
		//		if (methodInfo.ReturnType == null || methodInfo.ReturnType == typeof(void))
		//		{
		//			il.Emit(OpCodes.Ldnull);
		//		}
		//		else if (methodInfo.ReturnType.IsValueType)
		//		{
		//			il.Emit(OpCodes.Box, methodInfo.ReturnType);
		//		}
		//	}
		//	else if (method is ConstructorInfo)
		//	{
		//		var ctorInfo = method as ConstructorInfo;
		//		il.Emit(OpCodes.Newobj, ctorInfo);
		//	}

		//	il.Emit(OpCodes.Ret);
		//	var createType = dynType.CreateType();

		//	return (Func<object, object[], object>)dm.CreateDelegate(typeof(Func<object, object[], object>));
		//}

		private static Func<object, object[], object> Wrap(MethodBase method, Type declaringType)
		{
			var dm = new DynamicMethod(method.Name, typeof (object), new[] {typeof (object), typeof (object[])}, declaringType,
				true);
			var il = dm.GetILGenerator();

			if (!method.IsStatic)
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Unbox_Any, declaringType);
			}
			var parameters = method.GetParameters();
			for (var i = 0; i < parameters.Length; i++)
			{
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldc_I4, i);
				il.Emit(OpCodes.Ldelem_Ref);
				il.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
			}
			if (method is MethodInfo)
			{
				var methodInfo = method as MethodInfo;
				il.EmitCall(method.IsStatic || declaringType.IsValueType ? OpCodes.Call : OpCodes.Callvirt, methodInfo, null);
				if (methodInfo.ReturnType == null || methodInfo.ReturnType == typeof (void))
				{
					il.Emit(OpCodes.Ldnull);
				}
				else if (methodInfo.ReturnType.IsValueType)
				{
					il.Emit(OpCodes.Box, methodInfo.ReturnType);
				}
			}
			else if (method is ConstructorInfo)
			{
				var ctorInfo = method as ConstructorInfo;
				il.Emit(OpCodes.Newobj, ctorInfo);
			}

			il.Emit(OpCodes.Ret);
			return (Func<object, object[], object>) dm.CreateDelegate(typeof (Func<object, object[], object>));
		}

		public override int GetHashCode()
		{
			return new MethodInfoCacheEquatableComparer<TAtt, TArg>().GetHashCode(this);
		}

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