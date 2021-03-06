﻿#region

using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators.Conditional;
using JPB.DataAccess.Query.QueryItems.Conditional;

#endregion

namespace JPB.DataAccess.Query.Operators
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TPoco">The type of the poco.</typeparam>
	/// <seealso cref="QueryBuilderX" />
	/// <seealso cref="IElementProducer{T}" />
	/// <seealso cref="System.Collections.Generic.IEnumerable{TPoco}" />
	public class ElementProducer<TPoco> : ElementResultQuery<TPoco>
	{
		/// <inheritdoc />
		public ElementProducer(IQueryBuilder database) : base(database)
		{
		}

		/// <summary>
		///     Adds a SQL WHERE statement
		///     does not emit any conditional statement
		///     should be followed by Column()
		/// </summary>
		/// <returns></returns>
		public virtual ConditionalQuery<TPoco> Where
		{
			get
			{
				return new ConditionalQuery<TPoco>(Add(new ConditionStatementQueryPart()));
			}
		}
	}
}