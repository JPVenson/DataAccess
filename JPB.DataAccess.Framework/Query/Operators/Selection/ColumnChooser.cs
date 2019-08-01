//#region

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using JPB.DataAccess.Manager;
//using JPB.DataAccess.MetaApi;
//using JPB.DataAccess.Query.Contracts;
//using JPB.DataAccess.Query.Operators.Conditional;
//using JPB.DataAccess.Query.Operators.Orders;

//#endregion

//namespace JPB.DataAccess.Query.Operators.Selection
//{
//	/// <summary>
//	/// </summary>
//	/// <typeparam name="TPoco">The type of the poco.</typeparam>
//	/// <seealso cref="JPB.DataAccess.Query.Operators.ElementProducer{TPoco}" />
//	/// <seealso cref="JPB.DataAccess.Query.Contracts.IDbColumnSelector" />
//	public class ColumnChooser<TPoco> : ElementProducer<TPoco>, IDbColumnSelector
//	{
//		/// <summary>
//		///     Initializes a new instance of the <see cref="ColumnChooser{TPoco}" /> class.
//		/// </summary>
//		/// <param name="database">The database.</param>
//		/// <param name="columns">The columns.</param>
//		/// <param name="currentIdentifier">The current identifier.</param>
//		public ColumnChooser(IQueryBuilder database, List<string> columns, string currentIdentifier)
//			: base(database, currentIdentifier)
//		{
//			_columns = columns;
//		}

//		/// <inheritdoc cref="QueryBuilderX" />
//		public override IEnumerator<TPoco1> GetEnumerator<TPoco1>(bool async)
//		{
//			return CreateQuery().GetEnumerator<TPoco1>(async);
//		}

//		/// <inheritdoc cref="QueryBuilderX" />
//		public override IEnumerable<E> ForResult<E>(bool async = true)
//		{
//			return CreateQuery().ForResult<E>(async);
//		}

//		/// <inheritdoc />
//		public override ConditionalQuery<TPoco> Where
//		{
//			get { return CreateQuery().Where; }
//		}

//		/// <inheritdoc />
//		public override OrderStatementQuery<TPoco> Order
//		{
//			get { return CreateQuery().Order; }
//		}

//		/// <inheritdoc />
//		public override ElementProducer<TPoco> LimitBy(int limit)
//		{
//			return CreateQuery().LimitBy(limit);
//		}

//		private SelectQuery<TPoco> CreateQuery()
//		{
//			string selectQuery;
//			if (_columns.Any())
//			{
//				selectQuery = DbAccessLayer.CreateSelectByColumns(Cache, _columns.Aggregate((e, f) => e + ", " + f));
//			}
//			else
//			{
//				selectQuery = DbAccessLayer.CreateSelect(Cache);
//			}
//			if (!string.IsNullOrWhiteSpace(CurrentIdentifier))
//			{
//				selectQuery = selectQuery + " AS " + CurrentIdentifier;
//			}
//			return new SelectQuery<TPoco>(this).QueryText(selectQuery);
//		}

//		/// <summary>
//		///		Selects the current PrimaryKey
//		/// </summary>
//		/// <returns></returns>
//		public virtual ColumnChooser<TPoco> PrimaryKey()
//		{
//			var tCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
//			return Column(tCache.PrimaryKeyProperty.DbName);
//		}

//		/// <summary>
//		///		Selects the ForginKey to the table.
//		/// </summary>
//		/// <exception cref="InvalidOperationException">If there are 0 or more then 1 forginKeys</exception>
//		/// <returns></returns>
//		public virtual ColumnChooser<TPoco> ForginKey<TFkPoco>()
//		{
//			var tCache = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco));
//			var tProp = tCache.Propertys.Values
//			                  .Single(e =>
//				                  e.ForginKeyDeclarationAttribute != null &&
//				                  e.ForginKeyDeclarationAttribute.Attribute.ForeignType == typeof(TFkPoco));
//			return Column(tProp.DbName);
//		}

//		/// <summary>
//		///     Selectes a column based on a Propertie
//		/// </summary>
//		/// <typeparam name="TA">The type of a.</typeparam>
//		/// <param name="columnName">Name of the column.</param>
//		/// <returns></returns>
//		public virtual ColumnChooser<TPoco> Column<TA>(Expression<Func<TPoco, TA>> columnName)
//		{
//			var member = columnName.GetPropertyInfoFromLamdba();
//			var propName = ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
//			return Column(propName.DbName);
//		}

//		///// <summary>
//		/////     Selectes a column based on a name
//		///// </summary>
//		///// <param name="columnName">Name of the column.</param>
//		///// <returns></returns>
//		//public virtual ColumnChooser<TPoco> Column(string columnName)
//		//{
//		//	if (CurrentIdentifier != null)
//		//	{
//		//		_columns.Add(CurrentIdentifier + "." + columnName);
//		//	}
//		//	else
//		//	{
//		//		_columns.Add(columnName);
//		//	}
//		//	return new ColumnChooser<TPoco>(this, _columns, CurrentIdentifier);
//		//}
//	}
//}