using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class LimitByQueryPart : IQueryPart
	{
		private readonly int _limitBy;

		public LimitByQueryPart(int limitBy)
		{
			_limitBy = limitBy;
		}

		public IDbCommand Process(IQueryContainer container)
		{
			return container.AccessLayer.Database.CreateCommand($"LIMIT {_limitBy}");
		}
	}

	internal class CteQueryPart : IQueryPart
	{
		internal class CteInfo
		{
			public CteInfo()
			{
				CteContentParts = new List<IQueryPart>();
			}

			public QueryIdentifier Name { get; set; }
			public List<IQueryPart> CteContentParts { get; private set; }
		}

		public CteQueryPart()
		{
			CteInfos = new List<CteInfo>();
		}

		public List<CteInfo> CteInfos { get; set; }

		public IDbCommand Process(IQueryContainer container)
		{
			var commandBuilder = new StringBuilder();
			var commands = new List<IDbCommand>();
			var first = true;
			foreach (var cteInfo in CteInfos)
			{
				if (!first)
				{
					commandBuilder.Append(", ");
				}

				first = false;
				commandBuilder.Append($"WITH {cteInfo.Name.Value} AS (");
				var cteCommand = container.AccessLayer.Database.MergeTextToParameters(cteInfo.CteContentParts.Select(e => e.Process(container)).ToArray(), true);
				commandBuilder.Append(cteCommand.CommandText);
				commandBuilder.Append(")");
				commands.Add(container.AccessLayer.Database.CreateCommandWithParameterValues(commandBuilder.ToString(),
					cteCommand.Parameters.OfType<IDataParameter>().ToArray()));
			}

			return container.AccessLayer.Database.MergeTextToParameters(commands.ToArray(), true);
		}
	}
}