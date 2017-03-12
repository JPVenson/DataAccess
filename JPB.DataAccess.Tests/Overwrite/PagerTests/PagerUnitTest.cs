using JPB.DataAccess.Manager;
using JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests;
using NUnit.Framework;
using Users = JPB.DataAccess.Tests.Base.Users;

namespace JPB.DataAccess.Tests.PagerTests
{
    [TestFixture(DbAccessType.MsSql)]
    [TestFixture(DbAccessType.SqLite)]
    public class PagerUnitTest
    {
        [SetUp]
        public void Init()
        {
            expectWrapper = new Manager().GetWrapper(_type);
        }

        private readonly DbAccessType _type;

        public PagerUnitTest(DbAccessType type)
        {
            _type = type;
        }

        private DbAccessLayer expectWrapper;

        [Test]
        public void PagerCall()
        {
            var testUsers = DataMigrationHelper.AddUsers(250, expectWrapper);

            var refSelect =
                expectWrapper.Database.Run(
                    s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.TableName)));
            Assert.That(testUsers.Length, Is.EqualTo(refSelect));

            using (var pager = expectWrapper.Database.CreatePager<Users>())
            {
                Assert.That(pager, Is.Not.Null);

                pager.CurrentPage = 1;
                pager.PageSize = 25;

                Assert.That(pager.MaxPage, Is.EqualTo(0));

                #region CheckEvents

                var triggeredNewPageLoaded = false;
                var triggeredNewPageLoading = false;

                pager.NewPageLoaded += () => triggeredNewPageLoaded = true;
                pager.NewPageLoading += () => triggeredNewPageLoading = true;

                pager.LoadPage(expectWrapper);

                Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
                Assert.That(triggeredNewPageLoaded, Is.False);
                Assert.That(triggeredNewPageLoading, Is.False);

                pager.RaiseEvents = true;
                pager.LoadPage(expectWrapper);

                Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
                Assert.That(triggeredNewPageLoaded);
                Assert.That(triggeredNewPageLoading);

                #endregion

                #region CheckPage Size

                var oldPageSize = pager.PageSize;
                var newPageSize = 20;
                Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(oldPageSize));

                pager.PageSize = newPageSize;
                Assert.That(pager.PageSize, Is.EqualTo(newPageSize));

                pager.LoadPage(expectWrapper);
                Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
                Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(newPageSize));

                #endregion
            }
        }

        [Test]
        public void PagerConditionalCall()
        {
            var testUsers = DataMigrationHelper.AddUsers(250, expectWrapper);

            var refSelect =
                expectWrapper.Database.Run(
                    s => s.GetSkalar(string.Format("SELECT COUNT(*) FROM {0}", UsersMeta.TableName)));
            Assert.That(testUsers.Length, Is.EqualTo(refSelect));

            using (var pager = expectWrapper.Database.CreatePager<Users>())
            {
                pager.AppendedComands.Add(
                    expectWrapper.Database.CreateCommand(string.Format("WHERE User_ID = {0}", testUsers[0])));

                Assert.That(pager, Is.Not.Null);

                pager.CurrentPage = 1;
                pager.PageSize = 25;

                Assert.That(pager.MaxPage, Is.EqualTo(0));

                #region CheckEvents

                var triggeredNewPageLoaded = false;
                var triggeredNewPageLoading = false;

                pager.NewPageLoaded += () => triggeredNewPageLoaded = true;
                pager.NewPageLoading += () => triggeredNewPageLoading = true;

                pager.LoadPage(expectWrapper);

                Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
                Assert.That(triggeredNewPageLoaded, Is.False);
                Assert.That(triggeredNewPageLoading, Is.False);

                pager.RaiseEvents = true;
                pager.LoadPage(expectWrapper);

                Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
                Assert.That(triggeredNewPageLoaded);
                Assert.That(triggeredNewPageLoading);

                #endregion

                #region CheckPage Size

                var oldPageSize = pager.PageSize;
                var newPageSize = 20;
                Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(1));

                pager.PageSize = newPageSize;
                Assert.That(pager.PageSize, Is.EqualTo(newPageSize));

                pager.LoadPage(expectWrapper);
                Assert.That(pager.MaxPage, Is.Not.EqualTo(0));
                Assert.That(pager.CurrentPageItems.Count, Is.EqualTo(1));

                #endregion
            }
        }
    }
}