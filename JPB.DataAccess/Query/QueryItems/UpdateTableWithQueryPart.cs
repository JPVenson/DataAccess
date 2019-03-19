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

namespace JPB.DataAccess.Query.QueryItems
{
	internal class UpdateTableWithQueryPart : IIdentifiableQueryPart
	{
		private readonly DbClassInfoCache _classInfo;
		private readonly object _withObject;

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

		public UpdateTableWithQueryPart(DbClassInfoCache classInfo, QueryIdentifier queryIdentifier, object withObject = null)
		{
			_classInfo = classInfo;
			Alias = queryIdentifier;
			_withObject = withObject;
			ColumnAssignments = new List<ColumnAssignment>();
		}

		public IDbCommand Process(IQueryContainer container)
		{
			if (_withObject != null)
			{
				return DbAccessLayer
					.CreateUpdate(container
						.AccessLayer.Database, _classInfo, _withObject);
			}

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
										$"[{Alias.GetAlias()}].[{columnAssignment.Column}] = {columnAssignment.Value}")
								.Aggregate((e, f) => e + ", " + f)
						);
					query.Append($" FROM [{_classInfo.TableName}] AS [{Alias.GetAlias()}]");
					break;
				case DbAccessType.Experimental:
				case DbAccessType.Unknown:
				case DbAccessType.OleDb:
				case DbAccessType.Obdc:
				case DbAccessType.SqLite:
					query.Append($"UPDATE [{_classInfo.TableName}] SET ");
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

			

			return container.AccessLayer.Database.CreateCommandWithParameterValues(query.ToString(),
				ColumnAssignments.SelectMany(e => e.QueryParameters));
		}

		public QueryIdentifier Alias { get; }
	}
}