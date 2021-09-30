using System;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure.MsSql.Models
{
	[SelectFactory("SELECT name FROM sysobjects WHERE xtype='V'")]
	[Serializable]
	public class ViewInformation : TableInformations
	{
	}
}