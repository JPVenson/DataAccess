using System;

namespace JPB.DataAccess.Manager
{
	/// <summary>
	///     Defines a Common set of DBTypes
	/// </summary>
	[Flags]
	public enum DbAccessType
	{
		/// <summary>
		///     default and undefined behavior
		/// </summary>
		Unknown = 0,

		/// <summary>
		///     For Developing
		///     Not intended for your use!
		/// </summary>
		Experimental = 1 << 1,

		/// <summary>
		///     Defines the MsSQL Type as a Target database
		/// </summary>
		MsSql = 1 << 2,

		/// <summary>
		///     Defines the MySQL Type as a Target database
		///     Not as tested as the MsSQL type
		/// </summary>
		MySql = 1 << 3,

		/// <summary>
		///     Defines the MsSQL Type as a Target database
		///     Not tested!
		/// </summary>
		OleDb = 1 << 4,

		/// <summary>
		///     Defines the MsSQL Type as a Target database
		///     Not Tested!
		/// </summary>
		Obdc = 1 << 5,

		/// <summary>
		///     Defines the MsSQL Type as a Target database
		///     Not Tested
		/// </summary>
		SqLite = 1 << 6,

		/// <summary>
		///		Defines the Remoting Target. Should be used combined with the target database on the server.
		/// </summary>
		Remoting = 1 << 7
	}

	public static class DbAccessTypeExtensions
	{
		public static bool HasFlagFast(this DbAccessType value, DbAccessType flag)
		{
			return (value & flag) != 0;
		}
	}
}