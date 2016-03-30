using System.Collections.Generic;

namespace JPB.DataAccess.DbInfoConfig
{
	/// <summary>
	/// </summary>
	public class FactoryHelperSettings
	{
		private static readonly string[] _defaultNamespaces;

		static FactoryHelperSettings()
		{
			_defaultNamespaces = new[]
			{
				"System",
				"System.Collections.Generic",
				"System.CodeDom.Compiler",
				"System.Linq",
				"System.Data",
				"JPB.DataAccess.ModelsAnotations",
				"JPB.DataAccess.AdoWrapper"
			};
		}

		public FactoryHelperSettings()
		{
			HideSuperCreation = true;
		}

		/// <summary>
		///     Check and throw exception if not all propertys can be accessed by the Super class
		/// </summary>
		public bool EnforcePublicPropertys { get; set; }

		/// <summary>
		///     If any error is thrown so throw exception
		/// </summary>
		public bool EnforceCreation { get; set; }

		/// <summary>
		///     Shame on me.
		///     To set all propertys from the outside ill create a super class that inherts from the POCO .
		///     to get rid of this super class you can set this property to true then the superclass will be cased into its
		///     baseclass.
		///     If set to true the factory will cast the object to its base class and hide the super creation
		/// </summary>
		public bool HideSuperCreation { get; set; }

		/// <summary>
		///     Include PDB debug infos. Deactivate this during tests beacuse it can cause problems.
		/// </summary>
		public bool CreateDebugCode { get; set; }

		/// <summary>
		///     When a Factory is create inside an dll the factory can be reused. Deactivate this during tests beacuse it can cause
		///     problems.
		/// </summary>
		public bool ReuseFactorys { get; set; }

		/// <summary>
		///     A Collection that includes all Namespaces that are used by default to create new Factorys
		/// </summary>
		public IEnumerable<string> DefaultNamespaces
		{
			get { return _defaultNamespaces; }
		}
	}
}