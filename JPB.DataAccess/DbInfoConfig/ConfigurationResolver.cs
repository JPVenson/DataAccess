/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.MetaApi.Model;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.DbInfoConfig
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ConfigurationResolver<T>
	{
		private DbConfig _configBase;

		internal ConfigurationResolver(DbConfig configBase, DbClassInfoCache classInfoCache)
		{
			ClassInfoCache = classInfoCache;
			_configBase = configBase;
		}

		/// <summary>
		///     Easy access to the known Class Info
		/// </summary>
		public DbClassInfoCache ClassInfoCache { get; private set; }

		/// <summary>
		///     Set a attribute on a property
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		public void SetPropertyAttribute<TProp>(Expression<Func<T, TProp>> exp, DataAccessAttribute attribute)
		{
			var info = MetaInfoStoreExtentions.GetPropertyInfoFromLabda(exp);
			var fod = ClassInfoCache.Propertys.First(s => s.Key == info).Value;
			fod.Attributes.Add(new DbAttributeInfoCache(attribute));
		}

		/// <summary>
		///     Set a attribute on a property
		/// </summary>
		public void SetPropertyAttribute(string info, DataAccessAttribute attribute)
		{
			var fod = ClassInfoCache.Propertys.First(s => s.Key == info).Value;
			fod.Attributes.Add(new DbAttributeInfoCache(attribute));
		}

		/// <summary>
		///     set a Attribute on a method
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		public void SetMethodAttribute<TProp>(Expression<Func<T, TProp>> exp, DataAccessAttribute attribute)
		{
			var info = MetaInfoStoreExtentions.GetMehtodInfoFromLabda(exp);
			var fod = ClassInfoCache.Mehtods.First(s => s.MethodName == info);
			fod.Attributes.Add(new DbAttributeInfoCache(attribute));
		}

		/// <summary>
		///     set a Attribute on a method
		/// </summary>
		public void SetMethodAttribute(string info, DataAccessAttribute attribute)
		{
			var fod = ClassInfoCache.Mehtods.First(s => s.MethodName == info);
			fod.Attributes.Add(new DbAttributeInfoCache(attribute));
		}

		/// <summary>
		///     Adds a Fake Mehtod to the class
		/// </summary>
		public void CreateMethod(string methodName, Func<object, object[], object> methodBody, params DbAttributeInfoCache[] attributes)
		{
			if (methodName == null)
				throw new ArgumentNullException("methodName");
			if (methodBody == null)
				throw new ArgumentNullException("methodBody");
			if (ClassInfoCache.Mehtods.Any(s => s.MethodName == methodName))
				throw new ArgumentOutOfRangeException("methodName", "Method name does exist. Cannot define a Method twice");
			var mehtodInfo = new DbMethodInfoCache(methodBody, methodName, attributes);
			ClassInfoCache.Mehtods.Add(mehtodInfo);
		}


		/// <summary>
		///     Adds a Fake Mehtod to the class
		/// </summary>
		public void CreateMethod<Source, Input>(string methodName, Action<Source, Input> methodBody, params DbAttributeInfoCache[] attributes)
		{
			if (methodName == null)
				throw new ArgumentNullException("methodName");
			if (methodBody == null)
				throw new ArgumentNullException("methodBody");
			if (ClassInfoCache.Mehtods.Any(s => s.MethodName == methodName))
				throw new ArgumentOutOfRangeException("methodName", "Method name does exist. Cannot define a Method twice");
			var mehtodInfo = new DbMethodInfoCache((o, objects) =>
			{
				methodBody((Source)o, (Input)objects[0]);
				return null;
			}, methodName, attributes);
			ClassInfoCache.Mehtods.Add(mehtodInfo);
		}

		/// <summary>
		///     Adds a Fake Mehtod to the class
		/// </summary>
		public void CreateMethod<Source, Output>(string methodName, Func<Source, Output> methodBody, params DbAttributeInfoCache[] attributes)
		{
			if (methodName == null)
				throw new ArgumentNullException("methodName");
			if (methodBody == null)
				throw new ArgumentNullException("methodBody");
			if (ClassInfoCache.Mehtods.Any(s => s.MethodName == methodName))
				throw new ArgumentOutOfRangeException("methodName", "Method name does exist. Cannot define a Method twice");
			var mehtodInfo = new DbMethodInfoCache((o, objects) =>
			{
				return methodBody((Source)o);
			}, methodName, attributes);
			ClassInfoCache.Mehtods.Add(mehtodInfo);
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
			if (ClassInfoCache.Propertys.Any(s => s.Key == name))
				throw new ArgumentOutOfRangeException("name", "Property name does exist. Cannot define a property twice");
			if (setter == null && getter == null)
				throw new ArgumentNullException("setter",
					"Propertys must define at least one accessor. You cannot define a property without getter and setter");
			var propInfo = new DbPropertyInfoCache<T, TE>(name, setter, getter, attributes);
			ClassInfoCache.Propertys.Add(name, propInfo);
		}

		/// <summary>
		///     Adds a Fake property to the class getter and setter will be invoked like normal ones
		/// </summary>
		/// <typeparam name="TE"></typeparam>
		public void CreateProperty<TE>(string name, params AttributeInfoCache[] attributes)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (ClassInfoCache.Propertys.Any(s => s.Key == name))
				throw new ArgumentOutOfRangeException("name", "Property name does exist. Cannot define a property twice");
			var propInfo = new DbAutoPropertyInfoCache<TE>(name, attributes);
			ClassInfoCache.Propertys.Add(name, propInfo);
		}

		/// <summary>
		///     set a Attribute on a class
		/// </summary>
		public void SetClassAttribute(DataAccessAttribute attribute)
		{
			ClassInfoCache.Attributes.Add(new DbAttributeInfoCache(attribute));
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
		///     Set a ForModelAttribute key on a Property
		/// </summary>
		public void SetForModelKey<TProp>(Expression<Func<T, TProp>> exp, string value)
		{
			SetPropertyAttribute(exp, new ForModelAttribute(value));
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
		///     Set the Table Name ForModelAttribute key
		/// </summary>
		public void SetTableNameKey<TProp>(Expression<Func<T, TProp>> exp, string name)
		{
			SetClassAttribute(new ForModelAttribute(name));
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