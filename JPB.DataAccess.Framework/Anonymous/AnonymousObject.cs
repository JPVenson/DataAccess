#region

using System;

#endregion

#pragma warning disable 1591

namespace JPB.DataAccess.Framework.Anonymous
{
	public struct AnonymousObject : IDisposable
	{
		public AnonymousObject(object mappedPropValue, object sourcePropValue, string propName) : this()
		{
			MappedPropValue = mappedPropValue;
			SourcePropValue = sourcePropValue;
			PropName = propName;
		}

		public object MappedPropValue { get; private set; }
		public object SourcePropValue { get; private set; }
		public string PropName { get; private set; }

		public void Dispose()
		{
			MappedPropValue = null;
			SourcePropValue = null;
		}
	}
}