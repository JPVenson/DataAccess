namespace JPB.DataAccess.Helper
{
    public class QueryParameter : IQueryParameter
    {
        #region IQueryParameter Members

        public string Name { get; set; }
        public object Value { get; set; }

        #endregion
    }
}