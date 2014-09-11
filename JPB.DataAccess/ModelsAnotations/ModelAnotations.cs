using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using JPB.DataAccess.Helper;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.ModelsAnotations
{
    public class DataAccessAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DeleteFactoryMethodAttribute : DataAccessAttribute
    {
         
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class InsertIgnore : DataAccessAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ForeignKeyAttribute : InsertIgnore
    {
        public ForeignKeyAttribute(string keyname)
        {
            KeyName = keyname;
        }

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
    /// A rowstate that is used to Detect a newer version
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class RowVersionAttribute : InsertIgnore
    {

    }

    /// <summary>
    /// Marks a ctor or a Method as an Factory method
    /// The ctor must have only one param that is of type IDataRecord
    /// The Method must have only one param that is of type IDataRecord and returns a new Instance
    /// The Method must be static
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    public sealed class ObjectFactoryMethodAttribute : DataAccessAttribute
    {

    }

    /// <summary>
    /// Marks a mehtod as an Factory method
    /// The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SelectFactoryMethodAttribute : DataAccessAttribute
    {

    }

    /// <summary>
    /// Marks a mehtod as an Factory method
    /// The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class InsertFactoryMethodAttribute : DataAccessAttribute
    {

    }

    /// <summary>
    /// Marks a mehtod as an Factory method
    /// The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class UpdateFactoryMethodAttribute : DataAccessAttribute
    {

    }

    /// <summary>
    /// Marks a mehtod as an Factory method
    /// The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ObjectFactoryMehtodAttribute : DataAccessAttribute
    {

    }

    /// <summary>
    /// Marks a mehtod as an Factory method
    /// The method must return a <code>string</code> or <code>IQueryFactoryResult</code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SelectFactoryMehtodAttribute : DataAccessAttribute
    {

    }

    /// <summary>
    /// Marks a property to be ignored by the complete searching logic
    /// TO BE SUPPORTED
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class IgnoreReflectionAttribute : DataAccessAttribute
    {

    }

    /// <summary>
    /// Marks the property that will be used to hold all non existing Columns
    /// Must be of Type <code>IDictionary string Object</code>
    /// Only for Automatik Loading
    /// TO BE SUPPORTED
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class LoadNotImplimentedDynamicAttribute : IgnoreReflectionAttribute
    {

    }

    /// <summary>
    /// Provieds a Query ( parametes not used ) for selection
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SelectFactoryAttribute : DataAccessAttribute, IQueryFactoryResult
    {
        public SelectFactoryAttribute(string query)
        {
            Query = query;
        }

        public string Query { get; private set; }

        public IEnumerable<IQueryParameter> Parameters { get; private set; }
    }
}