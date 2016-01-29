using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Base type for all maker Attributes
	/// </summary>
	public class DataAccessAttribute : Attribute
	{
	}

	#region FactoryAttributes

	//Work in Progress

	//[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	//public class FactoryBaseAttribute : Attribute
	//{
	//    public FactoryBaseAttribute()
	//    {
	//        DbQuery = DbAccessType.Unknown;
	//    }
	//    // This is a positional argument

	//    public DbAccessType DbQuery { get; set; }
	//}

	/// <summary>
	///     Marks this class to be allowed by the Framework for the CodeDOM Ado.net ctor creation
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class AutoGenerateCtorAttribute : DataAccessAttribute
	{
	}

	/// <summary>
	///     When a methode is marked with this attribute it can be used to configurate the current class. Must be public static
	///     void
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class ConfigMehtodAttribute : DataAccessAttribute
	{
	}

	/// <summary>
	///     Marks a Method as an Factory mehtod
	///     The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class DeleteFactoryMethodAttribute : /*FactoryBaseAttribute*/ DataAccessAttribute
	{
	}

	/// <summary>
	///     Marks a ctor or a Method as an Factory method
	///     The ctor must have only one param that is of type IDataRecord
	///     The Method must have only one param that is of type IDataRecord and returns a new Instance
	///     The Method must be static
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
	public sealed class ObjectFactoryMethodAttribute : /*FactoryBaseAttribute*/ DataAccessAttribute
	{
	}

	/// <summary>
	///     Marks a mehtod as an Factory method
	///     The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class SelectFactoryMethodAttribute : /*FactoryBaseAttribute*/ DataAccessAttribute
	{
	}

	/// <summary>
	///     Marks a mehtod as an Factory method
	///     The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class InsertFactoryMethodAttribute : /*FactoryBaseAttribute*/ DataAccessAttribute
	{
	}

	/// <summary>
	///     Marks a mehtod as an Factory method
	///     The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class UpdateFactoryMethodAttribute : /*FactoryBaseAttribute*/ DataAccessAttribute
	{
	}

	/// <summary>
	///     Provieds a Query ( parametes not used ) for selection
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class SelectFactoryAttribute : /*FactoryBaseAttribute*/ DataAccessAttribute, IQueryFactoryResult
	{
		/// <summary>
		///     Ctor
		/// </summary>
		/// <param name="query"></param>
		public SelectFactoryAttribute(string query)
		{
			Query = query;
			Parameters = null;
		}

		/// <summary>
		///     The Select Query that are used for selection of this Class
		/// </summary>
		public string Query { get; private set; }

		/// <summary>
		///     Not in USE
		/// </summary>
		public IEnumerable<IQueryParameter> Parameters { get; private set; }
	}

	/// <summary>
	///     Provieds a Query ( parametes not used ) for selection
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class StoredProcedureFactoryAttribute : /*FactoryBaseAttribute*/ DataAccessAttribute, IQueryFactoryResult
	{
		/// <summary>
		///     ctor
		/// </summary>
		/// <param name="query"></param>
		public StoredProcedureFactoryAttribute(string query)
		{
			Query = query;
			Parameters = null;
		}

		/// <summary>
		///     The Select Query that are used for selection of this Class
		/// </summary>
		public string Query { get; private set; }

		/// <summary>
		///     Not in USE
		/// </summary>
		public IEnumerable<IQueryParameter> Parameters { get; private set; }
	}

	#endregion

	/// <summary>
	///     Ignores this Property when creating an Update or Insert statement
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class InsertIgnoreAttribute : DataAccessAttribute
	{
	}

	/// <summary>
	///     Indicates this Property to be resolved as a ForeignKey
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class ForeignKeyAttribute : InsertIgnoreAttribute
	{
		/// <summary>
		///     The name of the Column that should be used
		/// </summary>
		public string KeyName { get; set; }
	}

	/// <summary>
	///     Indicates that this property is a Primary key
	///     Requert for Selection over PK
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class PrimaryKeyAttribute : DataAccessAttribute
	{
	}

	/// <summary>
	///     Allows renaming of the local class name to any name and the mapping from that name to the Db Table name
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class ForModelAttribute : DataAccessAttribute
	{
		/// <summary>
		///     Creates a new Instance of ForModelAttribute
		/// </summary>
		/// <param name="alternatingName" />
		public ForModelAttribute(string alternatingName)
		{
			AlternatingName = alternatingName;
		}

		/// <summary>
		/// </summary>
		public string AlternatingName { get; private set; }
	}

	/// <summary>
	///     A rowstate that is used to Detect a newer version
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class RowVersionAttribute : InsertIgnoreAttribute
	{
	}

	/// <summary>
	///     Marks a property to be ignored by the complete searching logic
	///     TO BE SUPPORTED
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class IgnoreReflectionAttribute : DataAccessAttribute
	{
	}

	/// <summary>
	///     Marks the property that will be used to hold all non existing Columns
	///     Must be of Type <code>IDictionary string Object</code>
	///     Only for Automatik Loading
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class LoadNotImplimentedDynamicAttribute : IgnoreReflectionAttribute
	{
	}

	/// <summary>
	///     Adds a Converter that is used to convert from an DB object to an C# object
	///     The Converter must inhert from
	///     ModelAnotations.IValueConverter
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
	public sealed class ValueConverterAttribute : DataAccessAttribute
	{
		private static readonly Dictionary<object, IValueConverter> ConverterInstance;
		internal readonly Type Converter;

		static ValueConverterAttribute()
		{
			ConverterInstance = new Dictionary<object, IValueConverter>();
		}

		internal ValueConverterAttribute(IValueConverter runtimeSupport, object para)
		{
			ConverterInstance.Add(para, runtimeSupport);
		}


		/// <summary>
		/// </summary>
		/// <param name="converter" />
		public ValueConverterAttribute(Type converter)
		{
			Converter = converter;

			if (!typeof (IValueConverter).IsAssignableFrom(converter))
			{
				throw new ArgumentException("converter must be Inhert from IValueConverter", "converter");
			}

			Parameter = string.Empty;
		}

		/// <summary>
		/// </summary>
		/// <param name="converter" />
		/// <param name="parameter" />
		public ValueConverterAttribute(Type converter, object parameter)
			: this(converter)
		{
			Parameter = parameter;
		}

		/// <summary>
		///     A static object that will be given to the Paramether
		/// </summary>
		public object Parameter { get; private set; }

		internal IValueConverter CreateConverter()
		{
			var fod = ConverterInstance.FirstOrDefault(s => s.Key.Equals(Parameter));
			if (fod.Equals(default(KeyValuePair<object, IValueConverter>)))
			{
				var instance = (IValueConverter) Activator.CreateInstance(Converter);
				ConverterInstance.Add(Parameter, instance);
				return instance;
			}

			return fod.Value;
		}
	}

	/// <summary>
	///     Marks a Property as XML Serilized
	///     If marked the output field from the query will be Serlized to the given object
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class FromXmlAttribute : ForModelAttribute
	{
		private static ILoadFromXmlStrategy _loadFromXmlStrategyInstance;
		private Type _loadFromXmlStrategy;

		/// <summary>
		/// </summary>
		public FromXmlAttribute(string fieldName)
			: base(fieldName)
		{
			FieldName = fieldName;
		}

		/// <summary>
		///     The name of the Field inside the result stream
		/// </summary>
		public string FieldName { get; set; }

		/// <summary>
		///     Specifiys the Strategy that is used to load the Property
		/// </summary>
		[DefaultValue("IncludeInSelect")]
		public LoadStrategy LoadStrategy { get; set; }

		/// <summary>
		///     if set the type will be used to define a user logic for the Serialization process
		/// </summary>
		public Type LoadFromXmlStrategy
		{
			get { return _loadFromXmlStrategy; }
			set
			{
				if (!typeof (ILoadFromXmlStrategy).IsAssignableFrom(value))
					throw new ArgumentException("Not able to assgin value from IloadFromXMLStrategy");
				_loadFromXmlStrategy = value;
			}
		}

		internal ILoadFromXmlStrategy CreateLoader()
		{
			return _loadFromXmlStrategyInstance ??
			       (_loadFromXmlStrategyInstance = (ILoadFromXmlStrategy) Activator.CreateInstance(_loadFromXmlStrategy));
		}
	}

	/// <summary>
	///     Marks a class as a StoredPrecedure wrapper
	///     if the marked class contains a Generic Arguement
	///     The result stream from the Select Statement will be parsed into the generic arguement
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class StoredProcedureAttribute : InsertIgnoreAttribute
	{
	}
}