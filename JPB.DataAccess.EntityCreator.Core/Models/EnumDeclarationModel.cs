using System;
using System.Collections.Generic;
using JPB.DataAccess.EntityCreator.Core.Contracts;

namespace JPB.DataAccess.EntityCreator.MsSql
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