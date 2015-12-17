using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Config.Model
{
	/// <summary>
	/// 
	/// </summary>
	public class PropertyInfoCache
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
				var targetType = propertyInfo.PropertyType;

				GetterDelegate = typeof(Func<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
				SetterDelegate = typeof(Action<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);

				//Getter = new MethodInfoCache(PropertyInfo.GetGetMethod().CreateDelegate(GetterDelegate));
				//Setter = new MethodInfoCache(PropertyInfo.GetSetMethod().CreateDelegate(SetterDelegate));

				Getter = new MethodInfoCache(GetPropGetter(GetterDelegate, propertyInfo.DeclaringType, PropertyName));
				Setter = new MethodInfoCache(GetPropSetter(SetterDelegate, propertyInfo.DeclaringType, targetType, PropertyName));

				this.AttributeInfoCaches = propertyInfo
					.GetCustomAttributes(true)
					.Where(s => s is Attribute)
					.Select(s => new AttributeInfoCache(s as Attribute))
					.ToList();

				RenumeratePropertys();
			}
		}

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

		public PropertyInfo PropertyInfo { get; private set; }
		public string PropertyName { get; private set; }
		public List<AttributeInfoCache> AttributeInfoCaches { get; private set; }

		public AttributeInfoCache ForModel { get; private set; }
		public AttributeInfoCache FromXmlAttribute { get; set; }

		public bool IsPrimaryKey { get; private set; }
		public bool InsertIgnore { get; private set; }
		public bool IsNavProperty { get; private set; }

		//internal static PropertyInfoCache Logical(string info)
		//{
		//    return new PropertyInfoCache(null)
		//    {
		//        PropertyName = info
		//    };
		//}
	}
}
