using System;
using System.ComponentModel;
using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.ModelsAnotations
{
	/// <summary>
	///     Marks a Property as XML Serilized
	///     If marked the output field from the query will be Serlized to the given object
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class FromXmlAttribute : ForModelAttribute
	{
		private static ILoadFromXmlStrategy _loadFromXmlStrategyInstance;
		private Type _loadFromXmlStrategy;

		/// <summary>
		/// </summary>
		public FromXmlAttribute(string fieldName)
				: base(fieldName)
		{
			FieldName = fieldName;
		}

		/// <summary>
		///     The name of the Field inside the result stream
		/// </summary>
		public string FieldName { get; set; }

		/// <summary>
		///     Specifiys the Strategy that is used to load the Property
		/// </summary>
		[DefaultValue("IncludeInSelect")]
		public LoadStrategy LoadStrategy { get; set; }

		/// <summary>
		///     if set the type will be used to define a user logic for the Serialization process
		/// </summary>
		public Type LoadFromXmlStrategy
		{
			get { return _loadFromXmlStrategy; }
			set
			{
				if (!typeof(ILoadFromXmlStrategy).IsAssignableFrom(value))
				{
					throw new ArgumentException("Not able to assgin value from IloadFromXMLStrategy");
				}
				_loadFromXmlStrategy = value;
			}
		}

		internal ILoadFromXmlStrategy CreateLoader()
		{
			return _loadFromXmlStrategyInstance ??
			       (_loadFromXmlStrategyInstance = (ILoadFromXmlStrategy) Activator.CreateInstance(_loadFromXmlStrategy));
		}
	}
}