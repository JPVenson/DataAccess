/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System.Collections;
using System.Collections.Generic;

namespace JPB.DataAccess.Manager
{
	/// <summary>
	///     A list that contains all kown Provider and there Basic implimentation
	/// </summary>
	public class PreDefinedProviderCollection : IReadOnlyCollection<KeyValuePair<DbAccessType, string>>
	{
		private readonly Dictionary<DbAccessType, string> _preDefinedProvider = new Dictionary<DbAccessType, string>
		{
			{DbAccessType.MsSql, "JPB.DataAccess.AdoWrapper.MsSql.MsSql"},
			{DbAccessType.OleDb, "JPB.DataAccess.AdoWrapper.OleDB.OleDb"},
			{DbAccessType.Obdc, "JPB.DataAccess.AdoWrapper.Obdc.Obdc"},
			{DbAccessType.SqLite, "JPB.DataAccess.SqLite.SqLite"}
		};

		public IEnumerator<KeyValuePair<DbAccessType, string>> GetEnumerator()
		{
			return _preDefinedProvider.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count
		{
			get { return _preDefinedProvider.Count; }
		}
	}
}