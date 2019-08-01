using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Framework.Query.QueryItems
{
	/// <summary>
	///		Internal Use ONLY
	/// </summary>
	public class ColumnInfo
	{
		/// <summary>
		/// 
		/// </summary>
		public ColumnInfo AliasOf { get; }
		internal readonly IQueryContainer _container;

		internal ColumnInfo(string columnName,
			ColumnInfo aliasOf,
			QueryIdentifier alias,
			IQueryContainer container) 
			: this(columnName, alias, container)
		{
			AliasOf = aliasOf;
		}

		internal bool IsEquivalentTo(string columnName)
		{
			return
				(ColumnName.Equals(columnName))
				||
				(AliasOf != null && AliasOf.IsEquivalentTo(columnName));
		}

		internal bool IsEqualsTo(ColumnInfo column)
		{
			if (column.AliasOf == column || AliasOf == column)
			{
				return true;
			}
			
			if (column.IsEquivalentTo(ColumnName) && Alias.Equals(column.Alias))
			{
				return true;
			}

			return false;
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
			get { return AliasOf?.NaturalName ?? ColumnName; }
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
			return $"[{Alias.GetAlias()}].[{ColumnName}]";
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