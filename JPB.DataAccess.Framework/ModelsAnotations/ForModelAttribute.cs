using System;

namespace JPB.DataAccess.Framework.ModelsAnotations
{
	/// <summary>
	///     Allows renaming of the local class name to any name and the mapping from that name to the Db Table name
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
	public class ForModelAttribute : DataAccessAttribute
	{
		/// <summary>
		///     Creates a new Instance of ForModelAttribute
		/// </summary>
		/// <param name="alternatingName" />
		public ForModelAttribute(string alternatingName)
		{
			AlternatingName = alternatingName;
		}

		/// <summary>
		/// </summary>
		public string AlternatingName { get; private set; }
	}
}