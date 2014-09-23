using System.Collections;
using System.Collections.Generic;
using testing.Annotations;

namespace JPB.DataAccess
{
    [UsedImplicitly]
    internal class StaticHelper
    {
        [UsedImplicitly]
        internal static IEnumerable<T> CastToEnumerable<T>(object o) where T : class
        {
            var foo = (o as IEnumerable<T>);
            if (foo != null)
                return foo;

            var basicEnumerable = o as IEnumerable;
            var castedEmumerable = new List<T>();

            if (basicEnumerable != null)
            {
                foreach (object VARIABLE in basicEnumerable)
                    castedEmumerable.Add((T)VARIABLE);
            }

            return castedEmumerable.AsReadOnly();
        }
    }
}