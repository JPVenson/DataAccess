using System;

namespace UnitTestProject1
{
    public class ValueInformations
    {
        public string Name { get; set; }
        public int MaxContentSize { get; set; }

        public int MaxSize
        {
            get
            {
                var val = Name.Length > MaxContentSize ? Name.Length : MaxContentSize;
                return val;
            }
        }

        public Func<object, object> GetValue { get; set; }
    }
}