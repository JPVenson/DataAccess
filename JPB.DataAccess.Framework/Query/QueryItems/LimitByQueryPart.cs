using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.QueryFactory;

namespace JPB.DataAccess.Framework.Query.QueryItems
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