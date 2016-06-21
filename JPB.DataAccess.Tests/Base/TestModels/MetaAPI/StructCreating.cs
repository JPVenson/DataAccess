namespace JPB.DataAccess.Tests.Base.TestModels.MetaAPI
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
		public ClassCreating()
		{
		}
	}

	public class ClassCreatingWithArguments
	{
		public ClassCreatingWithArguments(string propString)
		{
			PropString = propString;
		}

		public string PropString { get; set; }
	}

	public class ClassSpeedMeasurement
	{
		public ClassSpeedMeasurement()
		{
		}

		public string PropString { get; set; }
	}
}