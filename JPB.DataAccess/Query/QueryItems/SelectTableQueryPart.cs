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

namespace JPB.DataAccess.Query.QueryItems
{
	internal class SelectTableQueryPart : ISelectableQueryPart
	{
		private readonly string _source;
		private readonly DbClassInfoCache _tableInfo;
		private IList<ColumnInfo> _columns;

		public SelectTableQueryPart(string source,
			DbClassInfoCache tableInfo,
			QueryIdentifier alias)
		{
			_source = source;
			_tableInfo = tableInfo;
			Alias = alias;
			_columns = new List<ColumnInfo>();
			Joins = new List<JoinTableQueryPart>();

			_columns = DbAccessLayer.GetSelectableColumnsOf(_tableInfo, null)
				.Select(e => new ColumnInfo(e, Alias))
				.ToList();
			ColumnMappings = new Dictionary<Type, ColumnInfo[]>();
			ColumnMappings[_tableInfo.Type] = _columns.ToArray();
		}

		public SelectTableQueryPart(ISelectableQueryPart source,
			DbClassInfoCache tableInfo,
			QueryIdentifier alias)
		{
			_source = source.Alias.Value;
			_tableInfo = tableInfo;
			Alias = alias;
			_columns = new List<ColumnInfo>();
			Joins = new List<JoinTableQueryPart>();

			_columns = source.Columns
				.Select(e => new ColumnInfo(e.ColumnIdentifier().Trim('[', ']'), e, Alias)).ToArray();
			ColumnMappings = new Dictionary<Type, ColumnInfo[]>();
			ColumnMappings[_tableInfo.Type] = _columns.ToArray();
		}

		public void AddJoin(JoinTableQueryPart join)
		{
			var columns = new List<ColumnInfo>();
			foreach (var column in join.Columns)
			{
				columns.Add(column);
				_columns.Add(column);
			}

			ColumnMappings[join.Type] = columns.ToArray();
		}

		public bool Distinct { get; set; }
		public int? Limit { get; set; }
		public IDictionary<Type, ColumnInfo[]> ColumnMappings { get; private set; }

		public IEnumerable<ColumnInfo> Columns
		{
			get { return _columns; }
		}

		public IEnumerable<JoinTableQueryPart> Joins { get; private set; }

		public IDbCommand Process(IQueryContainer container)
		{
			ColumnMapper mappings;
			container.PostProcessors.Add(mappings = new ColumnMapper());
			foreach (var columnMapping in ColumnMappings)
			{
				mappings.Mappings.Add(columnMapping);
			}

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

			return container.AccessLayer.Database.CreateCommand(query);

			//var selectQuery = container.AccessLayer.CreateSelectQueryFactory(
			//	_tableInfo,
			//	() =>
			//	{

			//	},
			//	_argumentsForFactory);

			//return selectQuery;
		}

		public QueryIdentifier Alias { get; }
	}
}
