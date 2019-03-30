﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class CteDefinitionQueryPart : IQueryPart
	{
		private readonly IList<CteInfo> _cteInfos;

		internal class CteInfo
		{
			public CteInfo()
			{
				CteContentParts = new List<IQueryPart>();
			}

			public QueryIdentifier Name { get; set; }
			public List<IQueryPart> CteContentParts { get; private set; }
		}

		public CteDefinitionQueryPart()
		{
			_cteInfos = new List<CteInfo>();
		}

		public IEnumerable<CteInfo> CteInfos
		{
			get { return _cteInfos; }
		}

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			var commandBuilder = new StringBuilder();
			var commands = new List<IQueryFactoryResult>();
			var first = true;
			foreach (var cteInfo in CteInfos)
			{
				if (!first)
				{
					commandBuilder.Append(", ");
				}

				first = false;
				commandBuilder.Append($"WITH {cteInfo.Name.Value} AS (");
				var cteCommand = DbAccessLayerHelper.MergeQueryFactoryResult(true, 1, true, null,
					cteInfo.CteContentParts.Select(e => e.Process(container)).Where(e => e != null).ToArray());
				commandBuilder.Append(cteCommand.Query);
				commandBuilder.Append(")");
				commands.Add(new QueryFactoryResult(commandBuilder.ToString(), cteCommand.Parameters.ToArray()));
			}

			return DbAccessLayerHelper.MergeQueryFactoryResult(true, 1, true, null, commands.ToArray());
		}

		class CteQueryPart : ISelectableQueryPart
		{
			private readonly CteInfo _cteInfo;
			private readonly IEnumerable<ColumnInfo> _columns;

			public CteQueryPart(CteInfo cteInfo)
			{
				_cteInfo = cteInfo;
				_columns = cteInfo.CteContentParts
					.OfType<ISelectableQueryPart>()
					.LastOrDefault()?
					.Columns;
			}

			public IQueryFactoryResult Process(IQueryContainer container)
			{
				return null;
			}

			public QueryIdentifier Alias
			{
				get { return _cteInfo.Name; }
			}

			public bool Distinct { get; set; }
			public int? Limit { get; set; }

			public IEnumerable<ColumnInfo> Columns
			{
				get { return _columns; }
			}
		}

		public ISelectableQueryPart AddCte(CteInfo cteInfo)
		{
			_cteInfos.Add(cteInfo);
			return new CteQueryPart(cteInfo);
		}
	}
}