namespace JPB.DataAccess.Manager
{
    /// <summary>
    ///     Defines a Common set of DBTypes
    /// </summary>
    public enum DbAccessType
    {
        /// <summary>
        /// default
        /// </summary>
        Unknown = 0,
        MsSql,
        MySql,
        OleDb,
        Obdc,
        SqLite
    }
}