﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
    //        DbQuery = DbTypes.Unknown;
    //    }
    //    // This is a positional argument

    //    public DbTypes DbQuery { get; set; }
    //}

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
    ///     Marks a mehtod as an Factory method
    ///     The method must be Public | Static and return a <code>string</code> or <code>IQueryFactoryResult</code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SelectFactoryMehtodAttribute : /*FactoryBaseAttribute*/ DataAccessAttribute
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

    #endregion

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class InsertIgnore : DataAccessAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ForeignKeyAttribute : InsertIgnore
    {
        public string KeyName { get; set; }
    }

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

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class ValueConverterAttribute : Attribute
    {
        private readonly Type _converter;

        // This is a positional argument
        public ValueConverterAttribute(Type converter)
        {
            this._converter = converter;

            if (!converter.IsAssignableFrom(typeof(IValueConverter)))
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
            return (IValueConverter) Activator.CreateInstance(_converter);
        }
    }

    /// <summary>
    /// Converts values from DB to C# and back
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// Converts a value from a DB to a C# object 
        /// </summary>
        /// <param name="value">Object from the DB</param>
        /// <param name="targetType">Type of Property to convert to</param>
        /// <param name="parameter">given Params</param>
        /// <param name="culture">Current Culture</param>
        /// <returns>C# object that is of type of property</returns>
        object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
}