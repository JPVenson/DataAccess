using System;
using System.Linq.Expressions;
using System.Text;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	public class CountElementsObjectSelector : QueryBuilderX
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="CountElementsObjectSelector" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="type">The type.</param>
		public CountElementsObjectSelector(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CountElementsObjectSelector" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public CountElementsObjectSelector(IQueryContainer database) : base(database)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CountElementsObjectSelector" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public CountElementsObjectSelector(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     Gets or sets a value indicating whether the Count should be Distincted.
		/// </summary>
		/// <value>
		///     <c>true</c> if [distinct mode]; otherwise, <c>false</c>.
		/// </value>
		public bool DistinctMode { get; set; }

		/// <summary>
		///     Counts all elements from a table
		/// </summary>
		/// <returns></returns>
		public CountElementsObjectSelector Distinct
		{
			get
			{
				return new CountElementsObjectSelector(this)
				       {
					       DistinctMode = true
				       };
			}
		}

		/// <summary>
		///     Counts all elements from a table
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <returns></returns>
		public ElementProducer<int> Table<TPoco>()
		{
			var sb = new StringBuilder();
			sb.Append("SELECT COUNT( ");
			if (DistinctMode)
				sb.Append("DISTINCT");
			sb.Append("1) FROM ");
			sb.Append(ContainerObject.AccessLayer.Config.GetOrCreateClassInfoCache(typeof(TPoco)).TableName);
			return new SelectQuery<int>(this.QueryText(sb.ToString()));
		}

		/// <summary>
		///     Counts all elements from a table
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <typeparam name="TA">The type of a.</typeparam>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public ElementProducer<int> Column<TPoco, TA>(Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetPropertyInfoFromLamdba();
			var propName = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			return Column<TPoco>(propName.DbName);
		}

		/// <summary>
		///     Counts all elements from a table
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public ElementProducer<int> Column<TPoco>(string columnName)
		{
			var sb = new StringBuilder();
			sb.Append("SELECT COUNT( ");
			if (DistinctMode)
				sb.Append("DISTINCT");
			sb.AppendFormat("{0}) FROM ", columnName);
			sb.Append(ContainerObject.AccessLayer.Config.GetOrCreateClassInfoCache(typeof(TPoco)).TableName);
			return new SelectQuery<int>(this.QueryText(sb.ToString()));
		}
	}
}