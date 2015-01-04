using System;
using System.Collections;
using System.Threading.Tasks;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.QueryBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private static void Main()
        {
            var dbAccessTest = new UnitTest1();
            try
            {
                dbAccessTest.UnitTest();
            }
            finally
            {
            }
        }

        public void UnitTest()
        {
            TestMethod1().Wait();
            Console.ReadKey();
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            var builder = new DbAccessLayer(DbTypes.MsSql,
                "Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;");

            var queryBuilder = builder.Query().Select<LoadXmlTest>().MsTop(10).As("xml").Where("xml.ID_test > @test").WithParamerters(new
            {
                test = 3
            }).ForResult<LoadXmlTest>().ConfigurateAwaiter();

            foreach (var result in await queryBuilder)
            {
                Console.WriteLine(result);
            }
        }
    }
}
