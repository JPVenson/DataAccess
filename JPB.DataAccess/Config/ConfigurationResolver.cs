using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using JPB.DataAccess.Config.Model;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Config
{
    public class ConfigurationResolver<T>
    {
        private Config config;

        internal ConfigurationResolver(Config config)
        {
            this.config = config;
        }

        public void SetPropertyAttribute<TProp>(Expression<Func<T, TProp>> exp, DataAccessAttribute attribute)
        {
            var classInfo = config.GetOrCreateClassInfoCache(typeof(T));
            var info = ConfigHelper.GetPropertyInfoFromLabda(exp);
            var fod = classInfo.PropertyInfoCaches.First(s => s.PropertyName == info);
            fod.AttributeInfoCaches.Add(new AttributeInfoCache(attribute));
        }

        public void SetMethodAttribute<TProp>(Expression<Func<T, TProp>> exp, DataAccessAttribute attribute)
        {
            var classInfo = config.GetOrCreateClassInfoCache(typeof(T));
            var info = ConfigHelper.GetMehtodInfoFromLabda(exp);
            var fod = classInfo.MethodInfoCaches.First(s => s.MethodName == info);
            fod.AttributeInfoCaches.Add(new AttributeInfoCache(attribute));
        }

        public void SetClassAttribute(DataAccessAttribute attribute)
        {
            var classInfo = config.GetOrCreateClassInfoCache(typeof(T));
            classInfo.AttributeInfoCaches.Add(new AttributeInfoCache(attribute));
        }

        /// <summary>
        /// Set a Mehtod for creating an instance. When FullLoad is true the Framework assumes that the Factory has loaded all infos from the IDataRecord into the new Object
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="fullLoad"></param>
        public void SetFactory(Func<IDataRecord, object> factory, bool fullLoad)
        {
            var classInfo = config.GetOrCreateClassInfoCache(typeof(T));
            classInfo.Factory = factory;
            classInfo.FullFactory = fullLoad;
        }

        /// <summary>
        /// Set the Primary key 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetPrimaryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetPropertyAttribute(exp, new PrimaryKeyAttribute());
        }

        /// <summary>
        /// Set a Forgin key on a Property
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetForginKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetPropertyAttribute(exp, new ForeignKeyAttribute());
        }

        /// <summary>
        /// Set a ForModel key on a Property
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetForModelKey<TProp>(Expression<Func<T, TProp>> exp, string value)
        {
            SetPropertyAttribute(exp, new ForModel(value));
        }

        /// <summary>
        /// Set a delete factory mehtod
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetDeleteFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetMethodAttribute(exp, new DeleteFactoryMethodAttribute());
        }

        /// <summary>
        /// Set a Object factory mehtod
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetObjectFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetMethodAttribute(exp, new ObjectFactoryMethodAttribute());
        }

        /// <summary>
        /// Set a Select method
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetSelectFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetMethodAttribute(exp, new SelectFactoryMethodAttribute());
        }

        /// <summary>
        /// Set a Insert mehtod
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetInsertFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetMethodAttribute(exp, new InsertFactoryMethodAttribute());
        }

        /// <summary>
        /// Set a Update mehtod
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetUpdateFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetMethodAttribute(exp, new UpdateFactoryMethodAttribute());
        }


        /// <summary>
        /// Set the Table Name ForModel key
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetTableNameKey<TProp>(Expression<Func<T, TProp>> exp, string name)
        {
            SetClassAttribute(new ForModel(name));
        }


        ///// <summary>
        ///// Set the Primary key 
        ///// </summary>
        ///// <param name="config"></param>
        ///// <param name="PropertyName"></param>
        //public void SetStaticQueryKey(string query)
        //{

        //}
    }
}
