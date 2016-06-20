using System;
using System.Collections;
using JPB.DataAccess.DbInfoConfig.DbInfo;

namespace JPB.DataAccess.Helper.LocalDb
{
	internal interface ILocalDbReposetoryBaseInternalUsage : ILocalDbReposetoryBase
	{
		new bool ReposetoryCreated { get; set; }
		bool IsMigrating { get; set; }
	}
	
	public interface ILocalDbReposetoryBase : ICollection
	{
		LocalDbManager Database { get; }
		bool IsReadOnly { get; }
		bool ReposetoryCreated { get; }
		DbClassInfoCache TypeInfo { get; }

		void Add(object item);
		void Clear();
		bool Contains(object item);
		bool Contains(long item);
		bool Contains(int item);
		bool ContainsId(object fkValueForTableX);
		bool Remove(object item);
		bool Update(object item);
	}
}