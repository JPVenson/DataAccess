using System;

namespace JPB.DataAccess.Helper.LocalDb.Constraints
{
	internal class ConstraintException : Exception
	{
		public ConstraintException(string message) : base(message)
		{

		}
	}
}