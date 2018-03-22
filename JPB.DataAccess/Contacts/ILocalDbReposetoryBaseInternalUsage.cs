namespace JPB.DataAccess.Contacts
{
	internal interface ILocalDbReposetoryBaseInternalUsage : ILocalDbReposetoryBase
	{
		new bool ReposetoryCreated { get; set; }
		bool IsMigrating { get; set; }
	}
}