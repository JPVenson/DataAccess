using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JPB.DataAccess.DbInfoConfig.DbInfo;

namespace JPB.DataAccess.Framework.Anonymous
{
	/// <summary>
	///		Can be used to mask strings
	/// </summary>
	public class AnonymousStringGenerator : IAnonymousObjectGenerator
	{
		/// <inheritdoc />
		public bool OneWayGeneration
		{
			get { return true; }
		}

		/// <inheritdoc />
		public Type TargetPropType
		{
			get { return typeof(string); }
		}

		/// <inheritdoc />
		public object GenerateAnoymousAlias(DbClassInfoCache targetClass, DbClassInfoCache targetPropType, object targetValue)
		{
			if (targetValue == null)
			{
				return null;
			}

			return
					MD5.Create()
					   .ComputeHash(Encoding.UTF8.GetBytes(targetValue as string))
					   .Select(f => f.ToString("X2"))
					   .Aggregate((e, f) => e + f);
		}

		/// <inheritdoc />
		public bool Equals(IAnonymousObjectGenerator other)
		{
			return other != null && TargetPropType == other.TargetPropType;
		}
	}
}