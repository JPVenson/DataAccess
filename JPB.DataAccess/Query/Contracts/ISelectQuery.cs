namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryBuilder" />
	public interface IQueryElement : IQueryBuilder
	{
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IIdentifyerElementQuery" />
	public interface IElementProducer<out T> : IIdentifyerElementQuery
	{
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryElement" />
	public interface IOrderdElementProducer<out T> : IQueryElement
	{
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryElement" />
	public interface IOrderdColumnElementProducer<out T> : IQueryElement
	{
	}

	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryElement" />
	public interface IJoinQuery : IQueryElement
	{
	}

	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryElement" />
	public interface IIdentifyerElementQuery : IQueryElement
	{
		/// <summary>
		///     Gets the current identifier.
		/// </summary>
		/// <value>
		///     The current identifier.
		/// </value>
		string CurrentIdentifier { get; }
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IElementProducer{T}" />
	public interface IConditionalQuery<out T> : IElementProducer<T>
	{
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryElement" />
	public interface IConditionalColumnQuery<out T> : IQueryElement
	{
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryElement" />
	public interface IConditionalOperatorQuery<out T> : IQueryElement
	{
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryElement" />
	public interface IConditionalEvalQuery<out T> : IQueryElement
	{
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IElementProducer{T}" />
	public interface ISelectQuery<out T> : IElementProducer<T>
	{
	}

	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IRootQuery" />
	public interface IDbElementSelector : IRootQuery
	{
	}

	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IRootQuery" />
	public interface IDbColumnSelector : IRootQuery
	{
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IElementProducer{T}" />
	public interface IUpdateQuery<out T> : IElementProducer<T>
	{
	}

	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryElement" />
	public interface IRootQuery : IQueryElement
	{
	}

	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryElement" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IIdentifyerElementQuery" />
	public interface IInCteQuery : IQueryElement, IIdentifyerElementQuery
	{
	}

	/// <summary>
	/// </summary>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IRootQuery" />
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IIdentifyerElementQuery" />
	public interface INestedRoot : IRootQuery, IIdentifyerElementQuery
	{
	}
}