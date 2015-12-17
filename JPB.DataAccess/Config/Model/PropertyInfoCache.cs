using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JPB.DataAccess.Config.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class PropertyInfoCache
    {
        public PropertyInfoCache(PropertyInfo propertyInfo)
        {
            this.AttributeInfoCaches = new List<AttributeInfoCache>();
            if (propertyInfo != null)
            {
                PropertyInfo = propertyInfo;
                PropertyName = propertyInfo.Name;
                Getter = MethodInfoCache.ExtractDelegate(PropertyInfo.GetGetMethod());
                Setter = MethodInfoCache.ExtractDelegate(PropertyInfo.GetSetMethod());

                this.AttributeInfoCaches = propertyInfo
                    .GetCustomAttributes(true)
                    .Where(s => s is Attribute)
                    .Select(s => new AttributeInfoCache(s as Attribute))
                    .ToList();
            }
        }

        public PropertyInfoCache(string name, 
            Action<object> setter, 
            Func<object> getter, 
            params AttributeInfoCache[] attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException("attributes");

            this.AttributeInfoCaches = attributes.ToList();
            PropertyName = name;

            if (setter != null)
            {
                Setter = setter;
            }

            if(getter != null)
            {
                Getter = getter;
            }           
        }

        public Delegate Setter { get; private set; }
        public Delegate Getter { get; private set; }

        public PropertyInfo PropertyInfo { get; private set; }
        public string PropertyName { get; private set; }
        public List<AttributeInfoCache> AttributeInfoCaches { get; private set; }
        
        public AttributeInfoCache ForModel { get; private set; }

        //internal static PropertyInfoCache Logical(string info)
        //{
        //    return new PropertyInfoCache(null)
        //    {
        //        PropertyName = info
        //    };
        //}
    }
}
