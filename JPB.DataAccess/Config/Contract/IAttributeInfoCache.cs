using System;
using JPB.DataAccess.Config.Model;

namespace JPB.DataAccess.Config.Contract
{
	public interface IAttributeInfoCache
	{
		Attribute Attribute { get; }
		object AttributeName { get; }
		IAttributeInfoCache Init(Attribute attribute);
		int CompareTo(AttributeInfoCache other);
	}
}