namespace JPB.DataAccess.Helper
{
    public interface IQueryParameter
    {
        string Name { get; set; }
        object Value { get; set; }
    }
}