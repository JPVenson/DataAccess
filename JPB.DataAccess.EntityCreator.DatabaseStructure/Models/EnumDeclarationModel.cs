using System;
using System.Collections.Generic;
using JPB.DataAccess.EntityCreator.DatabaseStructure.Contracts;

namespace JPB.DataAccess.EntityCreator.Core.Models
{
	[Serializable]
	public class EnumDeclarationModel : IEnumDeclarationModel
	{
		public EnumDeclarationModel()
		{
			Values = new Dictionary<int, string>();
		}
		public Dictionary<int, string> Values { get; private set; }
		public string Name { get; set; }
	}
}