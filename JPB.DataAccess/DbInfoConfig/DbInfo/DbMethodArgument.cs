/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using JPB.DataAccess.MetaApi.Model;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	/// </summary>
	public class DbMethodArgument : MethodArgsInfoCache<DbAttributeInfoCache>
	{
		/// <summary>
		///     The class that owns this Property
		/// </summary>
		public DbMethodInfoCache DeclaringMethod { get; protected internal set; }
	}
}