#region

using System.Collections.Concurrent;
using System.Collections.Generic;

#endregion

namespace JPB.DataAccess.DbInfoConfig
{
	/// <summary>
	///     How to handle existing created Poco.Dlls
	/// </summary>
	public enum CollisonDetectionMode
	{
		/// <summary>
		///     No detection. Will may cause File access problems in Multithreaded Environments
		/// </summary>
		Non,

		/// <summary>
		///     Checks for Existing Dlls and tries to load them. If this failes an exception will be thrown
		/// </summary>
		Optimistic,

		/// <summary>
		///     Does not checks for existing dlls. Will allways create a new DLL
		/// </summary>
		Pessimistic
	}

	/// <summary>
	/// </summary>
	public class FactoryHelperSettings
	{
		/// <summary>
		///     The default namespaces
		/// </summary>
		private static readonly string[] _defaultNamespaces;

		private static FactoryHelperSettings _defaultSettings = new FactoryHelperSettings();

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

		/// <summary>
		///     Initializes a new instance of the <see cref="FactoryHelperSettings" /> class.
		/// </summary>
		public FactoryHelperSettings()
		{
			HideSuperCreation = true;
			TempFileData = new ConcurrentBag<string>();
		}

		/// <summary>
		///     Gets the Default settings that are applyed to the Factory Creation
		/// </summary>
		public static FactoryHelperSettings DefaultSettings
		{
			get { return _defaultSettings; }
			set { _defaultSettings = value; }
		}


		internal ConcurrentBag<string> TempFileData { get; set; }

		/// <summary>
		///     Checks for precreated poco Elements
		/// </summary>
		/// <value>
		///     The file collison detection.
		/// </value>
		public CollisonDetectionMode FileCollisonDetection { get; set; }

		/// <summary>
		///     Check and throw exception if not all propertys can be accessed by the Super class
		/// </summary>
		/// <value>
		///     <c>true</c> if [enforce public propertys]; otherwise, <c>false</c>.
		/// </value>
		public bool EnforcePublicPropertys { get; set; }

		/// <summary>
		///     If any error is thrown so throw exception
		/// </summary>
		/// <value>
		///     <c>true</c> if [enforce creation]; otherwise, <c>false</c>.
		/// </value>
		public bool EnforceCreation { get; set; }

		/// <summary>
		///		When set the check for DbNull will be skipped
		/// </summary>
		public bool AssertDataNotDbNull { get; set; }

		/// <summary>
		///     Shame on me.
		///     To set all propertys from the outside ill create a super class that inherts from the POCO .
		///     to get rid of this super class you can set this property to true then the superclass will be cased into its
		///     baseclass.
		///     If set to true the factory will cast the object to its base class and hide the super creation
		/// </summary>
		/// <value>
		///     <c>true</c> if [hide super creation]; otherwise, <c>false</c>.
		/// </value>
		public bool HideSuperCreation { get; set; }

		/// <summary>
		///     Include PDB debug infos. Deactivate this during tests beacuse it can cause problems.
		/// </summary>
		/// <value>
		///     <c>true</c> if [create debug code]; otherwise, <c>false</c>.
		/// </value>
		public bool CreateDebugCode { get; set; }

		/// <summary>
		///     A Collection that includes all Namespaces that are used by default to create new Factorys
		/// </summary>
		/// <value>
		///     The default namespaces.
		/// </value>
		public IEnumerable<string> DefaultNamespaces
		{
			get { return _defaultNamespaces; }
		}

		/// <summary>
		///     Makes a Deep copy of the current Settings
		/// </summary>
		/// <returns></returns>
		public FactoryHelperSettings Copy()
		{
			var copy = (FactoryHelperSettings) MemberwiseClone();
			copy.TempFileData = new ConcurrentBag<string>();
			return copy;
		}
	}
}