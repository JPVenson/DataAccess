using System.Collections.Generic;
using System.Data;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntityCreator.MsSql
{
    [SelectFactory("SELECT name FROM sysobjects WHERE xtype='U'")]
    public class TableInformations
    {
        [ForModel("name")]
        public string TableName { get; set; }
    }

    [SelectFactory("SELECT name FROM sysobjects WHERE xtype='V'")]
    public class ViewInformation : TableInformations
    {
    }

    [SelectFactory("SELECT sp.name, xmlText FROM sys.procedures sp OUTER APPLY( SELECT DISTINCT p.name AS Parameter, t.name AS [Type] FROM sys.parameters p JOIN sys.types t ON p.system_type_id = t.system_type_id WHERE p.object_id = sp.object_id AND t.name NOT LIKE '%sysname%' FOR XML PATH('Param'),ROOT('ArrayOfParam'), TYPE ) AS XMLT(xmlText) LEFT JOIN sys.extended_properties E ON sp.object_id = E.major_id WHERE E.class IS NULL ORDER BY sp.name ")]
    public class StoredProcedureInformation : TableInformations
    {
        public StoredProcedureInformation()
        {
            
        }

        [FromXml("xmlText")]
        public IEnumerable<SpParam> ParamaterSpParams { get; set; }
    }

    public class SpParam
    {
        public string Parameter { get; set; }
        public SqlDbType Type { get; set; }
    }
}