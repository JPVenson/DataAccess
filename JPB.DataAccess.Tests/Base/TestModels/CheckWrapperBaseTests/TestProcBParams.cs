using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	[ForModel("TestProcB")]
	public class TestProcBParams
	{
		[ForModel("number")]
		public int Number { get; set; }
	}
}