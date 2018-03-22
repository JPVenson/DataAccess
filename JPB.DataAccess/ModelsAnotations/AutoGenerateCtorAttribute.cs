using System;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Marks this class to be allowed by the Framework for the CodeDOM Ado.net ctor creation
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class AutoGenerateCtorAttribute : DataAccessAttribute
	{
		/// <summary>
		///     Creates a new Instance without any Meta Infos
		/// </summary>
		public AutoGenerateCtorAttribute()
		{
			CtorGeneratorMode = CtorGeneratorMode.Inhert;
		}

		/// <summary>
		///     Tells the framework how a factory for this class should be created
		/// </summary>
		public CtorGeneratorMode CtorGeneratorMode { get; set; }

		/// <summary>
		///     If set to true all Assemblys that are used inside the base Assembly will be imported to the new one
		/// </summary>
		public bool FullSateliteImport { get; set; }
	}
}