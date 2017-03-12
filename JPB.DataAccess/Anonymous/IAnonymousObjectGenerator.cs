using System;
using JPB.DataAccess.DbInfoConfig.DbInfo;

namespace JPB.DataAccess.Anonymous
{
    /// <summary>
    /// Creates an Object that maps values to non Responding values
    /// </summary>
    public interface IAnonymousObjectGenerator : IEquatable<IAnonymousObjectGenerator>
    {
        /// <summary>
        /// If true the Generation of Anonymous objects is linar and will always return the same value for the same targetValue
        /// </summary>
        bool OneWayGeneration { get; }
        /// <summary>
        /// Returns the target type this Generator can handle. If it returns null this Generator can handle all kinds of objects
        /// </summary>
        Type TargetPropType { get; }
        /// <summary>
        /// Generate an Anonymous object for the <paramref name="targetValue"/>
        /// </summary>
        /// <param name="targetClass"></param>
        /// <param name="targetPropType"></param>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        object GenerateAnoymousAlias(DbClassInfoCache targetClass, DbClassInfoCache targetPropType, object targetValue);
    }
}