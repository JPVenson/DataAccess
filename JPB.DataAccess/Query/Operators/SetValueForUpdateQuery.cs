using System;
using System.Linq.Expressions;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///		An update related Column selection
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
	public class SetValueForUpdateQuery<TPoco> : QueryBuilderX
	{
		/// <inheritdoc />
		public SetValueForUpdateQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public SetValueForUpdateQuery(IQueryContainer database) : base(database)
		{
		}

		/// <inheritdoc />
		public SetValueForUpdateQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <inheritdoc />
		public SetValueForUpdateQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public SetValueForUpdateQuery(DbAccessLayer database) : base(database)
		{
		}


		/// <summary>
		///		Adds the Column name
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public UpdateValueQuery<TPoco> Column(string columnName)
		{
			return new UpdateValueQuery<TPoco>(this.QueryQ(columnName));
		}
		/// <summary>
		///		Adds the Column name
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public UpdateValueQuery<TPoco> Column<TA>(Expression<Func<TPoco, TA>> columnName)
		{
			return Column(ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[columnName.GetPropertyInfoFromLamdba()].DbName);
		}
		/// <summary>
		///
		/// </summary>
		/// <param name="columnName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		[Obsolete("Use Column().Is")]
		public NextUpdateOrCondtionQuery<TPoco> ColumnTo(string columnName, object value)
		{
			var arg = "@setArg" + base.ContainerObject.GetNextParameterId();
			return new NextUpdateOrCondtionQuery<TPoco>(this.QueryQ(columnName + " = " + arg, new QueryParameter(arg, value)));
		}
		/// <summary>
		///
		/// </summary>
		/// <param name="columnName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		[Obsolete("Use Column().Is")]
		public NextUpdateOrCondtionQuery<TPoco> ColumnTo<TA>(Expression<Func<TPoco, TA>> columnName, object value)
		{
			return ColumnTo(
			ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[columnName.GetPropertyInfoFromLamdba()].DbName,
			value);
		}
	}
}