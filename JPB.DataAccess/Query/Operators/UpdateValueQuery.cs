using System;
using System.Linq;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	///		Defines mehtods for setting a Column
	/// </summary>
	/// <typeparam name="TPoco"></typeparam>
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