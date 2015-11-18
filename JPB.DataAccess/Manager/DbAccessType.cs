namespace JPB.DataAccess.Manager
{
    /// <summary>
    ///     Defines a Common set of DBTypes
    /// </summary>
    public enum DbAccessType
    {
        /// <summary>
        /// For Developing
        /// Not itend for your use!
        /// </summary>
        Experimental = -1,
        /// <summary>
        /// default
        /// </summary>
        Unknown = 0,
        MsSql,
        MySql,
        OleDb,
        Obdc,
        SqLite,
    }
}