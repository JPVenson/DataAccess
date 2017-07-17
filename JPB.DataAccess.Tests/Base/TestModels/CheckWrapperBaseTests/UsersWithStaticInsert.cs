#region

using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	public class UsersWithStaticInsert
	{
		public long User_ID { get; set; }
		public string UserName { get; set; }

		[InsertFactoryMethod(TargetDatabase = DbAccessType.MsSql)]
		[InsertFactoryMethod(TargetDatabase = DbAccessType.MySql)]
		public string Insert()
		{
			return string.Format("INSERT INTO {0}({2}) VALUES ('{1}')", UsersMeta.TableName, UserName,
				UsersMeta.ContentName);
		}

		[InsertFactoryMethod(TargetDatabase = DbAccessType.SqLite)]
		public string InsertSqLite()
		{
			return string.Format("INSERT INTO {0} VALUES (0, '{1}')", UsersMeta.TableName, UserName);
		}
	}
}