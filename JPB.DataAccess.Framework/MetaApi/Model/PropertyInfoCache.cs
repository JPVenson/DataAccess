#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model.Equatable;

#endregion

namespace JPB.DataAccess.MetaApi.Model
{
	/// <summary>
	///     Infos about the Property
	/// </summary>
	[DebuggerDisplay("{" + nameof(PropertyName) + "}")]
	[Serializable]
	public class PropertyInfoCache<TAtt> : IPropertyInfoCache<TAtt>
		where TAtt : class, IAttributeInfoCache, new()
	{
		/// <summary>
		///     For internal use Only
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public PropertyInfoCache()
		{
			Attributes = new HashSet<TAtt>();
		}

		/// <summary>
		///     Internal use Only
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <param name="anon"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">The object is already Initialed. A Change is not allowed</exception>
		public virtual IPropertyInfoCache<TAtt> Init(PropertyInfo propertyInfo, bool anon)
		{
			if (!string.IsNullOrEmpty(PropertyName))
			{
				throw new InvalidOperationException("The object is already Initialed. A Change is not allowed");
			}

			Attributes.Clear();
			if (propertyInfo == null)
			{
				return this;
			}

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
				var builder = MetaInfoStoreExtentions.GetExpressionLambda();

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
								accessField, null
							});

						var getterDelegate = MetaInfoStoreExtentions
							.GetCompileMethodFromExpression(getExpression.GetType())
							.Invoke(getExpression, null);

						Getter = new PropertyHelper<TAtt>(getMethod);
						((PropertyHelper<TAtt>) Getter).SetGet(getterDelegate);
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
							});

						var setterDelegate = MetaInfoStoreExtentions
							.GetCompileMethodFromExpression(setExpression.GetType())
							.Invoke(setExpression, null);

						Setter = new PropertyHelper<TAtt>(setMethod);
						((PropertyHelper<TAtt>) Setter).SetSet(setterDelegate);
					}
				}
				else
				{
					GetterDelegate =
						typeof(Func<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
					SetterDelegate = typeof(Func<,,>).MakeGenericType(propertyInfo.DeclaringType,
						propertyInfo.PropertyType,
						propertyInfo.DeclaringType);
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
						((PropertyHelper<TAtt>) Getter).SetGet(getterDelegate);
					}

					if (setMethod != null)
					{
						Setter = new PropertyHelper<TAtt>(setMethod);
						try
						{
							var valueRef = Expression.Parameter(propertyInfo.PropertyType, "newValue");
							var setter = Expression.Assign(
								accessField,
								valueRef);
							var makeRetunLabel = Expression.Label(propertyInfo.DeclaringType);
							var retunLabel = Expression.Label(makeRetunLabel,
								Expression.Default(propertyInfo.DeclaringType));
							var returnMaybeValueType = Expression.Return(makeRetunLabel, thisRef);

							var setExpression = builder
								.MakeGenericMethod(SetterDelegate)
								.Invoke(null, new object[]
								{
									Expression.Block(setter, returnMaybeValueType, retunLabel),
									new[] {thisRef, valueRef}
								}) as dynamic;

							var setterDelegate = setExpression.Compile();
							((PropertyHelper<TAtt>) Setter).SetSet(setterDelegate);
						}
						catch (Exception)
						{
							//sometimes the Compile method does throw an error. In this case fall back to reflection
							var setDelegate = setMethod.CreateDelegate(SetterDelegate);
							((PropertyHelper<TAtt>)Setter).SetSet(setDelegate);
						}
					}
				}
			}
			else
			{
				if (getMethod != null)
				{
					Getter = new MethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>>();
					Getter.Init(getMethod);
				}

				if (setMethod != null)
				{
					Setter = new MethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>>();
					Setter.Init(setMethod);
				}
			}

			Attributes = new HashSet<TAtt>(propertyInfo
				.GetCustomAttributes(true)
				.Where(s => s is Attribute)
				.Select(s => new TAtt().Init(s as Attribute) as TAtt));

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
		public HashSet<TAtt> Attributes { get; protected internal set; }

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
		public int CompareTo(IPropertyInfoCache<TAtt> other)
		{
			return new PropertyEquatableComparer<TAtt>().Compare(this, other);
		}

		/// <summary>
		///     Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(IPropertyInfoCache<TAtt> other)
		{
			return new PropertyEquatableComparer<TAtt>().Equals(this, other);
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
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
		internal static Delegate GetPropSetter(Type delegateType, Type typeOfObject, Type typeOfProperty,
			string propertyName)
		{
			var paramExpression = Expression.Parameter(typeOfObject);
			var paramExpression2 = Expression.Parameter(typeOfProperty, propertyName);
			var propertyGetterExpression = Expression.Property(paramExpression, propertyName);
			return
				Expression.Lambda(delegateType, Expression.Assign(propertyGetterExpression, paramExpression2),
						paramExpression,
						paramExpression2)
					.Compile();
		}
	}

	[Serializable]
	internal class PropertyInfoCache<T, TE, TAtt> : PropertyInfoCache<TAtt>
		where TAtt : class, IAttributeInfoCache, new()
	{
		internal PropertyInfoCache(string name, Action<T, TE> setter = null, Func<T, TE> getter = null,
			params AttributeInfoCache[] attributes)
		{
			if (attributes == null)
			{
				throw new ArgumentNullException(nameof(attributes));
			}

			PropertyName = name;

			if (setter != null)
			{
				Setter = new MethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>>();
				Setter.Init(new Func<object, object[], object>((o, objects) =>
				{
					setter((T) o, (TE) objects[0]);
					return null;
				}).Method);
			}

			if (getter != null)
			{
				Getter = new MethodInfoCache<TAtt, MethodArgsInfoCache<TAtt>>();
				Setter.Init(new Func<object, object[], object>((o, objects) => getter((T)o)).Method);
			}
		}
	}
}