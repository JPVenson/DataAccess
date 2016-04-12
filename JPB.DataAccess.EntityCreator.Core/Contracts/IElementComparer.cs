using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace JPB.DataAccess.EntityCreator.Core.Contracts
{
	public interface IElementComparer<T>
	{
		IEnumerable<string> Compare(T other);
	}
}