using System.Data;
using JPB.DataAccess.Query.Contracts;

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
}