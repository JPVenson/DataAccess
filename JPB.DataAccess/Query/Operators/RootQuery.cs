using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Selection;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	public class CountElementsObjectSelector : QueryBuilderX
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CountElementsObjectSelector"/> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="type">The type.</param>
		public CountElementsObjectSelector(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CountElementsObjectSelector"/> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public CountElementsObjectSelector(IQueryContainer database) : base(database)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CountElementsObjectSelector"/> class.
		/// </summary>
		/// <param name="database">The database.</param>
		public CountElementsObjectSelector(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Count should be Distincted.
		/// </summary>
		/// <value>
		///   <c>true</c> if [distinct mode]; otherwise, <c>false</c>.
		/// </value>
		public bool DistinctMode { get; set; }

		/// <summary>
		/// Counts all elements from a table
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <returns></returns>
		public ElementProducer<int> Table<TPoco>()
		{
			var sb = new StringBuilder();
			sb.Append("SELECT COUNT( ");
			if (DistinctMode)
			{
				sb.Append("DISTINCT");
			}
			sb.Append("1) FROM ");
			sb.Append(base.ContainerObject.AccessLayer.Config.GetOrCreateClassInfoCache(typeof(TPoco)).TableName);
			return new SelectQuery<int>(this.QueryText(sb.ToString()));
		}

		/// <summary>
		/// Counts all elements from a table
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <typeparam name="TA">The type of a.</typeparam>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public ElementProducer<int> Column<TPoco, TA>(Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetPropertyInfoFromLabda();
			var propName = this.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			return Column<TPoco>(propName.DbName);
		}

		/// <summary>
		/// Counts all elements from a table
		/// </summary>
		/// <typeparam name="TPoco">The type of the poco.</typeparam>
		/// <param name="columnName">Name of the column.</param>
		/// <returns></returns>
		public ElementProducer<int> Column<TPoco>(string columnName)
		{
			var sb = new StringBuilder();
			sb.Append("SELECT COUNT( ");
			if (DistinctMode)
			{
				sb.Append("DISTINCT");
			}
			sb.AppendFormat("{0}) FROM ", columnName);
			sb.Append(base.ContainerObject.AccessLayer.Config.GetOrCreateClassInfoCache(typeof(TPoco)).TableName);
			return new SelectQuery<int>(this.QueryText(sb.ToString()));
		}

		/// <summary>
		/// Counts all elements from a table
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
	}

	/// <summary>
	/// Defines the root for every Query
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.QueryBuilderX" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IRootQuery" />
	public class RootQuery : QueryBuilderX, IRootQuery
	{
		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> Execute<T>(params object[] argumentsForFactory)
		{
			var cmd = ContainerObject
				.AccessLayer
				.CreateSelectQueryFactory(
					this.ContainerObject.AccessLayer.GetClassInfo(typeof(T)), argumentsForFactory);
			return new SelectQuery<T>(this.QueryCommand(cmd));
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public DatabaseObjectSelector Select
		{
			get
			{
				return new DatabaseObjectSelector(this);
			}
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public CountElementsObjectSelector Count
		{
			get
			{
				return new CountElementsObjectSelector(this);
			}
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> SelectFactory<T>(params object[] argumentsForFactory)
		{
			return new DatabaseObjectSelector(this).Table<T>(argumentsForFactory);
		}

		/// <summary>
		///     Adds a Select - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <returns></returns>
		public SelectQuery<T> Distinct<T>()
		{
			var cmd = DbAccessLayer.CreateSelect(this.ContainerObject.AccessLayer.GetClassInfo(typeof(T)), "DISTINCT");
			return new SelectQuery<T>(this.QueryText(cmd));
		}

		/// <summary>
		///     Adds a Update - Statement
		///     Uses reflection or a Factory mehtod to create
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public UpdateQuery<T> Update<T>(T obj)
		{
			return new UpdateQuery<T>(this
			.QueryCommand(
				DbAccessLayer
				.CreateUpdate(this
				.ContainerObject
				.AccessLayer.Database, this.ContainerObject.AccessLayer.GetClassInfo(typeof(T)), obj)));
		}
		/// <summary>
		/// For Internal Usage only
		/// </summary>
		public RootQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}
		/// <summary>
		/// For Internal Usage only
		/// </summary>
		public RootQuery(IQueryContainer database) : base(database)
		{
		}
		/// <summary>
		/// For Internal Usage only
		/// </summary>
		public RootQuery(IQueryBuilder database) : base(database)
		{
		}
		/// <summary>
		/// For Internal Usage only
		/// </summary>
		public RootQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}
		/// <summary>
		/// For Internal Usage only
		/// </summary>
		public RootQuery(DbAccessLayer database) : base(database)
		{
		}
	}
}
