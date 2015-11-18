using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.Configuration.Model
{
    internal class PropertyInfoCache
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

        internal static PropertyInfoCache Logical(string info)
        {
            return new PropertyInfoCache(null)
            {
                PropertyName = info
            };
        }
    }
}
