using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	/// <summary>
	///		Internal Use ONLY
	/// </summary>
	public class ColumnInfo
	{
		private readonly ColumnInfo _aliasOf;
		private readonly IQueryContainer _container;

		internal ColumnInfo(string columnName,
			ColumnInfo aliasOf,
			QueryIdentifier alias,
			IQueryContainer container) 
			: this(columnName, alias, container)
		{
			_aliasOf = aliasOf;
		}

		internal bool IsEquivalentTo(string columnName)
		{
			return
				(ColumnName.TrimAlias().Equals(columnName))
				||
				(_aliasOf != null && _aliasOf.IsEquivalentTo(columnName));
		}

		/// <summary>
		/// 
		/// </summary>
		public ColumnInfo(string columnName,
			QueryIdentifier sourceAlias,
			IQueryContainer container)
		{
			Alias = sourceAlias;
			_container = container;
			ColumnName = columnName;
			_counter = container?.GetNextColumnId() ?? -1;
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
		private int _counter;

		/// <summary>
		/// 
		/// </summary>
		public string ColumnIdentifier()
		{
			if (_counter == -1)
			{
				return ColumnName;
			}
			return $"[C{_counter}]";
		}

		/// <summary>
		/// 
		/// </summary>
		public string ColumnSourceAlias()
		{
			return $"[{Alias.GetAlias()}].[{ColumnName.TrimAlias()}]";
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