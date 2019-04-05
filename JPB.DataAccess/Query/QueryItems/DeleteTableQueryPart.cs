using System.Collections.Generic;
using System.Data;
using System.Linq;
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
			IQueryContainer queryContainer)
		{
			Alias = alias;
			_columns = DbAccessLayer.GetSelectableColumnsOf(tableInfo)
				.Select(e => new ColumnInfo(e, Alias, queryContainer))
				.ToList();
			_target = target;
		}

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			return new QueryFactoryResult($"DELETE [{Alias.GetAlias().TrimAlias()}] " +
			                              $"FROM [{_target.GetAlias().TrimAlias()}] " +
			                              $"AS [{Alias.GetAlias().TrimAlias()}]");
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