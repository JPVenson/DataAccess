using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.QueryFactory;

namespace JPB.DataAccess.Framework.Query.QueryItems
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