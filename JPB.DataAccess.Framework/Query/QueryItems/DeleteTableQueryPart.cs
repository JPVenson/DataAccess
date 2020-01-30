using System;
using System.Collections.Generic;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class DeleteTableQueryPart : ISelectableQueryPart
	{
		private readonly QueryIdentifier _target;
		private readonly IEnumerable<ColumnInfo> _columns;

		public DeleteTableQueryPart(QueryIdentifier target,
			QueryIdentifier alias,
			DbClassInfoCache tableInfo,
			IQueryContainer queryContainer, 
			IEnumerable<ColumnInfo> columns)
		{
			Alias = alias;
			_columns = columns;
			_target = target;
		}

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			if (container.AccessLayer.DbAccessType.HasFlag(DbAccessType.SqLite))
			{
				return new QueryFactoryResult($"DELETE FROM {_target.GetAlias().EnsureAlias()} ");
			}
			else
			{
				return new QueryFactoryResult($"DELETE {Alias.GetAlias().EnsureAlias()} " +
				                              $"FROM {_target.GetAlias().EnsureAlias()} " +
				                              $"AS {Alias.GetAlias().EnsureAlias()}");
			}
		}

		public QueryIdentifier Alias { get; }
		public bool Distinct { get; set; }
		public int? Limit { get; set; }

		public IEnumerable<ColumnInfo> Columns
		{
			get { return _columns; }
		}
	}
}