using System.Data;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class DeleteTableQueryPart : IQueryPart
	{
		private readonly DbClassInfoCache _classInfo;

		public DeleteTableQueryPart(DbClassInfoCache classInfo)
		{
			_classInfo = classInfo;
		}

		public IDbCommand Process(IQueryContainer container)
		{
			return DbAccessLayer
				.CreateDelete(container
					.AccessLayer.Database, _classInfo);
		}
	}
}