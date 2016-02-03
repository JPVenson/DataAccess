using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.DataAccess.UnitTests.TestModels.XmlDataRecordTest
{
	public class InstanceMock
	{
		public InstanceMock()
		{
			MockPropA = "NAN";
		}

		public string MockPropA { get; set; }
		public int MockPropB { get; set; }
	}
}
