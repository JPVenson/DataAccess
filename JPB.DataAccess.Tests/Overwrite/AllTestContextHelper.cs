using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace JPB.DataAccess.Tests
{
    public static class AllTestContextHelper
    {
        public static void TearDown(this object that, IManager mgr)
        {
            var state = Equals(TestContext.CurrentContext.Result.Outcome, ResultState.Failure); // TestState enum

            if (state)
            {
                mgr.FlushErrorData();
            }
        }

        public static void Clear(this object that, DbAccessLayer dbAccess)
        {
            dbAccess.Config.Dispose();
            dbAccess.ExecuteGenericCommand(string.Format("DELETE FROM {0} ", UsersMeta.TableName), null);
            if (dbAccess.DbAccessType == DbAccessType.MsSql)
            {
                dbAccess.ExecuteGenericCommand(string.Format("TRUNCATE TABLE {0} ", UsersMeta.TableName), null);
            }
        }
    }
}
