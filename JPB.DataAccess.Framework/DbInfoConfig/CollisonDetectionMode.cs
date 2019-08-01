namespace JPB.DataAccess.Framework.DbInfoConfig
{
	/// <summary>
	///     How to handle existing created Poco.Dlls
	/// </summary>
	public enum CollisonDetectionMode
	{
		/// <summary>
		///     No detection. Will may cause File access problems in Multithreaded Environments
		/// </summary>
		Non,

		/// <summary>
		///     Checks for Existing Dlls and tries to load them. If this failes an exception will be thrown
		/// </summary>
		Optimistic,

		/// <summary>
		///     Does not checks for existing dlls. Will allways create a new DLL
		/// </summary>
		Pessimistic
	}
}