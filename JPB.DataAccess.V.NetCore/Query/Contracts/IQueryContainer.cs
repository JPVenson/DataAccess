using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	///
	/// </summary>
	public interface IQueryContainer
	{
		/// <summary>
		///		Declares the Return type that is awaited
		/// </summary>
		Type ForType { get; set; }

		/// <summary>
		///		Gets the current number of used SQL Arguments
		/// </summary>
		int AutoParameterCounter { get; }

		/// <summary>
		///		Defines all elements added by the Add Method
		/// </summary>
		List<GenericQueryPart> Parts { get; }

		/// <summary>
		///     Defines the Way how the Data will be loaded
		/// </summary>
		EnumerationMode EnumerationMode { get; set; }

		/// <summary>
		///     If enabled Variables that are only used for parameters will be Renamed if there Existing multiple times
		/// </summary>
		bool AllowParamterRenaming { get; set; }

		/// <summary>
		///		Access to the underlying AccessLayer
		/// </summary>
		DbAccessLayer AccessLayer { get; }

		/// <summary>
		///     Will concat all QueryParts into a statement and will check for Spaces
		/// </summary>
		/// <returns></returns>
		IDbCommand Compile();
		/// <summary>
		///     Increment the counter +1 and return the value
		/// </summary>
		/// <returns></returns>
		int GetNextParameterId();

		/// <summary>
		///     Compiles the QueryCommand into a String|IEnumerable of Paramameter
		/// </summary>
		/// <returns></returns>
		Tuple<string, IEnumerable<IQueryParameter>> CompileFlat();

		/// <summary>
		/// Clones this Container
		/// </summary>
		/// <returns></returns>
		IQueryContainer Clone();
	}
}