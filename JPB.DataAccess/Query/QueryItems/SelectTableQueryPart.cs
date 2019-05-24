using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class SelectTableQueryPart : ISelectableQueryPart
	{
		private readonly string _source;
		private readonly DbClassInfoCache _tableInfo;
		private IList<ColumnInfo> _columns;
		private readonly List<JoinParseInfo> _joins;

		public SelectTableQueryPart(string source,
			DbClassInfoCache tableInfo,
			QueryIdentifier alias, 
			IQueryContainer queryContainer)
		{
			_source = source;
			_tableInfo = tableInfo;
			Alias = alias;
			_columns = new List<ColumnInfo>();
			_joins = new List<JoinParseInfo>();

			_columns = DbAccessLayer.GetSelectableColumnsOf(_tableInfo)
				.Select(e => new ColumnInfo(e, Alias, queryContainer))
				.ToList();
		}

		public SelectTableQueryPart(ISelectableQueryPart source,
			DbClassInfoCache tableInfo,
			QueryIdentifier alias, 
			IQueryContainer queryContainer)
		{
			_source = source.Alias.GetAlias();
			_tableInfo = tableInfo;
			Alias = alias;
			_columns = new List<ColumnInfo>();
			_joins = new List<JoinParseInfo>();

			_columns = source.Columns
				.Select(e => new ColumnInfo(e.ColumnIdentifier().TrimAlias(), e, Alias, queryContainer)).ToList();
			
			if (source is CteDefinitionQueryPart.CteQueryPart cte)
			{
				queryContainer.Joins.Clear();
				var joinTableQueryParts = (cte.CteInfo.CteContentParts.FirstOrDefault(f => f is SelectTableQueryPart) as SelectTableQueryPart)
					?.Joins;
				if (joinTableQueryParts != null)
				{
					foreach (var joinTableQueryPart in joinTableQueryParts)
					{
						var cloneForJoinTo = joinTableQueryPart.CloneForJoinTo(Alias, _columns, source.Columns);
						_joins.Add(cloneForJoinTo);
						queryContainer.Joins.Add(cloneForJoinTo);
					}
				}
			}
		}

		public void AddJoin(JoinTableQueryPart join)
		{
			_joins.Add(join.JoinParseInfo);
			foreach (var column in join.Columns)
			{
				_columns.Add(column);
			}
		}

		public bool Distinct { get; set; }
		public int? Limit { get; set; }

		public IEnumerable<ColumnInfo> Columns
		{
			get { return _columns; }
		}

		public IEnumerable<JoinParseInfo> Joins
		{
			get { return _joins; }
		}

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			string modifier = null;

			if (Distinct)
			{
				modifier += "DISTINCT ";
			}

			if (Limit.HasValue && container.AccessLayer.DbAccessType == DbAccessType.MsSql)
			{
				modifier += $"TOP {Limit.Value} ";
			}

			var sb = new StringBuilder();
			sb.Append("SELECT ");
			if (modifier != null)
			{
				sb.Append(modifier);
			}

			sb.Append(Columns.Select(e => e.ColumnAliasStatement()).Aggregate((e, f) => e + ", " + f));
			sb.Append(" FROM ");
			sb.Append($"[{_source}] ");
			if (Alias != null)
			{
				sb.Append($"AS [{Alias.GetAlias()}] ");
			}

			//var query = DbAccessLayer.CreateSelectByColumns(_source,
			//	Columns.Select(e => e.ColumnAliasStatement()).Aggregate((e, f) => e + ", " + f), Alias.GetAlias(),
			//	modifier);
			return new QueryFactoryResult(sb.ToString());
		}

		public QueryIdentifier Alias { get; }
	}
}
