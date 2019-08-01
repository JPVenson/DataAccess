using JPB.DataAccess.Framework.Contacts;
using JPB.DataAccess.Framework.Helper;
using JPB.DataAccess.Framework.Query.Contracts;
using JPB.DataAccess.Framework.Query.QueryItems;
using JPB.DataAccess.Framework.QueryFactory;

namespace JPB.DataAccess.SqLite.NetStandard
{
	public class SqLitePagerPart : IQueryPart
	{
		public IQueryFactoryResult Process(IQueryContainer container)
		{
			return new QueryFactoryResult("LIMIT @PageSize OFFSET @PagedRows",
				new QueryParameter("@PagedRows", (Page - 1) * PageSize),
				new QueryParameter("@PageSize", PageSize));
		}

		public int Page { get; set; }
		public int PageSize { get; set; }
	}
}