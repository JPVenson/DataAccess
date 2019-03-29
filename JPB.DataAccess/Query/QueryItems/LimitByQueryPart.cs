using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class LimitByQueryPart : IQueryPart
	{
		private readonly int _limitBy;

		public LimitByQueryPart(int limitBy)
		{
			_limitBy = limitBy;
		}

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			return new QueryFactoryResult($"LIMIT {_limitBy}");
		}
	}
}