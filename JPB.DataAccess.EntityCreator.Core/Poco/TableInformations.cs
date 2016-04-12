/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;
using JPB.DataAccess.EntityCreator.Core.Contracts;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.EntityCreator.Core.Poco
{
	[Serializable]
	[SelectFactory("SELECT name FROM sysobjects WHERE xtype='U'")]
	public class TableInformations : ITableInformations
	{
		[ForModel("name")]
		public string TableName { get; set; }
	}
}