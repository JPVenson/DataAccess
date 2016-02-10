using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.UnitTests.TestModels.MetaAPI
{
	public struct StructCreating
	{
		private string _propString;

		public StructCreating(string propString)
		{
			_propString = propString;
		}

		public string PropString
		{
			get { return _propString; }
			private set { _propString = value; }
		}
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
