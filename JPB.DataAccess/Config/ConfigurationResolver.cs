﻿using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using JPB.DataAccess.Config.Model;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Config
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ConfigurationResolver<T>
	{
		private Config _config;

		internal ConfigurationResolver()
		{
		}

		internal ConfigurationResolver(Config config, ClassInfoCache classInfoCache)
		{
			ClassInfoCache = classInfoCache;
			_config = config;
		}

		/// <summary>
		///     Easy access to the known Class Info
		/// </summary>
		public ClassInfoCache ClassInfoCache { get; private set; }

		/// <summary>
		///     Set a attribute on a property
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		public void SetPropertyAttribute<TProp>(Expression<Func<T, TProp>> exp, DataAccessAttribute attribute)
		{
			string info = ConfigHelper.GetPropertyInfoFromLabda(exp);
			PropertyInfoCache fod = ClassInfoCache.PropertyInfoCaches.First(s => s.Key == info).Value;
			fod.AttributeInfoCaches.Add(new AttributeInfoCache(attribute));
		}

		/// <summary>
		///     Set a attribute on a property
		/// </summary>
		public void SetPropertyAttribute(string info, DataAccessAttribute attribute)
		{
			PropertyInfoCache fod = ClassInfoCache.PropertyInfoCaches.First(s => s.Key == info).Value;
			fod.AttributeInfoCaches.Add(new AttributeInfoCache(attribute));
		}

		/// <summary>
		///     set a Attribute on a method
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		public void SetMethodAttribute<TProp>(Expression<Func<T, TProp>> exp, DataAccessAttribute attribute)
		{
			string info = ConfigHelper.GetMehtodInfoFromLabda(exp);
			MethodInfoCache fod = ClassInfoCache.MethodInfoCaches.First(s => s.MethodName == info);
			fod.AttributeInfoCaches.Add(new AttributeInfoCache(attribute));
		}

		/// <summary>
		///     set a Attribute on a method
		/// </summary>
		public void SetMethodAttribute(string info, DataAccessAttribute attribute)
		{
			MethodInfoCache fod = ClassInfoCache.MethodInfoCaches.First(s => s.MethodName == info);
			fod.AttributeInfoCaches.Add(new AttributeInfoCache(attribute));
		}

		/// <summary>
		///     Adds a Fake Mehtod to the class
		/// </summary>
		public void CreateMethod(string methodName, Delegate methodBody, params AttributeInfoCache[] attributes)
		{
			if (methodName == null)
				throw new ArgumentNullException("methodName");
			if (methodBody == null)
				throw new ArgumentNullException("methodBody");
			if (ClassInfoCache.MethodInfoCaches.Any(s => s.MethodName == methodName))
				throw new ArgumentOutOfRangeException("methodName", "Method name does exist. Cannot define a Method twice");
			var mehtodInfo = new MethodInfoCache(methodBody, methodName, attributes);
			ClassInfoCache.MethodInfoCaches.Add(mehtodInfo);
		}

		/// <summary>
		///     Adds a Fake property to the class getter and setter will be invoked like normal ones
		/// </summary>
		/// <typeparam name="TE"></typeparam>
		public void CreateProperty<TE>(string name, Action<T, TE> setter = null, Func<T, TE> getter = null,
			params AttributeInfoCache[] attributes)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (ClassInfoCache.PropertyInfoCaches.Any(s => s.Key == name))
				throw new ArgumentOutOfRangeException("name", "Property name does exist. Cannot define a property twice");
			if (setter == null && getter == null)
				throw new ArgumentNullException("setter",
					"Propertys must define at least one accessor. You cannot define a property without getter and setter");
			var propInfo = new PropertyInfoCache<T, TE>(name, setter, getter, attributes);
			ClassInfoCache.PropertyInfoCaches.Add(name, propInfo);
		}

		/// <summary>
		///     set a Attribute on a class
		/// </summary>
		public void SetClassAttribute(DataAccessAttribute attribute)
		{
			ClassInfoCache.AttributeInfoCaches.Add(new AttributeInfoCache(attribute));
		}

		/// <summary>
		///     Set a Mehtod for creating an instance. When FullLoad is true the Framework assumes that the Factory has loaded all
		///     infos from the IDataRecord into the new Object
		/// </summary>
		public void SetFactory(Func<IDataRecord, object> factory, bool fullLoad)
		{
			ClassInfoCache.Factory = factory;
			ClassInfoCache.FullFactory = fullLoad;
		}

		/// <summary>
		///     Set a converter type that allows you to convert incomming and outgoing data to be converted befor set to the
		///     property
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		public void SetConverter<TProp>(Expression<Func<T, TProp>> exp, Type converter)
		{
			SetPropertyAttribute(exp, new ValueConverterAttribute(converter));
		}

		//class RuntimeConverter : IValueConverter
		//{
		//	private readonly Func<object, Type, object, CultureInfo, object> _convertFrom;
		//	private readonly Func<object, Type, object, CultureInfo, object> _convertTo;
		//	public RuntimeConverter(Func<object, Type, object, CultureInfo, object> convertFrom, 
		//		Func<object, Type, object, CultureInfo, object> convertTo)
		//	{
		//		_convertFrom = convertFrom;
		//		_convertTo = convertTo;
		//	}
		//	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		//	{
		//		return _convertFrom(value, targetType, parameter, culture);
		//	}
		//	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		//	{
		//		return _convertTo(value, targetType, parameter, culture);
		//	}
		//}
		//public void SetConverter<TProp>(Expression<Func<T, TProp>> exp,
		//	Func<object, Type, object, CultureInfo, object> convertFrom,
		//	Func<object, Type, object, CultureInfo, object> convertTo, object key)
		//{
		//	SetPropertyAttribute(exp, new ValueConverterAttribute(new RuntimeConverter(convertFrom, convertTo), key));
		//}
		/// <summary>
		///     Set the Primary key
		/// </summary>
		public void SetPrimaryKey<TProp>(Expression<Func<T, TProp>> exp)
		{
			SetPropertyAttribute(exp, new PrimaryKeyAttribute());
		}

		/// <summary>
		///     Set a Forgin key on a Property
		/// </summary>
		public void SetForginKey<TProp>(Expression<Func<T, TProp>> exp)
		{
			SetPropertyAttribute(exp, new ForeignKeyAttribute());
		}

		/// <summary>
		///     Set a ForModel key on a Property
		/// </summary>
		public void SetForModelKey<TProp>(Expression<Func<T, TProp>> exp, string value)
		{
			SetPropertyAttribute(exp, new ForModel(value));
		}

		/// <summary>
		///     Set a delete factory mehtod
		/// </summary>
		public void SetDeleteFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
		{
			SetMethodAttribute(exp, new DeleteFactoryMethodAttribute());
		}

		/// <summary>
		///     Set a Object factory mehtod
		/// </summary>
		public void SetObjectFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
		{
			SetMethodAttribute(exp, new ObjectFactoryMethodAttribute());
		}

		/// <summary>
		///     Set a Select method
		/// </summary>
		public void SetSelectFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
		{
			SetMethodAttribute(exp, new SelectFactoryMethodAttribute());
		}

		/// <summary>
		///     Set a Insert mehtod
		/// </summary>
		public void SetInsertFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
		{
			SetMethodAttribute(exp, new InsertFactoryMethodAttribute());
		}

		/// <summary>
		///     Set a Update mehtod
		/// </summary>
		public void SetUpdateFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
		{
			SetMethodAttribute(exp, new UpdateFactoryMethodAttribute());
		}

		/// <summary>
		///     Set the Table Name ForModel key
		/// </summary>
		public void SetTableNameKey<TProp>(Expression<Func<T, TProp>> exp, string name)
		{
			SetClassAttribute(new ForModel(name));
		}

		///// <summary>
		///// Set the Primary key 
		///// </summary>
		//
		//
		//public void SetStaticQueryKey(string query)
		//{
		//}
	}
}