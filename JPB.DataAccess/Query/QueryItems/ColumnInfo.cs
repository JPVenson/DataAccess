using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	/// <summary>
	///		Internal Use ONLY
	/// </summary>
	public class ColumnInfo
	{
		private readonly ColumnInfo _aliasOf;

		internal ColumnInfo(string columnName, ColumnInfo aliasOf,
			QueryIdentifier alias)
		{
			_aliasOf = aliasOf;
			Alias = alias;
			ColumnName = columnName;
			_counter = _gCounter++;
		}

		internal bool IsEquivalentTo(string columnName)
		{
			return
				(ColumnName.Trim('[', ']').Equals(columnName))
				||
				(_aliasOf != null && _aliasOf.IsEquivalentTo(columnName));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="columnName"></param>
		/// <param name="sourceAlias"></param>
		public ColumnInfo(string columnName, QueryIdentifier sourceAlias)
		{
			//ColumnAlias = columnAlias;
			Alias = sourceAlias;
			ColumnName = columnName;
			_counter = _gCounter++;
		}

		internal string NaturalName
		{
			get { return _aliasOf?.NaturalName ?? ColumnName; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string ColumnName { get; }
		/// <summary>
		/// 
		/// </summary>
		public QueryIdentifier Alias { get; }
		/// <summary>
		/// 
		/// </summary>
		//public QueryIdentifier ColumnAlias { get; }
		private int _counter;
		private static int _gCounter;

		/// <summary>
		/// 
		/// </summary>
		public string ColumnIdentifier()
		{
			return $"[C{_counter}]";
		}

		/// <summary>
		/// 
		/// </summary>
		public string ColumnSourceAlias()
		{
			return $"[{Alias.GetAlias()}].[{ColumnName.Trim('[', ']')}]";
		}

		/// <summary>
		/// 
		/// </summary>
		public string ColumnAliasStatement()
		{
			return $"{ColumnSourceAlias()} AS {ColumnIdentifier()}";
		}
	}
}