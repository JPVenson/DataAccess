/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
namespace JPB.DataAccess.Query
{
	/// <summary>
	///     Enum for specifying the way in enumeration that is used by enumerating a IQueryContainer
	/// </summary>
	public enum EnumerationMode
	{
		/// <summary>
		///     At the first call of GetEnumerator all items will be enumerated and stored
		///     Eager loading
		/// </summary>
		FullOnLoad,

		/// <summary>
		///     Will bypass the current Complete loading logic and forces the DbAccessLayer to use a
		///     Lazy loading
		/// </summary>
		OnCall
	}
}