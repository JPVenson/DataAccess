using System;

namespace JPB.DataAccess.Helper.LocalDb.Constraints
{
	public class ConstraintException : Exception
	{
		public ConstraintException(string message) : base(message)
		{

		}
	}
}