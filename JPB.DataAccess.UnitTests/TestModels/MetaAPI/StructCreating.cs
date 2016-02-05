using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.UnitTests.TestModels.MetaAPI
{
	public struct StructCreating
	{
		public StructCreating(string propString)
		{
			PropString = propString;
		}

		public string PropString { get; private set; }
	}

	public class ClassCreating
	{
		public ClassCreating(string propString)
		{
			PropString = propString;
		}

		public string PropString { get; private set; }
	}
}
