using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JPB.DataAccess.Config.Model
{
    public class MethodInfoCache
    {
        public MethodInfoCache(MethodInfo mehtodInfo)
        {
            this.AttributeInfoCaches = new List<AttributeInfoCache>();
            if (mehtodInfo != null)
            {
                MethodInfo = mehtodInfo;
                MethodName = mehtodInfo.Name;
                this.AttributeInfoCaches =
                    mehtodInfo.GetCustomAttributes(true).Where(s => s is Attribute).Select(s => new AttributeInfoCache(s as Attribute)).ToList();
            }
        }

        public MethodInfo MethodInfo { get; private set; }
        public string MethodName { get; private set; }
        public List<AttributeInfoCache> AttributeInfoCaches { get; private set; }
    }
}
