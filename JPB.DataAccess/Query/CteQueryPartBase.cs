#region

using System.Collections.Generic;
using JPB.DataAccess.Contacts;

#endregion

namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Maker for CTS in MSSQL
	/// </summary>
	public class CteQueryPartBase : QueryPartBase
	{
		/// <summary>
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="parameters"></param>
		public CteQueryPartBase(string prefix, IEnumerable<IQueryParameter> parameters)
			: base(prefix, parameters, null)
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="prefix"></param>
		public CteQueryPartBase(string prefix)
			: base(prefix)
		{
		}
	}
}