using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntryCreator.MsSql
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
}