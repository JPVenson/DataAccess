namespace EyeC.ProofSuite.Examination.Processor.Config.Contract
{
	public interface ISettingsProvider
	{
		ISettingsValue GetValue(string key);
	}
}