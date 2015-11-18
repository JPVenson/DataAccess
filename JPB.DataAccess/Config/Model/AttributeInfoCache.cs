using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.Configuration.Model
{
    internal class AttributeInfoCache
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
