using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class SubSelectQueryPart : ISelectableQueryPart
	{
		private IList<ColumnInfo> _columns;

		public SubSelectQueryPart(QueryIdentifier queryAlias,
			IEnumerable<IQueryPart> subSelectionQueryParts,
			IQueryContainer queryContainer)
		{
			Alias = queryAlias;
			SubSelectionQueryParts = subSelectionQueryParts;
			_columns = SubSelectionQueryParts
				.OfType<ISelectableQueryPart>()
				.LastOrDefault()?
				.Columns
				.Select(e => new ColumnInfo(e.ColumnIdentifier().TrimAlias(), e, Alias, queryContainer))
				.ToArray();
		}

		public IEnumerable<IQueryPart> SubSelectionQueryParts { get; private set; }

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			var subSelect =
				DbAccessLayerHelper.MergeQueryFactoryResult(true, 1, true, null,
					SubSelectionQueryParts.Select(e => e.Process(container)).Where(e => e != null).ToArray());
			var modifer = Distinct ? "DISTINCT" : "";
			modifer += Limit.HasValue ? " TOP" + Limit.Value : "";

			var select = new QueryFactoryResult(
				$"SELECT {modifer} {Columns.Select(e => e.ColumnAliasStatement()).Aggregate((e, f) => e + "," + f)} " +
				$"FROM ({subSelect.Query}) AS [{Alias.GetAlias()}]",
				subSelect.Parameters.ToArray());
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