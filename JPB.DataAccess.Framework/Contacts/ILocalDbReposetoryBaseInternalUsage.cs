namespace JPB.DataAccess.Framework.Contacts
{
	internal interface ILocalDbReposetoryBaseInternalUsage : ILocalDbReposetoryBase
	{
		new bool ReposetoryCreated { get; set; }
		bool IsMigrating { get; set; }
	}
}