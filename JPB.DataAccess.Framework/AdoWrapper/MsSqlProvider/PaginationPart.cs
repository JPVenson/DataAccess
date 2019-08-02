using System;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.AdoWrapper.MsSqlProvider
{
	internal class PaginationPart : IQueryPart
	{
		public int Page { get; set; }
		public int PageSize { get; set; }

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			string query;
			switch (container.AccessLayer.DbAccessType)
			{
				case DbAccessType.MsSql:
					query = "OFFSET @PagedRows ROWS FETCH NEXT @PageSize ROWS ONLY";
					break;
				case DbAccessType.MySql:
				case DbAccessType.SqLite:
					query = "LIMIT @PageSize OFFSET @PagedRows";
					break;
				default:
					throw new NotSupportedException("There are no in build pageing support for these type of Database");
			}

			return new QueryFactoryResult(query,
				new QueryParameter("@PagedRows", (Page - 1) * PageSize),
				new QueryParameter("@PageSize", PageSize));
		}
	}
}