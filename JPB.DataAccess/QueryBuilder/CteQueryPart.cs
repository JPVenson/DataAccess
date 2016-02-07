/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System.Collections.Generic;
using JPB.DataAccess.Helper;

namespace JPB.DataAccess.QueryBuilder
{
	/// <summary>
	///     Maker for CTS in MSSQL
	/// </summary>
	public class CteQueryPart : GenericQueryPart
	{
		/// <summary>
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="parameters"></param>
		public CteQueryPart(string prefix, IEnumerable<IQueryParameter> parameters)
			: base(prefix, parameters)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="prefix"></param>
		public CteQueryPart(string prefix)
			: base(prefix)
		{
		}
	}
}