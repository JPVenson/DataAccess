/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Collections.Generic;

namespace JPB.DataAccess.Pager.Contracts
{
	/// <summary>
	///     A wrapper interface to convert all incomming items from Load method into new type
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TE"></typeparam>
	public interface IWrapperDataPager<T, TE> : IDataPager<T>
	{
		/// <summary>
		///     Function to convert all items from T to TE
		/// </summary>
		Func<T, TE> Converter { get; set; }

		/// <summary>
		///     new Collection of TE
		/// </summary>
		new ICollection<TE> CurrentPageItems { get; }
	}
}