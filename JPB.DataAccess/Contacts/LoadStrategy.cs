/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
namespace JPB.DataAccess.Contacts
{
	/// <summary>
	/// </summary>
	public enum LoadStrategy
	{
		/// <summary>
		///     Tells the API to include the field name into a Requested Select
		/// </summary>
		IncludeInSelect,

		/// <summary>
		///     Tells the API that the field should be loaded Implizit
		///     If you do select the field with your own statement the xml will not be parsed
		/// </summary>
		NotIncludeInSelect
	}
}