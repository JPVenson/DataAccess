using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class DeleteTableQueryPart : IIdentifiableQueryPart
	{
		private readonly QueryIdentifier _target;

		public DeleteTableQueryPart(QueryIdentifier target, QueryIdentifier alias)
		{
			Alias = alias;
			_target = target;
		}

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			return new QueryFactoryResult($"DELETE FROM {_target.GetAlias()} AS {Alias.GetAlias()}");
		}

		public QueryIdentifier Alias { get; }
	}
}