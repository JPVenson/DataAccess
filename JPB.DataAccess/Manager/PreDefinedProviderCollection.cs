#region

using System.Collections;
using System.Collections.Generic;

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
			{DbAccessType.MsSql, "JPB.DataAccess.AdoWrapper.MsSqlProvider.MsSql"},
			{DbAccessType.OleDb, "JPB.DataAccess.AdoWrapper.OdbcProvider.OleDb"},
			{DbAccessType.Obdc, "JPB.DataAccess.AdoWrapper.OleDBProvider.Obdc"},
			{DbAccessType.SqLite, "JPB.DataAccess.SqLite.SqLite"},
			{DbAccessType.MySql, "JPB.DataAccess.MySql.MySql"}
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