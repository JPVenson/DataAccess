using System.Data;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
    public class FooTest
    {
        public FooTest()
        {
            var access = new DbAccessLayer(DbAccessType.MsSql,
                "Data Source=(localdb)\\Projects;Initial Catalog=Northwind;Integrated Security=True;");
            var select = access.Select<Foo>("SomeName");
        }

        public class Foo
        {
            [ObjectFactoryMethod]
            public Foo(IDataRecord record)
            {
                Id_Foo = (long) record["Id_Foo"];
                FooName = (string) record["FooName"];
            }

            public long Id_Foo { get; set; }
            public string FooName { get; set; }
        }
    }
}