using System.Data;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;

namespace JPB.DataAccess.AdoWrapper.MsSqlProvider
{
	internal class MsSqlPagerPart : IQueryPart
	{
		public int Page { get; set; }
		public int PageSize { get; set; }

		public IDbCommand Process(IQueryContainer container)
		{
			return container.AccessLayer.Database.CreateCommandWithParameterValues(
				"OFFSET @PagedRows ROWS FETCH NEXT @PageSize ROWS ONLY",
				new[]
				{
					new QueryParameter("@PagedRows", (Page - 1) * PageSize),
					new QueryParameter("@PageSize", PageSize),
				});
		}
	}
}