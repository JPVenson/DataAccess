using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class CteDefinitionQueryPart : IQueryPart
	{
		private readonly IList<CteInfo> _cteInfos;

		internal class CteInfo
		{
			public CteInfo()
			{
				CteContentParts = new List<IQueryPart>();
			}

			public QueryIdentifier Name { get; set; }
			public List<IQueryPart> CteContentParts { get; private set; }
		}

		public CteDefinitionQueryPart()
		{
			_cteInfos = new List<CteInfo>();
		}

		public IEnumerable<CteInfo> CteInfos
		{
			get { return _cteInfos; }
		}

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
				var cteCommand = container.AccessLayer.Database.MergeTextToParameters(
					cteInfo.CteContentParts.Select(e => e.Process(container)).Where(e => e != null).ToArray(), true);
				commandBuilder.Append(cteCommand.CommandText);
				commandBuilder.Append(")");
				commands.Add(container.AccessLayer.Database.CreateCommandWithParameterValues(commandBuilder.ToString(),
					cteCommand.Parameters.OfType<IDataParameter>().ToArray()));
			}

			return container.AccessLayer.Database.MergeTextToParameters(commands.ToArray(), true);
		}

		class CteQueryPart : ISelectableQueryPart
		{
			private readonly CteInfo _cteInfo;
			private readonly IEnumerable<ColumnInfo> _columns;

			public CteQueryPart(CteInfo cteInfo)
			{
				_cteInfo = cteInfo;
				_columns = cteInfo.CteContentParts
					.OfType<ISelectableQueryPart>()
					.LastOrDefault()?
					.Columns;
			}

			public IDbCommand Process(IQueryContainer container)
			{
				return null;
			}

			public QueryIdentifier Alias
			{
				get { return _cteInfo.Name; }
			}

			public bool Distinct { get; set; }
			public int? Limit { get; set; }

			public IEnumerable<ColumnInfo> Columns
			{
				get { return _columns; }
			}
		}

		public ISelectableQueryPart AddCte(CteInfo cteInfo)
		{
			_cteInfos.Add(cteInfo);
			return new CteQueryPart(cteInfo);
		}
	}
}