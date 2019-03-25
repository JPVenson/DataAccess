using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class SubSelectQueryPart : ISelectableQueryPart
	{
		private readonly Type _targetTable;
		private IList<ColumnInfo> _columns;

		public SubSelectQueryPart(QueryIdentifier queryAlias,
			IEnumerable<IQueryPart> subSelectionQueryParts,
			Type targetTable)
		{
			_targetTable = targetTable;
			Alias = queryAlias;
			SubSelectionQueryParts = subSelectionQueryParts;
			_columns = SubSelectionQueryParts
				.OfType<ISelectableQueryPart>()
				.LastOrDefault()?
				.Columns
				.Select(e => new ColumnInfo(e.ColumnIdentifier().Trim('[', ']'), e, Alias))
				.ToArray();
		}

		public IEnumerable<IQueryPart> SubSelectionQueryParts { get; private set; }

		public IDbCommand Process(IQueryContainer container)
		{
			ColumnMapper mapper;
			container.PostProcessors.Add(mapper = new ColumnMapper());
			mapper.Mappings[_targetTable] = Columns.ToArray();

			var subSelect =
				container.AccessLayer.Database.MergeTextToParameters(
					SubSelectionQueryParts.Select(e => e.Process(container)).Where(e => e != null).ToArray(), true);
			var modifer = Distinct ? "DISTINCT" : "";
			modifer += Limit.HasValue ? " TOP" + Limit.Value : "";

			var select = container.AccessLayer.Database.CreateCommand(
				$"SELECT {modifer} {Columns.Select(e => e.ColumnAliasStatement()).Aggregate((e, f) => e + "," + f)} " +
				$"FROM ({subSelect.CommandText}) AS [{Alias.GetAlias()}]",
				subSelect.Parameters.OfType<IDataParameter>().ToArray());
			return select;
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