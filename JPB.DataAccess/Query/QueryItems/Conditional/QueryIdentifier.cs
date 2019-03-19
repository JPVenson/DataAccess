using JPB.DataAccess.DbInfoConfig.DbInfo;

namespace JPB.DataAccess.Query.QueryItems.Conditional
{
	/// <summary>
	///		Identifies an SQL target
	/// </summary>
	public class QueryIdentifier
	{
		/// <summary>
		/// 
		/// </summary>
		public QueryIdentifier()
		{
			
		}

		/// <summary>
		///		Creates an known alias for a class
		/// </summary>
		/// <param name="getClassInfo"></param>
		/// <param name="table"></param>
		public QueryIdentifier(DbClassInfoCache getClassInfo, QueryIdTypes table)
		{
			
		}

		/// <summary>
		///		Returns a valid alias
		/// </summary>
		/// <returns></returns>
		public string GetAlias()
		{
			return Value.Trim('[', ']');
		}

		/// <summary>
		///		The Generated Alias for SQL
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public QueryIdTypes QueryIdType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public enum QueryIdTypes
		{
			/// <summary>
			///		The Unkown Target Id Type
			/// </summary>
			Unkown,
			/// <summary>
			///		The QueryId references a Table
			/// </summary>
			Table,
			/// <summary>
			///		The QueryId references a Cte
			/// </summary>
			Cte,
			/// <summary>
			///		The QueryId references a SubQuery
			/// </summary>
			SubQuery
		}
	}
}