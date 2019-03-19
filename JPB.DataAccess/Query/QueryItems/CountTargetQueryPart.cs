using System.Data;
using System.Text;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.QueryItems.Conditional
{
	internal class CountTargetQueryPart : IQueryPart
	{
		private readonly QueryIdentifier _queryId;

		public CountTargetQueryPart(QueryIdentifier queryId)
		{
			_queryId = queryId;
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
			sb.Append("1) FROM ");
			sb.Append(_queryId.Value);
			return container.AccessLayer.Database.CreateCommand(sb.ToString());
		}
	}
}