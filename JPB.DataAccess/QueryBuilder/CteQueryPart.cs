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