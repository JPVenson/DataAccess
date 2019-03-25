using System.Collections.Generic;

namespace JPB.DataAccess.Query.QueryItems
{
	/// <summary>
	///		Internal Use Only
	/// </summary>
	public interface ISelectableQueryPart : IIdentifiableQueryPart
	{
		/// <summary>
		/// 
		/// </summary>
		bool Distinct { get; set; }
		/// <summary>
		/// 
		/// </summary>
		int? Limit { get; set; }
		/// <summary>
		/// 
		/// </summary>
		IEnumerable<ColumnInfo> Columns { get; }
	}
}