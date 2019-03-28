using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;
#pragma warning disable 1591

namespace JPB.DataAccess.Query.QueryItems
{
	public class JoinTableQueryPart : ISelectableQueryPart
	{
		public string TargetColumn { get; }
		public string SourceColumn { get; }
		public JoinTableQueryPart(QueryIdentifier targetTable,
			QueryIdentifier sourceTable,
			QueryIdentifier joinAlias,
			Type targetTargetTableType,
			string targetColumn,
			string sourceColumn,
			IEnumerable<string> columns,
			IQueryContainer queryContainer)
		{
			TargetTable = targetTable;
			SourceTable = sourceTable;
			Alias = joinAlias;
			TargetTableType = targetTargetTableType;

			TargetColumn = targetColumn;
			SourceColumn = sourceColumn;
			Columns = new List<ColumnInfo>(columns.Select(e => new ColumnInfo(e, Alias, queryContainer)));
			DependingJoins = new List<JoinTableQueryPart>();
		}
		public QueryIdentifier TargetTable { get; private set; }
		public QueryIdentifier SourceTable { get; private set; }
		public IEnumerable<ColumnInfo> Columns { get; private set; }

		public IDbCommand Process(IQueryContainer container)
		{
			var joinBuilder = new StringBuilder();
			joinBuilder.Append($"JOIN [{TargetTable.GetAlias()}] AS [{Alias.GetAlias()}]" +
			                   $" ON [{Alias.GetAlias()}].[{TargetColumn}]" +
			                   $" = " +
			                   $"[{SourceTable.GetAlias()}].[{SourceColumn}]");

			return container.AccessLayer.Database.CreateCommand(joinBuilder.ToString());
		}

		public QueryIdentifier Alias { get; }
		public bool Distinct { get; set; }
		public int? Limit { get; set; }
		public Type TargetTableType { get; set; }
		public List<JoinTableQueryPart> DependingJoins { get; set; }
	}
}