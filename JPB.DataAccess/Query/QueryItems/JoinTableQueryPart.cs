using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class JoinTableQueryPart : ISelectableQueryPart
	{
		public string ParentColumn { get; }
		public string ChildColumn { get; }

		public JoinTableQueryPart(
			QueryIdentifier parentOfJoin,
			QueryIdentifier childOfJoin,
			QueryIdentifier joinAlias,
			Type targetType,
			string parentColumn,
			string childColumn) 
			: this(parentOfJoin, childOfJoin, joinAlias, targetType, parentColumn, childColumn, new string[0])
		{
		}

		public JoinTableQueryPart(
			QueryIdentifier parentOfJoin,
			QueryIdentifier childOfJoin,
			QueryIdentifier joinAlias,
			Type targetType,
			string parentColumn,
			string childColumn,
			IEnumerable<string> columns)
		{
			Alias = joinAlias;
			ParentColumn = parentColumn;
			ChildColumn = childColumn;
			ParentOfJoin = parentOfJoin;
			ChildOfJoin = childOfJoin;
			Columns = new List<ColumnInfo>(columns.Select(e => new ColumnInfo(e, Alias)));
			Type = targetType;
		}

		public QueryIdentifier ParentOfJoin { get; private set; }
		public QueryIdentifier ChildOfJoin { get; private set; }
		public IEnumerable<ColumnInfo> Columns { get; private set; }

		public IDbCommand Process(IQueryContainer container)
		{
			container.PostProcessors.Add(new RelationProcessor(this));

			var joinBuilder = new StringBuilder();
			joinBuilder.Append($"JOIN [{ParentOfJoin.GetAlias()}] AS [{Alias.GetAlias()}]" +
			                   $"ON [{ParentOfJoin.GetAlias()}].[{ParentColumn}]" +
			                   $" = " +
			                   $"[{ChildOfJoin.GetAlias()}].[{ChildColumn}]");

			return container.AccessLayer.Database.CreateCommand(joinBuilder.ToString());
		}

		public QueryIdentifier Alias { get; }
		public bool Distinct { get; set; }
		public int? Limit { get; set; }
		public Type Type { get; set; }
	}
}