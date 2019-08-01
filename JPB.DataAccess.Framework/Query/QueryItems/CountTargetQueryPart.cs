using System.Collections.Generic;
using System.Text;
using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.Query.QueryItems.Conditional;
using JPB.DataAccess.Framework.QueryFactory;

namespace JPB.DataAccess.Framework.Query.QueryItems
{
	internal class CountTargetQueryPart : ISelectableQueryPart
	{
		private readonly QueryIdentifier _queryId;

		public CountTargetQueryPart(QueryIdentifier queryId, QueryIdentifier alias)
		{
			Alias = alias;
			_queryId = queryId;
			Columns = new ColumnInfo[]
			{
				new ColumnInfo("[Source]", Alias, null), 
			};
		}

		public bool DistinctMode { get; set; }

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			var sb = new StringBuilder();
			sb.Append("SELECT COUNT(");
			if (DistinctMode)
			{
				sb.Append(" DISTINCT ");
			}
			if (Limit.HasValue)
			{
				sb.Append(Limit);
			}
			else
			{
				sb.Append("1");
			}
			sb.Append(") AS [Count] FROM ");
			sb.Append(_queryId.Value);
			sb.Append(" AS ");
			sb.Append(Alias.GetAlias());
			return new QueryFactoryResult(sb.ToString());
		}

		public QueryIdentifier Alias { get; }
		public bool Distinct { get; set; }
		public int? Limit { get; set; }
		public IEnumerable<ColumnInfo> Columns { get; }
	}
}