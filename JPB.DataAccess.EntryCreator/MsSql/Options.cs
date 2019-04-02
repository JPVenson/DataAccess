/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/

using System;

namespace JPB.DataAccess.EntityCreator.Core.Models
{
	[Serializable]
	public class Options
	{
		public string Version { get; set; }
		public string[] Actions { get; set; }
	}
}

