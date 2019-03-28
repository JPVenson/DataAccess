#region

using System.Data;
using JPB.DataAccess.Contacts;

#endregion

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

		public static string TrimAlias(this string alias)
		{
			return alias.Trim('[', ']');
		}
	}
}