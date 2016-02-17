using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.TestModels.CheckWrapperBaseTests
{
	[ForModel("TestProcA")]
	public class TestProcAParams
	{
	}

	[ForModel("TestProcB")]
	public class TestProcBParams
	{
		[ForModel("number")]
		public int Number { get; set; }
	}
}