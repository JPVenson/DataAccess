using System.Collections.Generic;

namespace JPB.DataAccess.Query.Contracts
{
	public interface IQueryElement : IQueryBuilder
	{

	}

	public interface IElementProducer<out T> : IIdentifyerElementQuery
	{
	}

	public interface IJoinQuery : IQueryElement
	{

	}

	public interface IIdentifyerElementQuery : IQueryElement
	{

	}

	public interface IConditionalQuery<out T> : IElementProducer<T>
	{

	}

	public interface IConditionalColumnQuery<out T> : IQueryElement
	{

	}

	public interface IConditionalEvalQuery<out T> : IQueryElement
	{

	}

	public interface ISelectQuery<out T> : IElementProducer<T>
	{

	}

	public interface IUpdateQuery<out T> : IElementProducer<T>
	{

	}

	public interface IRootQuery : IQueryElement
	{

	}

	public interface IInCteQuery : IQueryElement, IIdentifyerElementQuery
	{

	}

	public interface INestedRoot : IRootQuery, IIdentifyerElementQuery
	{

	}
}