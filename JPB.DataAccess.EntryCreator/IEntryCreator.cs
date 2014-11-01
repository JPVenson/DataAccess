namespace JPB.DataAccess.EntityCreator
{
    public interface IEntryCreator
    {
        void CreateEntrys(string connection, string outputPath, string database);
    }
}