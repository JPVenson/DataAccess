using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Config.Model
{

	class PropertyHelper : MethodInfoCache
	{
		private readonly PropertyInfo _propertyInfo;
		private dynamic _getter;
		private dynamic _setter;

		public PropertyHelper(MethodInfo mehtodInfo)
			: base(mehtodInfo)
		{
		}

		public PropertyHelper(Delegate fakeMehtod)
			: base(fakeMehtod)
		{
		}

		public PropertyHelper()
			: base((MethodInfo)null)
		{

		}

		public void SetGet(dynamic getter)
		{
			this._getter = getter;
		}

		public void SetSet(dynamic setter)
		{
			this._setter = setter;
		}

		public override object Invoke(dynamic target, params dynamic[] param)
		{
			if (_getter != null)
			{
				return _getter(target);
			}
			else
			{
				dynamic paramOne = param[0];
				_setter(target, paramOne);
				return null;
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class PropertyInfoCache : IComparable<PropertyInfoCache>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="propertyInfo"></param>
		public PropertyInfoCache(PropertyInfo propertyInfo)
		{
			this.AttributeInfoCaches = new List<AttributeInfoCache>();
			if (propertyInfo != null)
			{
				PropertyInfo = propertyInfo;
				PropertyName = propertyInfo.Name;
				PropertyType = propertyInfo.PropertyType;

				GetterDelegate = typeof(Func<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
				SetterDelegate = typeof(Action<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);

				var builder = typeof(Expression)
					.GetMethods()
					.First(s => s.Name == "Lambda" && s.ContainsGenericParameters);

				var thisRef = Expression.Parameter(propertyInfo.DeclaringType, "that");
				var accessField = Expression.MakeMemberAccess(thisRef, propertyInfo);

				var getExpression = builder
					.MakeGenericMethod(GetterDelegate)
					.Invoke(null, new object[]
					{
						accessField,
						new[]{thisRef}
					}) as dynamic;

				var valueRef = Expression.Parameter(propertyInfo.PropertyType, "newValue");
				var setter = Expression.Assign(
					accessField,
					valueRef);

				var setExpression = builder
					.MakeGenericMethod(SetterDelegate)
					.Invoke(null, new object[]
					{
						setter,
						new []{thisRef,valueRef}
					}) as dynamic;

				var getterDelegate = getExpression.Compile();
				Getter = new PropertyHelper();
				((PropertyHelper)Getter).SetGet(getterDelegate);

				var setterDelegate = setExpression.Compile();
				Setter = new PropertyHelper();
				((PropertyHelper)Setter).SetSet(setterDelegate);




				//Getter = new MethodInfoCache(PropertyInfo.GetGetMethod().CreateDelegate(GetterDelegate));
				//Setter = new MethodInfoCache(PropertyInfo.GetSetMethod().CreateDelegate(SetterDelegate));

				//Getter = new MethodInfoCache(GetPropGetter(GetterDelegate, propertyInfo.DeclaringType, PropertyName));
				//Setter = new MethodInfoCache(GetPropSetter(SetterDelegate, propertyInfo.DeclaringType, targetType, PropertyName));

				//Getter = new MethodInfoCache(PropertyInfo.GetGetMethod());
				//Setter = new MethodInfoCache(PropertyInfo.GetSetMethod());

				this.AttributeInfoCaches = propertyInfo
					.GetCustomAttributes(true)
					.Where(s => s is Attribute)
					.Select(s => new AttributeInfoCache(s as Attribute))
					.ToList();

				RenumeratePropertys();
			}
		}


		/// <summary>
		/// For internal Usage only
		/// </summary>
		public void RenumeratePropertys()
		{
			IsPrimaryKey = AttributeInfoCaches.Any(f => f.Attribute is PrimaryKeyAttribute);
			InsertIgnore = AttributeInfoCaches.Any(f => f.Attribute is InsertIgnore);
			IsNavProperty = PropertyInfo.GetGetMethod().IsVirtual ||
							AttributeInfoCaches.Any(f => f.Attribute is ForeignKeyAttribute);
			FromXmlAttribute = AttributeInfoCaches.FirstOrDefault(f => f.Attribute is FromXmlAttribute);
			ForModel = AttributeInfoCaches.FirstOrDefault(f => f.Attribute is ForModel);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="setter"></param>
		/// <param name="getter"></param>
		/// <param name="attributes"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PropertyInfoCache(string name,
			Action<object, object> setter,
			Func<object, object> getter,
			params AttributeInfoCache[] attributes)
		{
			if (attributes == null)
				throw new ArgumentNullException("attributes");

			this.AttributeInfoCaches = attributes.ToList();
			PropertyName = name;

			if (setter != null)
			{
				Setter = new MethodInfoCache(setter);
			}

			if (getter != null)
			{
				Getter = new MethodInfoCache(getter);
			}
			RenumeratePropertys();
		}


		// returns property getter
		public static Delegate GetPropGetter(Type delegateType, Type typeOfObject, string propertyName)
		{
			var paramExpression = Expression.Parameter(typeOfObject, "value");
			var propertyGetterExpression = Expression.Property(paramExpression, propertyName);
			return Expression.Lambda(delegateType, propertyGetterExpression, paramExpression).Compile();
		}

		// returns property setter:
		public static Delegate GetPropSetter(Type delegateType, Type typeOfObject, Type typeOfProperty, string propertyName)
		{
			var paramExpression = Expression.Parameter(typeOfObject);
			var paramExpression2 = Expression.Parameter(typeOfProperty, propertyName);
			var propertyGetterExpression = Expression.Property(paramExpression, propertyName);
			return Expression.Lambda(delegateType, Expression.Assign(propertyGetterExpression, paramExpression2), paramExpression, paramExpression2)
				.Compile();
		}

		public Type SetterDelegate { get; private set; }
		public Type GetterDelegate { get; private set; }

		public MethodInfoCache Setter { get; private set; }
		public MethodInfoCache Getter { get; private set; }

		public Type PropertyType { get; set; }

		public PropertyInfo PropertyInfo { get; private set; }
		public string PropertyName { get; private set; }
		public List<AttributeInfoCache> AttributeInfoCaches { get; private set; }

		public AttributeInfoCache ForModel { get; private set; }
		public AttributeInfoCache FromXmlAttribute { get; set; }

		public bool IsPrimaryKey { get; private set; }
		public bool InsertIgnore { get; private set; }
		public bool IsNavProperty { get; private set; }

		public int CompareTo(PropertyInfoCache other)
		{
			return this.GetHashCode() - other.GetHashCode();
		}

		public override int GetHashCode()
		{
			return this.PropertyName.GetHashCode();
		}

		//internal static PropertyInfoCache Logical(string info)
		//{
		//    return new PropertyInfoCache(null)
		//    {
		//        PropertyName = info
		//    };
		//}
	}
}
