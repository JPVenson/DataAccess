using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	public class SynteticColumnInfo : ColumnInfo
	{
		public SynteticColumnInfo(string columnName, 
			QueryIdentifier sourceAlias, 
			IQueryContainer container) 
			: base(columnName, sourceAlias, container)
		{
		}

		public override string ColumnAliasStatement()
		{
			return $"{ColumnName} AS {ColumnIdentifier()}";
		}

		/// <summary>
		/// 
		/// </summary>
		public override string ColumnSourceAlias()
		{
			return $"[{Alias.GetAlias().TrimAlias()}].[{ColumnIdentifier().TrimAlias()}]";
		}
	}

	/// <summary>
	///		Internal Use ONLY
	/// </summary>
	public class ColumnInfo
	{
		/// <summary>
		///		If this is an inhered column of another query the original column
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
		///		The Real name of this column
		/// </summary>
		internal string NaturalName
		{
			get { return AliasOf?.NaturalName ?? ColumnName; }
		}

		/// <summary>
		///		The Generated Unique Column name 
		/// </summary>
		public string ColumnName { get; }

		/// <summary>
		///		The Alias of the Table or View this column origines from
		/// </summary>
		/// 
		public QueryIdentifier Alias { get; }
		public QueryIdentifier ColumnIdentifierEntry { get; set; }

		private int _counter;

		/// <summary>
		/// 
		/// </summary>
		public virtual string ColumnIdentifier()
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
		public virtual string ColumnSourceAlias()
		{
			return $"[{Alias.GetAlias().TrimAlias()}].[{ColumnName.TrimAlias()}]";
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual string ColumnAliasStatement()
		{
			return $"{ColumnSourceAlias()} AS {ColumnIdentifier()}";
		}
	}
}