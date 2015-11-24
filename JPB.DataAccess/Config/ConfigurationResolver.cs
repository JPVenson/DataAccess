using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Configuration;
using System.Linq.Expressions;
using JPB.DataAccess.Configuration.Model;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Config
{
    public class ConfigurationResolver<T>
    {
        private Configuration.Config config;

        internal ConfigurationResolver(Configuration.Config config)
        {
            this.config = config;
        }

        public void SetPropertyAttribute<TProp>(Expression<Func<T, TProp>> exp, DataAccessAttribute attribute)
        {
            var classInfo = config.GetOrCreateClassInfoCache(typeof(T));
            var info = ConfigHelper.GetPropertyInfoFromLabda(exp);
            var fod = classInfo.GetOrCreatePropertyCache(info);
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
        /// Set the Primary key 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetPrimaryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetPropertyAttribute(exp, new PrimaryKeyAttribute());
        }

        /// <summary>
        /// Set the Primary key 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetForginKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetPropertyAttribute(exp, new ForeignKeyAttribute());
        }

        /// <summary>
        /// Set the Primary key 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
public void SetForModelKey<TProp>(Expression<Func<T, TProp>> exp, string value)
{
    SetPropertyAttribute(exp, new ForModel(value));
}

        /// <summary>
        /// Set the Primary key 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetDeleteFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetMethodAttribute(exp, new DeleteFactoryMethodAttribute());
        }

        /// <summary>
        /// Set the Primary key 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetObjectFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetMethodAttribute(exp, new ObjectFactoryMethodAttribute());
        }

        /// <summary>
        /// Set the Primary key 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetSelectFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetMethodAttribute(exp, new SelectFactoryMethodAttribute());
        }

        /// <summary>
        /// Set the Primary key 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetInsertFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetMethodAttribute(exp, new InsertFactoryMethodAttribute());
        }

        /// <summary>
        /// Set the Primary key 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="PropertyName"></param>
        public void SetUpdateFactoryKey<TProp>(Expression<Func<T, TProp>> exp)
        {
            SetMethodAttribute(exp, new UpdateFactoryMethodAttribute());
        }


        /// <summary>
        /// Set the Primary key 
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
