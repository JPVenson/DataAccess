using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.Helper
{
	public class ForginKeyConstraintException : Exception
	{
		public ForginKeyConstraintException(string changedTableName, string constraintTableName, object constraintValue)
			: base(string.Format("The attempt to change the value '{0}' on the table '{1}' would violate an constraint on table '{2}'", changedTableName, constraintTableName, constraintValue))
		{

		}
	}
}
