using System;
using System.Collections.Generic;
using System.ComponentModel;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.ModelsAnotations
{
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
    /// When a methode is marked with this attribute it can be used to configurate the current class. Must be public static void
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ConfigMehtodAttribute : DataAccessAttribute
    {

    }

    /// <summary>
    /// Marks a Method as an Factory mehtod
    /// The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
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
        public SelectFactoryAttribute(string query)
        {
            Query = query;
        }

        public string Query { get; private set; }

        public IEnumerable<IQueryParameter> Parameters { get; private set; }
    }

    /// <summary>
    ///     Provieds a Query ( parametes not used ) for selection
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class StoredProcedureFactoryAttribute : /*FactoryBaseAttribute*/ DataAccessAttribute, IQueryFactoryResult
    {
        public StoredProcedureFactoryAttribute(string query)
        {
            Query = query;
        }

        public string Query { get; private set; }

        public IEnumerable<IQueryParameter> Parameters { get; private set; }
    }

    #endregion

    /// <summary>
    /// Ignores this Property when creating an Update or Insert statement
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class InsertIgnore : DataAccessAttribute
    {
    }

    /// <summary>
    /// Indicates this Property to be resolved as a ForeignKey
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ForeignKeyAttribute : InsertIgnore
    {
        public string KeyName { get; set; }
    }   

    /// <summary>
    /// Indicates that this property is a Primary key
    /// Requert for Selection over PK 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class PrimaryKeyAttribute : DataAccessAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ForModel : DataAccessAttribute
    {
        public ForModel(string alternatingName)
        {
            AlternatingName = alternatingName;
        }

        public string AlternatingName { get; set; }
    }

    /// <summary>
    ///     A rowstate that is used to Detect a newer version
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class RowVersionAttribute : InsertIgnore
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
    /// Adds a Converter that is used to convert from an DB object to an C# object
    /// The Converter must inhert from
    /// ModelAnotations.IValueConverter
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class ValueConverterAttribute : Attribute
    {
        private readonly Type _converter;

        // This is a positional argument
        public ValueConverterAttribute(Type converter)
        {
            this._converter = converter;

            if (!typeof(IValueConverter).IsAssignableFrom(converter))
            {
                throw new ArgumentException("converter must be Inhert from IValueConverter", "converter");
            }
        }

        // This is a positional argument
        public ValueConverterAttribute(Type converter, object parameter)
            : this(converter)
        {
            this.Parameter = parameter;
        }

        public object Parameter { get; private set; }

        internal IValueConverter CreateConverter()
        {
            return (IValueConverter)Activator.CreateInstance(_converter);
        }
    }

    /// <summary>
    /// Marks a Property as XML Serilized
    /// If marked the output field from the query will be Serlized to the given object
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class FromXmlAttribute : ForModel
    {
        private Type _loadFromXmlStrategy;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        public FromXmlAttribute(string fieldName)
            : base(fieldName)
        {
            FieldName = fieldName;
        }

        /// <summary>
        /// The name of the Field inside the result stream
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Specifiys the Strategy that is used to load the Property
        /// </summary>
        [DefaultValue("IncludeInSelect")]
        public LoadStrategy LoadStrategy { get; set; }

        /// <summary>
        /// if set the type will be used to define a user logic for the Serialization process
        /// </summary>
        public Type LoadFromXmlStrategy
        {
            get { return _loadFromXmlStrategy; }
            set
            {
                if (!typeof(ILoadFromXmlStrategy).IsAssignableFrom(value))
                    throw new ArgumentException("Not able to assgin value from IloadFromXMLStrategy");
                _loadFromXmlStrategy = value;
            }
        }

        internal ILoadFromXmlStrategy CreateLoader()
        {
            return (ILoadFromXmlStrategy)Activator.CreateInstance(_loadFromXmlStrategy);
        }
    }

    /// <summary>
    /// Marks a class as a StoredPrecedure wrapper
    /// if the marked class contains a Generic Arguement
    ///     The result stream from the Select Statement will be parsed into the generic arguement
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class StoredProcedureAttribute : InsertIgnore
    {

    }
}