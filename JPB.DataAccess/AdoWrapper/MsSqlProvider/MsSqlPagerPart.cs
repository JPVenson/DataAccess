using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.AdoWrapper.MsSqlProvider
{
	internal class MsSqlPagerPart : IQueryPart
	{
		public int Page { get; set; }
		public int PageSize { get; set; }

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			return new QueryFactoryResult("OFFSET @PagedRows ROWS FETCH NEXT @PageSize ROWS ONLY",
				new QueryParameter("@PagedRows", (Page - 1) * PageSize),
				new QueryParameter("@PageSize", PageSize));
		}
	}
}