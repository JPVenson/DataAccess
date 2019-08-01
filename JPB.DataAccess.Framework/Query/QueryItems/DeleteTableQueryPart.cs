using System;
using System.Collections.Generic;
using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.DbInfoConfig.DbInfo;
using JPB.DataAccess.Framework.Manager;
using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.Query.QueryItems.Conditional;
using JPB.DataAccess.Framework.QueryFactory;

namespace JPB.DataAccess.Framework.Query.QueryItems
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
			switch (container.AccessLayer.DbAccessType)
			{
				case DbAccessType.Experimental:
				case DbAccessType.Unknown:
				case DbAccessType.MsSql:
				case DbAccessType.MySql:
				case DbAccessType.OleDb:
				case DbAccessType.Obdc:
					return new QueryFactoryResult($"DELETE {Alias.GetAlias().EnsureAlias()} " +
					                              $"FROM {_target.GetAlias().EnsureAlias()} " +
					                              $"AS {Alias.GetAlias().EnsureAlias()}");
				case DbAccessType.SqLite:
					return new QueryFactoryResult($"DELETE FROM {_target.GetAlias().EnsureAlias()} ");
				default:
					throw new ArgumentOutOfRangeException();
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