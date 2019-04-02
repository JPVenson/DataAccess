﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;
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
		public ColumnInfo SourceColumnName { get; set; }
		public ColumnInfo TargetColumnName { get; set; }
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
				SourceColumnName = new ColumnInfo(SourceColumnName.ColumnName, SourceColumnName, alias, SourceColumnName._container),
				TargetColumnName = new ColumnInfo(TargetColumnName.ColumnName, TargetColumnName, alias, TargetColumnName._container)
			};
		}

		public JoinParseInfo CloneForJoinTo(
			QueryIdentifier alias,
			IList<ColumnInfo> columnSource,
			IEnumerable<ColumnInfo> originalColumnSource)
		{
			return new JoinParseInfo()
			{
				TargetProperty = TargetProperty,
				Alias = @alias,
				Columns = Columns.Select(e => columnSource.First(f => f.AliasOf.Equals(e))).ToArray(),
				DependingJoins = DependingJoins.Select(e => e.CloneForJoinTo(@alias, columnSource, originalColumnSource)).ToArray(),
				TargetTableType = TargetTableType,
				SourceTable = @alias,
				SourceColumnName = Columns.FirstOrDefault(f => f.AliasOf?.Equals(SourceColumnName) == true) ?? SourceColumnName,
				TargetColumnName = Columns.FirstOrDefault(f => f.AliasOf?.Equals(TargetColumnName) == true) ?? TargetColumnName
				//SourceColumnName = SourceColumnName,
				//TargetColumnName = TargetColumnName
			};
		}
	}

	public class JoinTableQueryPart : ISelectableQueryPart
	{
		private readonly JoinMode _joinAs;
		public JoinParseInfo JoinParseInfo { get; private set; }

		public ColumnInfo TargetColumn { get; }
		public ColumnInfo SourceColumn { get; }

		public JoinTableQueryPart(QueryIdentifier targetTable,
			QueryIdentifier sourceTable,
			QueryIdentifier joinAlias,
			Type targetTargetTableType,
			ColumnInfo targetColumn,
			ColumnInfo sourceColumn,
			IEnumerable<ColumnInfo> columns,
			DbPropertyInfoCache targetProperty, 
			JoinMode joinAs)
		{
			_joinAs = joinAs;
			TargetTable = targetTable;
			SourceTable = sourceTable;
			Alias = joinAlias;
			TargetTableType = targetTargetTableType;
			Columns = columns;

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
			joinBuilder.Append($"{_joinAs.JoinType} JOIN [{TargetTable.GetAlias()}] AS [{Alias.GetAlias()}]" +
							   $" ON [{Alias.GetAlias()}].[{TargetColumn.ColumnName.TrimAlias()}]" +
							   $" = " +
							   $"[{SourceTable.GetAlias()}].[{SourceColumn.ColumnName}]");
			return new QueryFactoryResult(joinBuilder.ToString());
		}

		public QueryIdentifier Alias { get; }
		public bool Distinct { get; set; }
		public int? Limit { get; set; }
		public Type TargetTableType { get; set; }
		public List<JoinTableQueryPart> DependingJoins { get; set; }
	}
}