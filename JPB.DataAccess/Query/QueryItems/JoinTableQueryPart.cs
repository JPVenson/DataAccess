using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;
using JPB.DataAccess.QueryFactory;

#pragma warning disable 1591

namespace JPB.DataAccess.Query.QueryItems
{
	public class JoinParseInfo
	{
		public JoinParseInfo()
		{
			Columns = new ColumnInfo[0];
			DependingJoins = new List<JoinParseInfo>();
		}

		public DbPropertyInfoCache TargetProperty { get; set; }
		public Type TargetTableType { get; set; }
		public QueryIdentifier SourceTable { get; set; }
		public string SourceColumnName { get; set; }
		public string TargetColumnName { get; set; }
		public QueryIdentifier Alias { get; set; }
		public IList<JoinParseInfo> DependingJoins { get; set; }
		public IEnumerable<ColumnInfo> Columns { get; set; }

		public JoinParseInfo CloneTo(QueryIdentifier @alias)
		{
			return new JoinParseInfo()
			{
				TargetProperty = TargetProperty,
				Alias = @alias,
				Columns = Columns.Select(e => new ColumnInfo(e.ColumnName, @alias, e._container)).ToArray(),
				DependingJoins = DependingJoins.Select(e => e.CloneTo(@alias)).ToArray(),
				TargetTableType = TargetTableType,
				SourceTable = SourceTable,
				SourceColumnName = SourceColumnName,
				TargetColumnName = TargetColumnName
			};
		}

		public JoinParseInfo CloneForJoinTo(QueryIdentifier alias, 
			IList<ColumnInfo> columnSource, 
			IEnumerable<ColumnInfo> originalColumnSource)
		{
			return new JoinParseInfo()
			{
				TargetProperty = TargetProperty,
				Alias = @alias,
				Columns = Columns
					.Select(e =>
					{
						return columnSource.First(f => f.AliasOf.Equals(e));

						//var matchingColumn = originalColumnSource.FirstOrDefault(f => f.IsEquivalentTo(e.ColumnName) && f.Alias.Equals(e.Alias));
						//return columnSource.First(f => f.IsEquivalentTo(e.ColumnName));
						//return new ColumnInfo(e.ColumnName, @alias, e._container);
					}).ToArray(),
				DependingJoins = DependingJoins.Select(e => e.CloneForJoinTo(@alias, columnSource, originalColumnSource)).ToArray(),
				TargetTableType = TargetTableType,
				SourceTable = @alias,
				SourceColumnName = SourceColumnName,
				TargetColumnName = TargetColumnName
			};
		}
	}

	public class JoinTableQueryPart : ISelectableQueryPart
	{
		public JoinParseInfo JoinParseInfo { get; private set; }

		public string TargetColumn { get; }
		public string SourceColumn { get; }

		public JoinTableQueryPart(QueryIdentifier targetTable,
			QueryIdentifier sourceTable,
			QueryIdentifier joinAlias,
			Type targetTargetTableType,
			string targetColumn,
			string sourceColumn,
			IEnumerable<string> columns,
			IQueryContainer queryContainer, 
			DbPropertyInfoCache targetProperty)
		{
			TargetTable = targetTable;
			SourceTable = sourceTable;
			Alias = joinAlias;
			TargetTableType = targetTargetTableType;
			Columns = new List<ColumnInfo>(columns.Select(e => new ColumnInfo(e, Alias, queryContainer)));

			TargetColumn = targetColumn;
			SourceColumn = sourceColumn;
			DependingJoins = new List<JoinTableQueryPart>();

			var joinInfos = new JoinParseInfo();
			joinInfos.TargetProperty = targetProperty;
			joinInfos.Columns = Columns;
			joinInfos.Alias = Alias;
			joinInfos.SourceColumnName = SourceColumn;
			joinInfos.TargetColumnName = TargetColumn;

			joinInfos.SourceTable = SourceTable;
			joinInfos.TargetTableType = targetTargetTableType;
			JoinParseInfo = joinInfos;
		}
		public QueryIdentifier TargetTable { get; private set; }
		public QueryIdentifier SourceTable { get; private set; }
		public IEnumerable<ColumnInfo> Columns { get; private set; }

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			var joinBuilder = new StringBuilder();
			joinBuilder.Append($"JOIN [{TargetTable.GetAlias()}] AS [{Alias.GetAlias()}]" +
			                   $" ON [{Alias.GetAlias()}].[{TargetColumn}]" +
			                   $" = " +
			                   $"[{SourceTable.GetAlias()}].[{SourceColumn}]");
			return new QueryFactoryResult(joinBuilder.ToString());
		}

		public QueryIdentifier Alias { get; }
		public bool Distinct { get; set; }
		public int? Limit { get; set; }
		public Type TargetTableType { get; set; }
		public List<JoinTableQueryPart> DependingJoins { get; set; }
	}
}