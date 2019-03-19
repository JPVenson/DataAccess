using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Input;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	internal interface ISelectableQueryPart : IIdentifiableQueryPart
	{
		bool Distinct { get; set; }
		int? Limit { get; set; }
	}

	internal class SelectTableQueryPart : ISelectableQueryPart
	{
		private readonly string _source;
		private readonly DbClassInfoCache _tableInfo;
		private readonly object[] _argumentsForFactory;

		public SelectTableQueryPart(string source, DbClassInfoCache tableInfo, QueryIdentifier alias, params object[] argumentsForFactory)
		{
			_source = source;
			_tableInfo = tableInfo;
			_argumentsForFactory = argumentsForFactory;
			Alias = alias;
		}

		public bool Distinct { get; set; }
		public int? Limit { get; set; }

		public IDbCommand Process(IQueryContainer container)
		{
			var selectQuery = container.AccessLayer.CreateSelectQueryFactory(
				_tableInfo,
				() =>
				{
					string modifier = null;

					if (Limit.HasValue && container.AccessLayer.DbAccessType == DbAccessType.MsSql)
					{
						modifier = $"TOP {Limit.Value}";
					}

					if (Distinct)
					{
						modifier += " DISTINCT";
					}

					return container.AccessLayer.Database.CreateCommand(DbAccessLayer.CreateSelect(_source, _tableInfo, Alias.Value,
						modifier));
				},
				_argumentsForFactory);
			return selectQuery;
		}

		public QueryIdentifier Alias { get; }
	}
}
