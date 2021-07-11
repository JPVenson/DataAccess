using System;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntityCreator.Core.Poco.SqLite
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