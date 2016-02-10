using System;

namespace JPB.DataAccess.Helper.LocalDb
{
	internal struct ReproMappings : IEquatable<ReproMappings>
	{
		private Type _sourceType;
		private Type _targetType;

		public ReproMappings(Type targetType, Type sourceType)
		{
			_sourceType = sourceType;
			_targetType = targetType;
		}

		public Type SourceType
		{
			get { return _sourceType; }
		}

		public Type TargetType
		{
			get { return _targetType; }
		}

		public bool Equals(ReproMappings other)
		{
			return SourceType == other.SourceType && TargetType == other.TargetType;
		}
	}
}