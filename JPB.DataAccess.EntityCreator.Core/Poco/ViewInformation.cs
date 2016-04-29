using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntityCreator.Core.Poco
{
	[SelectFactory("SELECT name FROM sysobjects WHERE xtype='V'")]
	public class ViewInformation : TableInformations
	{
	}
}