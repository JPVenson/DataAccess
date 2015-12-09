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
                this.AttributeInfoCaches = propertyInfo.GetCustomAttributes(true).Where(s => s is Attribute).Select(s => new AttributeInfoCache(s as Attribute)).ToList();
            }
        }

        public PropertyInfo PropertyInfo { get; private set; }
        public string PropertyName { get; private set; }
        public List<AttributeInfoCache> AttributeInfoCaches { get; private set; }
        
        public AttributeInfoCache ForModel { get; private set; }

        internal static PropertyInfoCache Logical(string info)
        {
            return new PropertyInfoCache(null)
            {
                PropertyName = info
            };
        }
    }
}
