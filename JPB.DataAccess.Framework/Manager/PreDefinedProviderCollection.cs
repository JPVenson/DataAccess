#region

using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.Framework.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.Framework.Contacts;

#endregion

namespace JPB.DataAccess.Framework.Manager
{
	/// <summary>
	///     A KeyValue list of providers that can be loaded with the DbAccessLayer where Key is a
	/// <see cref="DbAccessType"/> and value the Fully Qualified type name to an
	/// instance of <seealso cref="IDatabaseStrategy"/>
	/// </summary>
	public class PreDefinedProviderCollection : IReadOnlyCollection<KeyValuePair<DbAccessType, string>>
	{
		private readonly Dictionary<DbAccessType, string> _preDefinedProvider = new Dictionary<DbAccessType, string>
		{
			{DbAccessType.MsSql, typeof(MsSql).AssemblyQualifiedName},
			{DbAccessType.SqLite, "JPB.DataAccess.SqLite.SqLite, JPB.DataAccess.SqLite"},
			{DbAccessType.MySql, "JPB.DataAccess.MySql.MySql, JPB.DataAccess.MySql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"}
		};

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     An enumerator that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyValuePair<DbAccessType, string>> GetEnumerator()
		{
			return _preDefinedProvider.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		///     Gets the number of elements in the collection.
		/// </summary>
		public int Count
		{
			get { return _preDefinedProvider.Count; }
		}
	}
}