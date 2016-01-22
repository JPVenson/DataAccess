using System;

namespace EyeC.ProofSuite.Examination.Processor.Config.Contract
{
	public interface ISettingsValue
	{
		object Value { get; }
		string Key { get; }
		Type TypeInfo { get; }
	}
}