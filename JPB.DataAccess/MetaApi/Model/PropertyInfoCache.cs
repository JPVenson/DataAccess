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
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.MetaApi.Contract;
using JPB.DataAccess.MetaApi.Model.Equatable;

namespace JPB.DataAccess.MetaApi.Model
{
	[DebuggerDisplay("{MethodName}")]
	[Serializable]
	internal class PropertyHelper<TAtt> : MethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>>
		where TAtt : class, IAttributeInfoCache, new()
	{
		private dynamic _getter;
		private dynamic _setter;

		public PropertyHelper(MethodBase accessorMethod)
		{
			base.MethodInfo = accessorMethod;
			base.MethodName = accessorMethod.Name;
		}

		public void SetGet(dynamic getter)
		{
			_getter = getter;
		}

		public void SetSet(dynamic setter)
		{
			_setter = setter;
		}

		public override object Invoke(dynamic target, params dynamic[] param)
		{
			if (_getter != null)
			{
				return _getter(target);
			}
			var paramOne = param[0];
			var result = _setter(target, paramOne);
			return result;
		}
	}

	/// <summary>
	///     Infos about the Property
	/// </summary>
	[DebuggerDisplay("{PropertyName}")]
	[Serializable]
	public class PropertyInfoCache<TAtt> : IPropertyInfoCache<TAtt>
		where TAtt : class, IAttributeInfoCache, new()
	{
		/// <summary>
		/// </summary>
		internal PropertyInfoCache(PropertyInfo propertyInfo, bool anon)
		{
			Init(propertyInfo, anon);
		}

		/// <summary>
		/// For internal use Only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public PropertyInfoCache()
		{
		}

		public virtual IPropertyInfoCache<TAtt> Init(PropertyInfo propertyInfo, bool anon)
		{
			if (!string.IsNullOrEmpty(PropertyName))
				throw new InvalidOperationException("The object is already Initialed. A Change is not allowed");

			AttributeInfoCaches = new HashSet<TAtt>();
			if (propertyInfo != null)
			{
				var getMethod = propertyInfo.GetMethod;
				var setMethod = propertyInfo.SetMethod;
				PropertyInfo = propertyInfo;
				PropertyName = propertyInfo.Name;
				PropertyType = propertyInfo.PropertyType;


				if (!anon && (getMethod != null || setMethod != null))
				{
					var isStatic = getMethod != null
						? getMethod.Attributes.HasFlag(MethodAttributes.Static)
						: setMethod.Attributes.HasFlag(MethodAttributes.Static);
					var builder = typeof(Expression)
						.GetMethods()
						.First(s => s.Name == "Lambda" && s.ContainsGenericParameters);

					if (isStatic)
					{
						GetterDelegate = typeof(Func<>).MakeGenericType(propertyInfo.PropertyType);
						SetterDelegate = typeof(Action<>).MakeGenericType(propertyInfo.PropertyType);
						var accessField = Expression.Property(null, propertyInfo);

						if (getMethod != null)
						{
							var getExpression = builder
								.MakeGenericMethod(GetterDelegate)
								.Invoke(null, new object[]
						{
							accessField,null
						}) as dynamic;

							var getterDelegate = getExpression.Compile();
							Getter = new PropertyHelper<TAtt>(getMethod);
							((PropertyHelper<TAtt>)Getter).SetGet(getterDelegate);
						}
						if (setMethod != null)
						{
							var valueRef = Expression.Parameter(propertyInfo.PropertyType, "newValue");
							var setter = Expression.Assign(
								accessField,
								valueRef);

							var setExpression = builder
								.MakeGenericMethod(SetterDelegate)
								.Invoke(null, new object[]
						{
							setter,
							new[]
							{
								valueRef
							}
						}) as dynamic;

							var setterDelegate = setExpression.Compile();
							Setter = new PropertyHelper<TAtt>(setMethod);
							((PropertyHelper<TAtt>)Setter).SetSet(setterDelegate);
						}
					}
					else
					{
						GetterDelegate = typeof(Func<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
						SetterDelegate = typeof(Func<,,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType, propertyInfo.DeclaringType);
						var thisRef = Expression.Parameter(propertyInfo.DeclaringType, "that");

						var accessField = Expression.MakeMemberAccess(thisRef, propertyInfo);

						if (getMethod != null)
						{
							var getExpression = builder
								.MakeGenericMethod(GetterDelegate)
								.Invoke(null, new object[]
						{
							accessField,
							new[] {thisRef}
						}) as dynamic;

							var getterDelegate = getExpression.Compile();
							Getter = new PropertyHelper<TAtt>(getMethod);
							((PropertyHelper<TAtt>)Getter).SetGet(getterDelegate);
						}
						if (setMethod != null)
						{
							var valueRef = Expression.Parameter(propertyInfo.PropertyType, "newValue");
							var setter = Expression.Assign(
								accessField,
								valueRef);
							var makeRetunLabel = Expression.Label(propertyInfo.DeclaringType);
							var retunLabel = Expression.Label(makeRetunLabel, Expression.Default(propertyInfo.DeclaringType));
							var returnMaybeValueType = Expression.Return(makeRetunLabel, thisRef);

							var setExpression = builder
								.MakeGenericMethod(SetterDelegate)
								.Invoke(null, new object[]
						{
							Expression.Block(setter, returnMaybeValueType,retunLabel), 
							new[] {thisRef, valueRef}
						}) as dynamic;

							var setterDelegate = setExpression.Compile();
							Setter = new PropertyHelper<TAtt>(setMethod);
							((PropertyHelper<TAtt>)Setter).SetSet(setterDelegate);
						}
					}
				}
				else
				{
					if (getMethod != null)
						Getter = new MethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>>(getMethod);
					if (setMethod != null)
						Setter = new MethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>>(setMethod);
				}

				AttributeInfoCaches = new HashSet<TAtt>(propertyInfo
					.GetCustomAttributes(true)
					.Where(s => s is Attribute)
					.Select(s => new TAtt().Init(s as Attribute) as TAtt));
			}

			return this;
		}

		/// <summary>
		///     the type of the Setter delegate
		/// </summary>
		public Type SetterDelegate { get; protected internal set; }

		/// <summary>
		///     the type of the Getter delegate
		/// </summary>
		public Type GetterDelegate { get; protected internal set; }

		/// <summary>
		///     The Setter mehtod can be null
		/// </summary>
		public IMethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>> Setter { get; protected internal set; }

		/// <summary>
		///     The Getter Method can be null
		/// </summary>
		public IMethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>> Getter { get; protected internal set; }

		/// <summary>
		///     The return type of the property
		/// </summary>
		public Type PropertyType { get; protected internal set; }

		/// <summary>
		///     Direct Reflection
		/// </summary>
		public PropertyInfo PropertyInfo { get; protected internal set; }

		//public ClassInfoCache<PropertyInfoCache, AttributeInfoCache, MethodInfoCache, ConstructorInfoCache> PropertyTypeInfo { get; set; }

		/// <summary>
		///     The name of the Property
		/// </summary>
		public string PropertyName { get; protected internal set; }

		/// <summary>
		///     All Attributes on this Property
		/// </summary>
		public HashSet<TAtt> AttributeInfoCaches { get; protected internal set; }

		public int CompareTo(IPropertyInfoCache<TAtt> other)
		{
			return new PropertyEquatableComparer<TAtt>().Compare(this, other);
		}

		public bool Equals(IPropertyInfoCache<TAtt> other)
		{
			return new PropertyEquatableComparer<TAtt>().Equals(this, other);
		}

		public override int GetHashCode()
		{
			return new PropertyEquatableComparer<TAtt>().GetHashCode(this);
		}

		// returns property getter
		internal static Delegate GetPropGetter(Type delegateType, Type typeOfObject, string propertyName)
		{
			var paramExpression = Expression.Parameter(typeOfObject, "value");
			var propertyGetterExpression = Expression.Property(paramExpression, propertyName);
			return Expression.Lambda(delegateType, propertyGetterExpression, paramExpression).Compile();
		}

		// returns property setter:
		internal static Delegate GetPropSetter(Type delegateType, Type typeOfObject, Type typeOfProperty, string propertyName)
		{
			var paramExpression = Expression.Parameter(typeOfObject);
			var paramExpression2 = Expression.Parameter(typeOfProperty, propertyName);
			var propertyGetterExpression = Expression.Property(paramExpression, propertyName);
			return
				Expression.Lambda(delegateType, Expression.Assign(propertyGetterExpression, paramExpression2), paramExpression,
					paramExpression2)
					.Compile();
		}
	}

	[Serializable]
	internal class PropertyInfoCache<T, TE, TAtt> : PropertyInfoCache<TAtt> where TAtt : class, IAttributeInfoCache, new()
	{
		internal PropertyInfoCache(string name, Action<T, TE> setter = null, Func<T, TE> getter = null,
			params AttributeInfoCache[] attributes)
		{
			if (attributes == null)
				throw new ArgumentNullException("attributes");

			PropertyName = name;

			if (setter != null)
			{
				Setter = new MethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>>((o, objects) =>
				{
					setter((T)o, (TE)objects[0]);
					return null;
				});
			}

			if (getter != null)
			{
				Getter = new MethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>>((o, objects) => getter((T)o));
			}
		}
	}
}