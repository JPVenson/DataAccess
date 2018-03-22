#region

using System;
using System.Linq;
using System.Security.Cryptography;
using JPB.DataAccess.DbInfoConfig.DbInfo;

#endregion

#pragma warning disable 1591

namespace JPB.DataAccess.Anonymous
{
	public class AnonymousNumberGenerator : IAnonymousObjectGenerator
	{
		public bool OneWayGeneration
		{
			get { return true; }
		}

		public Type TargetPropType
		{
			get { return typeof(int); }
		}

		public object GenerateAnoymousAlias(DbClassInfoCache targetClass, DbClassInfoCache targetPropType, object targetValue)
		{
			if (targetValue == null)
			{
				return null;
			}

			return
				MD5.Create()
					.ComputeHash(BitConverter.GetBytes((int) targetValue))
					.Select(f => f.ToString("X2"))
					.Aggregate((e, f) => e + f);
		}

		public bool Equals(IAnonymousObjectGenerator other)
		{
			return other != null && TargetPropType == other.TargetPropType;
		}
	}
}