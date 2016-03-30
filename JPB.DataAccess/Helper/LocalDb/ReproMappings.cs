using System;

namespace JPB.DataAccess.Helper.LocalDb
{
	internal struct ReproMappings : IEquatable<ReproMappings>
	{
		public ReproMappings(Type targetType, Type sourceType)
		{
			SourceType = sourceType;
			TargetType = targetType;
		}

		public Type SourceType { get; }

		public Type TargetType { get; }

		public bool Equals(ReproMappings other)
		{
			return SourceType == other.SourceType && TargetType == other.TargetType;
		}
	}
}