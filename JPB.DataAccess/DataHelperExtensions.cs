/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System.Data;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess
{
	internal static class DataHelperExtensions
	{
		internal static void AddWithValue(this IDataParameterCollection source, string name, object parameter,
			IDatabase db)
		{
			source.Add(db.CreateParameter(name, parameter));
		}

		public static string CheckParamter(this string paramName)
		{
			return !paramName.StartsWith("@")
				? "@" + paramName
				: paramName;
		}
	}
}