namespace JPB.DataAccess.Contacts
{
    /// <summary>
    /// Used to load a Xml based Entry from a xml text
    /// </summary>
    public interface ILoadFromXmlStrategy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        object LoadFromXml(string xml);
    }
}