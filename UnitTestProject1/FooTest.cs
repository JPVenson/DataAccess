using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.Helper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace UnitTestProject1
{
public class FooTest
{
    public class Foo
    {
        [ObjectFactoryMethod]
        public Foo(IDataRecord record)
        {
            Id_Foo = (long)record["Id_Foo"];
            FooName = (string)record["FooName"];
        }

        public long Id_Foo { get; set; }
        public string FooName { get; set; }
    }

    public FooTest()
    {
        var access = new DbAccessLayer(DbAccessType.MsSql, "Data Source=(localdb)\\Projects;Initial Catalog=Northwind;Integrated Security=True;");
        var @select = access.Select<Foo>("SomeName");
    }
}
}
