#region

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
			var props = configuration.GetOrCreateClassInfoCache(type)
				.Propertys
				.ToArray();

			MetaHeader = new MultiValueDictionary<string, object>();
			for (var index = 0; index < props.Length; index++)
			{
				var name = props[index];
				MetaHeader.Add(name.Value.DbName, name.Value.Getter.Invoke(sourceObject));
			}
		}
	}
}