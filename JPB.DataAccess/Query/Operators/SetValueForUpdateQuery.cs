using System;
using System.Linq;
using System.Linq.Expressions;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;

namespace JPB.DataAccess.Query.Operators
{
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

		public UpdateValueQuery<TPoco> Column(string columnName)
		{
			return new UpdateValueQuery<TPoco>(this.QueryQ(columnName));
		}

		public UpdateValueQuery<TPoco> Column<TA>(Expression<Func<TPoco, TA>> columnName)
		{
			return Column(ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[columnName.GetPropertyInfoFromLamdba()].DbName);
		}

		public NextUpdateOrCondtionQuery<TPoco> ColumnTo(string columnName, object value)
		{
			var arg = "@setArg" + base.ContainerObject.GetNextParameterId();
			return new NextUpdateOrCondtionQuery<TPoco>(this.QueryQ(columnName + " = " + arg, new QueryParameter(arg, value)));
		}

		public NextUpdateOrCondtionQuery<TPoco> ColumnTo<TA>(Expression<Func<TPoco, TA>> columnName, object value)
		{
			return ColumnTo(
			ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[columnName.GetPropertyInfoFromLamdba()].DbName,
			value);
		}
	}

	public class UpdateValueQuery<TPoco> : QueryBuilderX
	{
		/// <inheritdoc />
		public UpdateValueQuery(DbAccessLayer database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public UpdateValueQuery(IQueryContainer database) : base(database)
		{
		}

		/// <inheritdoc />
		public UpdateValueQuery(IQueryBuilder database) : base(database)
		{
		}

		/// <inheritdoc />
		public UpdateValueQuery(IQueryBuilder database, Type type) : base(database, type)
		{
		}

		/// <inheritdoc />
		public UpdateValueQuery(DbAccessLayer database) : base(database)
		{
		}

		/// <summary>
		/// Declares the value to set the given column
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public NextUpdateOrCondtionQuery<TPoco> Value(object value)
		{
			var arg = "@setArg" + base.ContainerObject.GetNextParameterId();
			return new NextUpdateOrCondtionQuery<TPoco>(this.QueryQ(" = " + arg, new QueryParameter(arg, value)));
		}

		/// <summary>
		/// Sets the Column defined to the result of the query
		/// </summary>
		/// <param name="value"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public NextUpdateOrCondtionQuery<TPoco> QueryValue(string value, params IQueryParameter[] arguments)
		{
			return new NextUpdateOrCondtionQuery<TPoco>(this.QueryQ(" = " + value, arguments.ToArray()));
		}
	}
}