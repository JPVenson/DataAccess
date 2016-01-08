using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Config.Model
{
	internal class PropertyHelper : MethodInfoCache
	{
		private dynamic _getter;
		private dynamic _setter;

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
			dynamic paramOne = param[0];
			_setter(target, paramOne);
			return null;
		}
	}

	/// <summary>
	///     Infos about the Property
	/// </summary>
	public class PropertyInfoCache : IComparable<PropertyInfoCache>
	{
		/// <summary>
		/// </summary>
		internal PropertyInfoCache(PropertyInfo propertyInfo, bool anon)
		{
			AttributeInfoCaches = new HashSet<AttributeInfoCache>();
			if (propertyInfo != null)
			{
				PropertyInfo = propertyInfo;
				PropertyName = propertyInfo.Name;
				PropertyType = propertyInfo.PropertyType;

				GetterDelegate = typeof (Func<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
				SetterDelegate = typeof (Action<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
				if (!anon)
				{
					MethodInfo builder = typeof (Expression)
						.GetMethods()
						.First(s => s.Name == "Lambda" && s.ContainsGenericParameters);

					ParameterExpression thisRef = Expression.Parameter(propertyInfo.DeclaringType, "that");
					MemberExpression accessField = Expression.MakeMemberAccess(thisRef, propertyInfo);

					var getExpression = builder
						.MakeGenericMethod(GetterDelegate)
						.Invoke(null, new object[]
						{
							accessField,
							new[] {thisRef}
						}) as dynamic;

					ParameterExpression valueRef = Expression.Parameter(propertyInfo.PropertyType, "newValue");
					BinaryExpression setter = Expression.Assign(
						accessField,
						valueRef);

					var setExpression = builder
						.MakeGenericMethod(SetterDelegate)
						.Invoke(null, new object[]
						{
							setter,
							new[] {thisRef, valueRef}
						}) as dynamic;

					dynamic getterDelegate = getExpression.Compile();
					Getter = new PropertyHelper();
					((PropertyHelper) Getter).SetGet(getterDelegate);

					dynamic setterDelegate = setExpression.Compile();
					Setter = new PropertyHelper();
					((PropertyHelper) Setter).SetSet(setterDelegate);
				}
				else
				{
					MethodInfo getter = PropertyInfo.GetGetMethod();
					MethodInfo setter = PropertyInfo.GetSetMethod();
					if (getter != null)
						Getter = new MethodInfoCache(getter);
					if (setter != null)
						Setter = new MethodInfoCache(setter);
				}

				//Getter = new MethodInfoCache(PropertyInfo.GetGetMethod().CreateDelegate(GetterDelegate));
				//Setter = new MethodInfoCache(PropertyInfo.GetSetMethod().CreateDelegate(SetterDelegate));

				//Getter = new MethodInfoCache(GetPropGetter(GetterDelegate, propertyInfo.DeclaringType, PropertyName));
				//Setter = new MethodInfoCache(GetPropSetter(SetterDelegate, propertyInfo.DeclaringType, targetType, PropertyName));

				AttributeInfoCaches = new HashSet<AttributeInfoCache>(propertyInfo
					.GetCustomAttributes(true)
					.Where(s => s is Attribute)
					.Select(s => new AttributeInfoCache(s as Attribute)));

				Refresh();
			}
		}

		protected PropertyInfoCache()
		{
			AttributeInfoCaches = new HashSet<AttributeInfoCache>();
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
		public MethodInfoCache Setter { get; protected internal set; }

		/// <summary>
		///     The Getter Method can be null
		/// </summary>
		public MethodInfoCache Getter { get; protected internal set; }

		/// <summary>
		///     The return type of the property
		/// </summary>
		public Type PropertyType { get; protected internal set; }

		/// <summary>
		///     Direct Reflection
		/// </summary>
		public PropertyInfo PropertyInfo { get; protected internal set; }

		/// <summary>
		///     The name of the Property
		/// </summary>
		public string PropertyName { get; protected internal set; }

		/// <summary>
		///     All Attributes on this Property
		/// </summary>
		public HashSet<AttributeInfoCache> AttributeInfoCaches { get; protected internal set; }

		/// <summary>
		///     if known the ForModel attribute
		/// </summary>
		public AttributeInfoCache ForModel { get; protected internal set; }

		/// <summary>
		///     if known the ForXml attribute
		/// </summary>
		public AttributeInfoCache FromXmlAttribute { get; protected internal set; }

		/// <summary>
		///     Is this property a Primary key
		/// </summary>
		public bool IsPrimaryKey { get; protected internal set; }

		/// <summary>
		///     Should this property not be inserterd
		/// </summary>
		public bool InsertIgnore { get; protected internal set; }

		/// <summary>
		///     if known the ForginKey attribute
		/// </summary>
		public AttributeInfoCache ForginKeyAttribute { get; protected internal set; }

		/// <summary>
		///     Returns the For Model name if known or the Propertyname
		/// </summary>
		public string DbName
		{
			get
			{
				if (ForModel != null)
					return (ForModel.Attribute as ForModel).AlternatingName;
				return PropertyName;
			}
		}

		public int CompareTo(PropertyInfoCache other)
		{
			return GetHashCode() - other.GetHashCode();
		}

		/// <summary>
		///     For internal Usage only
		/// </summary>
		public void Refresh()
		{
			IsPrimaryKey = AttributeInfoCaches.Any(f => f.Attribute is PrimaryKeyAttribute);
			InsertIgnore = AttributeInfoCaches.Any(f => f.Attribute is InsertIgnore);
			ForginKeyAttribute = PropertyInfo.GetGetMethod().IsVirtual
				? AttributeInfoCaches.FirstOrDefault(f => f.Attribute is ForeignKeyAttribute)
				: null;
			FromXmlAttribute = AttributeInfoCaches.FirstOrDefault(f => f.Attribute is FromXmlAttribute);
			ForModel = AttributeInfoCaches.FirstOrDefault(f => f.Attribute is ForModel);
		}

		// returns property getter
		internal static Delegate GetPropGetter(Type delegateType, Type typeOfObject, string propertyName)
		{
			ParameterExpression paramExpression = Expression.Parameter(typeOfObject, "value");
			MemberExpression propertyGetterExpression = Expression.Property(paramExpression, propertyName);
			return Expression.Lambda(delegateType, propertyGetterExpression, paramExpression).Compile();
		}

		// returns property setter:
		internal static Delegate GetPropSetter(Type delegateType, Type typeOfObject, Type typeOfProperty, string propertyName)
		{
			ParameterExpression paramExpression = Expression.Parameter(typeOfObject);
			ParameterExpression paramExpression2 = Expression.Parameter(typeOfProperty, propertyName);
			MemberExpression propertyGetterExpression = Expression.Property(paramExpression, propertyName);
			return
				Expression.Lambda(delegateType, Expression.Assign(propertyGetterExpression, paramExpression2), paramExpression,
					paramExpression2)
					.Compile();
		}

		public override int GetHashCode()
		{
			return PropertyName.GetHashCode();
		}

		//internal static PropertyInfoCache Logical(string info)
		//{
		//    return new PropertyInfoCache(null)
		//    {
		//        PropertyName = info
		//    };
		//}
	}

	internal class PropertyInfoCache<T, TE> : PropertyInfoCache
	{
		internal PropertyInfoCache(string name, Action<T, TE> setter = null, Func<T, TE> getter = null,
			params AttributeInfoCache[] attributes)
		{
			if (attributes == null)
				throw new ArgumentNullException("attributes");

			PropertyName = name;

			if (setter != null)
			{
				Setter = new MethodInfoCache(setter);
			}

			if (getter != null)
			{
				Getter = new MethodInfoCache(getter);
			}
			Refresh();
		}
	}
}