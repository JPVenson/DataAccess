#region

using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.AdoWrapper.MsSqlProvider;
using JPB.DataAccess.AdoWrapper.OdbcProvider;
using JPB.DataAccess.AdoWrapper.OleDBProvider;

#endregion

namespace JPB.DataAccess.Manager
{
	/// <summary>
	///     A list that contains all kown Provider and there Basic implimentation
	/// </summary>
	public class PreDefinedProviderCollection : IReadOnlyCollection<KeyValuePair<DbAccessType, string>>
	{
		private readonly Dictionary<DbAccessType, string> _preDefinedProvider = new Dictionary<DbAccessType, string>
		{
			{DbAccessType.MsSql, typeof(MsSql).AssemblyQualifiedName},
			{DbAccessType.OleDb, typeof(OleDb).AssemblyQualifiedName},
			{DbAccessType.Obdc, typeof(Obdc).AssemblyQualifiedName},
			{DbAccessType.SqLite, "JPB.DataAccess.SqLite.SqLite, JPB.DataAccess.SqLite"},
			{DbAccessType.MySql, "JPB.DataAccess.MySql.MySql, JPB.DataAccess.MySql"}
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