using System;

namespace JPB.DataAccess.Config.Model
{
    public class AttributeInfoCache
    {
        public Attribute Attribute { get; private set; }
        public object AttributeName { get; private set; }

        public AttributeInfoCache(Attribute attribute)
        {
            Attribute = attribute;
            AttributeName = attribute.TypeId;
        }
    }
}
