using System.Collections.Generic;

namespace JPB.DataAccess.Query.Contracts
{
	public interface IQueryBuilder<Stack>
		where Stack : IQueryElement
	{
		//IQueryContainer<Stack> Add(GenericQueryPart part);
		//IQueryContainer<T> Add<T>(GenericQueryPart part) where T : IQueryElement;

		//IQueryContainer<T> ChangeType<T>() where T : IQueryElement;

		IQueryContainer ContainerObject { get; }

		IEnumerable<E> ForResult<E>();

		IQueryBuilder<T> ChangeType<T>() where T : IQueryElement;
	}
}