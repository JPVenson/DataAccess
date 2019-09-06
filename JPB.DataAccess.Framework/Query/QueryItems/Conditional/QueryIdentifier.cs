namespace JPB.DataAccess.Query.QueryItems.Conditional
{
	/// <summary>
	///     Identifies an SQL target
	/// </summary>
	public class QueryIdentifier
	{
		/// <summary>
		/// </summary>
		public enum QueryIdTypes
		{
			/// <summary>
			///     The Unkown Target Id Type
			/// </summary>
			Unkown,

			/// <summary>
			///     The QueryId references a Table
			/// </summary>
			Table,

			/// <summary>
			///     The QueryId references a Cte
			/// </summary>
			Cte,

			/// <summary>
			///     The QueryId references a SubQuery
			/// </summary>
			SubQuery,
			/// <summary>
			///		The QueryId references a Column
			/// </summary>
			Column
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (!(obj is QueryIdentifier ident))
			{
				return false;
			}

			if (ident.Value.Equals(Value) && ident.QueryIdType.Equals(QueryIdType))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		///     The Generated Alias for SQL
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// </summary>
		public QueryIdTypes QueryIdType { get; set; }

		/// <summary>
		///     Returns a valid alias
		/// </summary>
		/// <returns></returns>
		public string GetAlias()
		{
			return Value.TrimAlias();
		}
	}
}