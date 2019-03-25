using System.Collections.Generic;
using System.Data;
using System.Text;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.QueryItems.Conditional
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
				new ColumnInfo("[Source]", Alias), 
			};
		}

		public bool DistinctMode { get; set; }

		public IDbCommand Process(IQueryContainer container)
		{
			var sb = new StringBuilder();
			sb.Append("SELECT COUNT( ");
			if (DistinctMode)
			{
				sb.Append("DISTINCT ");
			}
			sb.Append("1) AS [Count] FROM ");
			sb.Append(_queryId.Value);
			sb.Append(" AS ");
			sb.Append(Alias.GetAlias());
			return container.AccessLayer.Database.CreateCommand(sb.ToString());
		}

		public QueryIdentifier Alias { get; }
		public bool Distinct { get; set; }
		public int? Limit { get; set; }
		public IEnumerable<ColumnInfo> Columns { get; }
	}
}