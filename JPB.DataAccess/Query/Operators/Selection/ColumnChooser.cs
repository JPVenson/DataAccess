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
	/// <summary>
	///
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Operators.ElementProducer{TPoco}" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IDbColumnSelector" />
	public class ColumnChooser<TPoco> : ElementProducer<TPoco>, IDbColumnSelector
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnChooser{TPoco}"/> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="columns">The columns.</param>
		/// <param name="currentIdentifier">The current identifier.</param>
		public ColumnChooser(IQueryBuilder database, List<string> columns, string currentIdentifier) : base(database, currentIdentifier)
		{
			_columns = columns;
		}

		internal readonly List<string> _columns;

		/// <summary>
		/// Selectes a column based on a Propertie
		/// </summary>
		/// <typeparam name="TA">The type of a.</typeparam>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public ColumnChooser<TPoco> Column<TA>(Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetPropertyInfoFromLabda();
			var propName = this.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			return Column(propName.DbName);
		}

		/// <summary>
		/// Selectes a column based on a name
		/// </summary>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Creates a Select statement that only query the columns that are previusly collected
		/// </summary>
		/// <returns></returns>
		public SelectQuery<TPoco> From
		{
			get
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
}
