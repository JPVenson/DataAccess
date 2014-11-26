namespace JPB.DataAccess.QueryBuilder
{
    public enum EnumerationMode
    {
        /// <summary>
        /// At the first call of GetEnumerator all items will be enumerated and stored
        /// Eager loading
        /// </summary>
        FullOnLoad,
        /// <summary>
        /// Will bypass the current Complete loading logic and forces the DbAccessLayer to use a 
        /// Lazy loading
        /// </summary>
        OnCall
    }
}