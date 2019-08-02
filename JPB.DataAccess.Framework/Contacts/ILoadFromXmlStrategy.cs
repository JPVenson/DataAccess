namespace JPB.DataAccess.Contacts
{
	/// <summary>
	///     Used to load a Xml based Entry from a xml text
	/// </summary>
	public interface ILoadFromXmlStrategy
	{
		/// <summary>
		/// </summary>
		/// <returns></returns>
		object LoadFromXml(string xml);
	}
}