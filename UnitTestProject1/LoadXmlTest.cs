using System.Collections;
using System.Collections.Generic;
using JPB.DataAccess.ModelsAnotations;

namespace UnitTestProject1
{

    static class Commands
    {
        public const string SelectCommand = "EXEC LoadTree";
    }

    [SelectFactory(Commands.SelectCommand)]
    public class LoadXmlTest
    {
        [ForModel("ID_test")]
        public long IdTest { get; set; }

        public string PropA { get; set; }
        public string PropB { get; set; }

        [FromXml("xmlText2")]
        public IEnumerable<LoadXmlTester> Self { get; set; }
    }

    public class LoadXmlTester
    {
        [ForModel("ID_test")]
        public long IdTest { get; set; }

        public string PropA { get; set; }
        public string PropB { get; set; }

        [FromXml("xmlText")]
        public IEnumerable<LoadXmlTest> Self { get; set; }
    }
}