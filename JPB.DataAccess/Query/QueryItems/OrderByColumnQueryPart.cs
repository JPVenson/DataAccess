using System.Collections.Generic;
using System.Data;
using System.Linq;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class OrderByColumnQueryPart : IQueryPart
	{
		public OrderByColumnQueryPart()
		{
			Columns = new List<string>();
		}

		public List<string> Columns { get; set; }
		public bool Descending { get; set; }

		public IDbCommand Process(IQueryContainer container)
		{
			var mode = Descending ? "DESC" : "ASC";
			return container.AccessLayer.Database.CreateCommand(
				$"ORDER BY {Columns.Aggregate((e, f) => e + ", " + f)} {mode}");
		}
	}
}