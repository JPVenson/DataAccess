using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class OrderByColumnQueryPart : IQueryPart
	{
		public OrderByColumnQueryPart()
		{
			Columns = new List<ColumnInfo>();
		}

		public List<ColumnInfo> Columns { get; set; }
		public bool Descending { get; set; }

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			var mode = Descending ? "DESC" : "ASC";
			return new QueryFactoryResult(
				$"ORDER BY {Columns.Select(e => e.ColumnIdentifier()).Aggregate((e, f) => e + ", " + f)} {mode}");
		}
	}
}