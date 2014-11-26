namespace JPB.DataAccess.Helper
{
    public class QueryParameter : IQueryParameter
    {
        public QueryParameter()
        {
            
        }

        public QueryParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        #region IQueryParameter Members

        /// <summary>
        /// Name with @ or without it
        /// if the system detects a name without @ it will add it
        /// </summary>
        public string Name { get; set; }
        public object Value { get; set; }

        #endregion

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}