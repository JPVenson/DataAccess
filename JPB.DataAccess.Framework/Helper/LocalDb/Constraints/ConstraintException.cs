#region

using System;

#endregion

namespace JPB.DataAccess.Helper.LocalDb.Constraints
{
	/// <summary>
	/// </summary>
	/// <seealso cref="System.Exception" />
	public class ConstraintException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ConstraintException" /> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ConstraintException(string message) : base(message)
		{
		}
	}
}