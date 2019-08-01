using System.Collections.Generic;

namespace JPB.DataAccess.Framework.Query.QueryItems
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

	/// <summary>
	///		Internal Use Only
	/// </summary>
	public interface ISelectQueryPart : ISelectableQueryPart
	{
		/// <summary>
		/// 
		/// </summary>
		IEnumerable<JoinParseInfo> Joins { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="join"></param>
		void AddJoin(JoinTableQueryPart join);
	}
}