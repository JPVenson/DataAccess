using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.SqLite
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