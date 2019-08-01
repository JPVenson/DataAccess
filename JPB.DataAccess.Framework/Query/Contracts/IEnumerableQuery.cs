using System.Collections.Generic;

namespace JPB.DataAccess.Framework.Query.Contracts
{
	/// <summary>
	///     This Part of the Query can be executed and returns a set of Pocos
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IEnumerableQuery<out T> : IEnumerable<T>
	{
	}
}