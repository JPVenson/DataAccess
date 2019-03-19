using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class SubSelectQueryPart : IIdentifiableQueryPart
	{
		public SubSelectQueryPart(QueryIdentifier queryAlias)
		{
			Alias = queryAlias;
			SubSelectionQueryParts = new List<IQueryPart>();
		}

		public List<IQueryPart> SubSelectionQueryParts { get; private set; }

		public IDbCommand Process(IQueryContainer container)
		{
			var subSelect =
				container.AccessLayer.Database.MergeTextToParameters(
					SubSelectionQueryParts.Select(e => e.Process(container)).ToArray(), true);
			var select = container.AccessLayer.Database.CreateCommand(
				$"SELECT * FROM ({subSelect.CommandText}) AS [{Alias.GetAlias()}]",
				subSelect.Parameters.OfType<IDataParameter>().ToArray());
			return select;
		}

		public QueryIdentifier Alias { get; }
	}
}