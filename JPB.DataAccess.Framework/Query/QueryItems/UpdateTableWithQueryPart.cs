﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Query.QueryItems
{
	public class UpdateTableWithQueryPart : ISelectableQueryPart
	{
		private readonly QueryIdentifier _target;

		public List<ColumnAssignment> ColumnAssignments { get; set; }

		public class ColumnAssignment
		{
			public ColumnAssignment()
			{
				QueryParameters = new List<IQueryParameter>();
			}

			public string Column { get; set; }
			public string Value { get; set; }
			public List<IQueryParameter> QueryParameters { get; set; }
		}

		public static IEnumerable<ColumnInfo> ColumsOfType(DbClassInfoCache dbClassInfoCache,
			QueryIdentifier alias,
			QueryIdentifier sourceReference,
			IQueryContainer container)
		{
			return DbAccessLayer.GetSelectableColumnsOf(dbClassInfoCache)
				.Select(e =>
				{
					if (container.AccessLayer.DbAccessType.HasFlag(DbAccessType.MsSql) ||
					    container.AccessLayer.DbAccessType.HasFlag(DbAccessType.MySql))
					{
						return new ColumnInfo(e, alias, container);
					}
					else
					{
						return new ColumnInfo(e, sourceReference, null);
					}
				}).ToArray();
		}

		public UpdateTableWithQueryPart(
			QueryIdentifier target, 
			IEnumerable<ColumnInfo> targetsColumns,
			QueryIdentifier queryIdentifier)
		{
			Columns = targetsColumns;
			_target = target;
			Alias = queryIdentifier;
			ColumnAssignments = new List<ColumnAssignment>();
		}

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			var query = new StringBuilder();

			if (container.AccessLayer.DbAccessType.HasFlag(DbAccessType.MsSql) ||
			    container.AccessLayer.DbAccessType.HasFlag(DbAccessType.MySql))
			{
				query.Append($"UPDATE [{Alias.GetAlias()}] SET ");
				query
					.Append(
						ColumnAssignments
							.Select(
								columnAssignment =>
									$"{Alias.GetAlias().EnsureAlias()}.{columnAssignment.Column.EnsureAlias()} = {columnAssignment.Value}")
							.Aggregate((e, f) => e + ", " + f)
					);
				query.Append($" FROM {_target.GetAlias().EnsureAlias()} AS {Alias.GetAlias().EnsureAlias()}");
			}
			else
			{
				query.Append($"UPDATE {_target.GetAlias().EnsureAlias()} SET ");
				query
					.Append(
						ColumnAssignments
							.Select(
								columnAssignment =>
									$"{columnAssignment.Column.EnsureAlias()} = {columnAssignment.Value}")
							.Aggregate((e, f) => e + ", " + f)
					);
				query.Append($"");
			}

			return new QueryFactoryResult(query.ToString(),
				ColumnAssignments.SelectMany(f => f.QueryParameters).ToArray());
		}

		public QueryIdentifier Alias { get; }
		public bool Distinct { get; set; }
		public int? Limit { get; set; }
		public IEnumerable<ColumnInfo> Columns { get; }
	}
}