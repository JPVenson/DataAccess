using System;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntityCreator.DatabaseStructure.SqLite.Models
{
	[SelectFactory(@"SELECT 
	name
	FROM 
	sqlite_master 
	WHERE 
	type ='view' AND 
	name NOT LIKE 'sqlite_%';")]
	[Serializable]
	public class ViewInformation : TableInformations
	{
	}
}