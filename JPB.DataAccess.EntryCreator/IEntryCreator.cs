namespace JPB.DataAccess.EntryCreator
{
    public interface IEntryCreator
    {
        void CreateEntrys(string connection, string outputPath, string database);
    }
}