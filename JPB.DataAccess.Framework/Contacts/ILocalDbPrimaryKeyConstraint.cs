#region

using System;
using System.Collections.Generic;

#endregion

namespace JPB.DataAccess.Framework.Contacts
{
	/// <summary>
	///     Contains logic for generating primary key values that are used by the LocalDbRepro
	///     Per Instance of LocalTable
	/// </summary>
	public interface ILocalDbPrimaryKeyConstraint : IEqualityComparer<object>
	{
		/// <summary>
		///     Type contract what type this generator is for
		/// </summary>
		Type GeneratingType { get; }

		/// <summary>
		///     Generate a new Uniq primary key that has the type of GeneratingType
		/// </summary>
		/// <returns></returns>
		object GetNextValue();

		/// <summary>
		///     Gets the object that indicates an Non Init primary key
		/// </summary>
		/// <returns></returns>
		object GetUninitilized();

		/// <summary>
		///     This should return a new Instance of the current ILocalPrimaryKeyValueProvider with resetted internal PK counter
		/// </summary>
		/// <returns></returns>
		ILocalDbPrimaryKeyConstraint Clone();

		/// <summary>
		///     Allows to update the index be faking the number of calles to GetNextNumber by <paramref name="index" />
		/// </summary>
		/// <param name="index"></param>
		void UpdateIndex(long index);
	}
}