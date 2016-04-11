using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	[SelectFactory("SELECT name FROM sysobjects WHERE xtype='V'")]
	public class ViewInformation : TableInformations
	{
	}
}