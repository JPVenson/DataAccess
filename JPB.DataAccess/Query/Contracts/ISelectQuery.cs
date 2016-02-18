namespace JPB.DataAccess.Query.Contracts
{
	public interface IQueryElement
	{

	}

	public interface IElementProducer : IQueryElement, IIdentifyerElementQuery
	{
		
	}

	public interface IJoinQuery : IQueryElement
	{

	}

	public interface IIdentifyerElementQuery : IQueryElement
	{

	}

	public interface IConditionalQuery : IQueryElement, IIdentifyerElementQuery
	{

	}

	public interface ISelectQuery : IQueryElement, IIdentifyerElementQuery, IElementProducer
	{

	}

	public interface IUpdateQuery : IQueryElement, IElementProducer
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