using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.QueryItems.Conditional;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.Query.QueryItems
{
	internal class UpdateTableWithQueryPart : IIdentifiableQueryPart
	{
		private readonly QueryIdentifier _target;

		public List<ColumnAssignment> ColumnAssignments { get; set; }

		internal class ColumnAssignment
		{
			public ColumnAssignment()
			{
				QueryParameters = new List<IQueryParameter>();
			}

			public string Column { get; set; }
			public string Value { get; set; }
			public List<IQueryParameter> QueryParameters { get; set; }
		}

		public UpdateTableWithQueryPart(QueryIdentifier target, 
			QueryIdentifier queryIdentifier)
		{
			_target = target;
			Alias = queryIdentifier;
			ColumnAssignments = new List<ColumnAssignment>();
		}

		public IQueryFactoryResult Process(IQueryContainer container)
		{
			var query = new StringBuilder();

			switch (container.AccessLayer.DbAccessType)
			{
				case DbAccessType.MsSql:
				case DbAccessType.MySql:
					query.Append($"UPDATE [{Alias.GetAlias()}] SET ");
					query
						.Append(
							ColumnAssignments
								.Select(
									columnAssignment =>
										$"{Alias.GetAlias()}.[{columnAssignment.Column}] = {columnAssignment.Value}")
								.Aggregate((e, f) => e + ", " + f)
						);
					query.Append($" FROM {_target.GetAlias()} AS {Alias.GetAlias()}");
					break;
				case DbAccessType.Experimental:
				case DbAccessType.Unknown:
				case DbAccessType.OleDb:
				case DbAccessType.Obdc:
				case DbAccessType.SqLite:
					query.Append($"UPDATE {_target.GetAlias()} SET ");
					query
						.Append(
							ColumnAssignments
								.Select(
									columnAssignment =>
										$"[{columnAssignment.Column}] = {columnAssignment.Value}")
								.Aggregate((e, f) => e + ", " + f)
						);
					query.Append($"");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return new QueryFactoryResult(query.ToString(),
				ColumnAssignments.SelectMany(f => f.QueryParameters).ToArray());
		}

		public QueryIdentifier Alias { get; }
	}
}