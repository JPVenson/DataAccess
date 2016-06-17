using JPB.DataAccess.Contacts;

namespace JPB.DataAccess.Helper.LocalDb.Constraints.Collections
{
	public class ConstraintCollection
	{
		private readonly LocalDbReposetoryBase _localDbReposetoryBase;

		public ConstraintCollection(LocalDbReposetoryBase localDbReposetoryBase, ILocalDbPrimaryKeyConstraint primaryKey)
		{
			_localDbReposetoryBase = localDbReposetoryBase;
			PrimaryKey = primaryKey;
			Unique = new UniqueConstrains(_localDbReposetoryBase);
			Default = new DefaultConstraints(_localDbReposetoryBase);
			Check = new CheckConstraints(_localDbReposetoryBase);
		}

		public UniqueConstrains Unique { get; private set; }
		public DefaultConstraints Default { get; private set; }
		public CheckConstraints Check { get; private set; }
		public ILocalDbPrimaryKeyConstraint PrimaryKey { get; private set; }
	}
}