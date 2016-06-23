using System;
using System.Linq.Expressions;
using JPB.DataAccess.MetaApi;
using JPB.DataAccess.Query.Contracts;

namespace JPB.DataAccess.Query.Operators.Conditional
{
	public class ConditionalQuery<TPoco> : QueryBuilderX, IConditionalQuery<TPoco>
	{
		public readonly CondtionBuilderState State;

		public ConditionalQuery(IQueryBuilder queryText, CondtionBuilderState state) : base(queryText)
		{
			State = state;
		}

		/// <summary>
		/// Opens a new Logical combined Query
		/// </summary>
		/// <returns></returns>
		public ConditionalQuery<TPoco> Parenthesis()
		{
			return new ConditionalQuery<TPoco>(this.QueryText("("), State.ToInBreaket(true));
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> Column(string columnName)
		{
			return new ConditionalColumnQuery<TPoco>(this.QueryText(columnName), State);
		}

		/// <summary>
		/// Prepaires an Conditional Query that targets an single Column
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public ConditionalColumnQuery<TPoco> Column<TA>(
								Expression<Func<TPoco, TA>> columnName)
		{
			var member = columnName.GetPropertyInfoFromLabda();
			var propName = this.ContainerObject.AccessLayer.GetClassInfo(typeof(TPoco)).Propertys[member];
			return Column(propName.DbName);
		}
	}
}
