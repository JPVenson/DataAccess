using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Framework;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class UpdateTableWithQueryPart : ISelectableQueryPart
	{
		private readonly QueryIdentifier _target;

		public List<ColumnAssignment> ColumnAssignments { get; set; }

		internal class ColumnAssignment
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
					switch (container.AccessLayer.DbAccessType)
					{
						case DbAccessType.MsSql:
						case DbAccessType.MySql:
							return new ColumnInfo(e, alias, container);
						case DbAccessType.Experimental:
						case DbAccessType.Unknown:
						case DbAccessType.OleDb:
						case DbAccessType.Obdc:
						case DbAccessType.SqLite:
							return new ColumnInfo(e, sourceReference, null);
						default:
							throw new ArgumentOutOfRangeException();
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

			switch (container.AccessLayer.DbAccessType)
			{
				case DbAccessType.MsSql:
				case DbAccessType.MySql:
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
					break;
				case DbAccessType.Experimental:
				case DbAccessType.Unknown:
				case DbAccessType.OleDb:
				case DbAccessType.Obdc:
				case DbAccessType.SqLite:
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
					break;
				default:
					throw new ArgumentOutOfRangeException();
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