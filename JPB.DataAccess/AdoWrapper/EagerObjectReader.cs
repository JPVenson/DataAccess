#region

using System;
using System.Collections;
using System.Data;
using System.Linq;
using JPB.DataAccess.DbInfoConfig;

#endregion

namespace JPB.DataAccess.AdoWrapper
{
	/// <summary>
	///     Provides access to the Given object
	/// </summary>
	/// <seealso cref="EagarDataRecord" />
	/// <seealso cref="System.Data.IDataReader" />
	public sealed class EagerObjectReader : EagarDataRecord
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EagerObjectReader" /> class.
		/// </summary>
		/// <param name="sourceObject">The source object.</param>
		/// <param name="configuration">The access layer.</param>
		internal EagerObjectReader(object sourceObject, DbConfig configuration) 
		{
			var type = sourceObject.GetType();
			var props = configuration.GetOrCreateClassInfoCache(type).Propertys;
			MetaHeader = props.Select(f => f.Value.DbName).ToArray();
			Objects = new ArrayList(props.Select(f => f.Value.Getter.Invoke(sourceObject)).ToArray());
		}
	}
}