using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators.Selection
{
	public class ColumnChooser<TPoco> : ElementProducer<TPoco>, IDbColumnSelector
	{
		public ColumnChooser(IQueryBuilder database, List<string> columns, string currentIdentifier) : base(database, currentIdentifier)
		{
			_columns = columns;
		}

		private readonly List<string> _columns;

		public ColumnChooser<TPoco> Column<TA>(Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetPropertyInfoFromLabda();
			var propName = this.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			return Column(propName.DbName);
		}

		public ColumnChooser<TPoco> Column(string columnName)
		{
			if (CurrentIdentifier != null)
			{
				_columns.Add(CurrentIdentifier + "." + columnName);
			}
			else
			{
				_columns.Add(columnName);
			}
			return new ColumnChooser<TPoco>(this, _columns, CurrentIdentifier);
		}

		public SelectQuery<TPoco> From()
		{
			string selectQuery;
			if (_columns.Any())
			{
				selectQuery = DbAccessLayer.CreateSelectByColumns(base.Cache, _columns.Aggregate((e, f) => e + ", " + f));
			}
			else
			{
				selectQuery = DbAccessLayer.CreateSelect(base.Cache);
			}
			return new SelectQuery<TPoco>(this.QueryText(selectQuery));
		}
	}
}
