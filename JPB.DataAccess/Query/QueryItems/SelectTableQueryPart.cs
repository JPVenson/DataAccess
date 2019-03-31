using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

		public SelectTableQueryPart(string source,
			DbClassInfoCache tableInfo,
			QueryIdentifier alias, 
			IQueryContainer queryContainer)
		{
			_source = source;
			_tableInfo = tableInfo;
			Alias = alias;
			_columns = new List<ColumnInfo>();
			Joins = new List<JoinTableQueryPart>();

			_columns = DbAccessLayer.GetSelectableColumnsOf(_tableInfo, null)
				.Select(e => new ColumnInfo(e, Alias, queryContainer))
				.ToList();
		}

		public SelectTableQueryPart(ISelectableQueryPart source,
			DbClassInfoCache tableInfo,
			QueryIdentifier alias, 
			IQueryContainer queryContainer)
		{
			_source = source.Alias.Value;
			_tableInfo = tableInfo;
			Alias = alias;
			_columns = new List<ColumnInfo>();
			Joins = new List<JoinTableQueryPart>();

			_columns = source.Columns
				.Select(e => new ColumnInfo(e.ColumnIdentifier().TrimAlias(), e, Alias, queryContainer)).ToArray();
		}

		public void AddJoin(JoinTableQueryPart join)
		{
			var columns = new List<ColumnInfo>();
			foreach (var column in join.Columns)
			{
				columns.Add(column);
				_columns.Add(column);
			}
		}

		public bool Distinct { get; set; }
		public int? Limit { get; set; }

		public IEnumerable<ColumnInfo> Columns
		{
			get { return _columns; }
		}

		public IEnumerable<JoinTableQueryPart> Joins { get; private set; }

		public IQueryFactoryResult Process(IQueryContainer container)
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

			var query = DbAccessLayer.CreateSelectByColumns(_source,
				Columns.Select(e => e.ColumnAliasStatement()).Aggregate((e, f) => e + ", " + f), Alias.GetAlias(),
				modifier);
			return new QueryFactoryResult(query);
		}

		public QueryIdentifier Alias { get; }
	}
}
