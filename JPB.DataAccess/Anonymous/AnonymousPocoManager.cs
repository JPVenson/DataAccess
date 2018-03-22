#region

using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.DbInfoConfig;

#endregion

#pragma warning disable 1591

namespace JPB.DataAccess.Anonymous
{
	/// <summary>
	///     Provides the Central Access for mapping hidden Fields
	/// </summary>
	public class AnonymousPocoManager
	{
		private readonly DbConfig _config;

		static AnonymousPocoManager()
		{
			DefaultGenerator = new HashSet<IAnonymousObjectGenerator>();
			AnonymousClasses = new HashSet<AnonymousClass>();
			DefaultGenerator.Add(new AnonymousStringGenerator());
			DefaultGenerator.Add(new AnonymousNumberGenerator());
		}

		/// <summary>
		/// </summary>
		public AnonymousPocoManager(DbConfig config)
		{
			_config = config;
		}

		public static HashSet<IAnonymousObjectGenerator> DefaultGenerator { get; private set; }

		public static HashSet<AnonymousClass> AnonymousClasses { get; private set; }

		/// <summary>
		///     Converts the Original object to an Anonymous one
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		[Obsolete("")]
		public object GenerateAnonymousClass(object source)
		{
			return source;

			//var orCreateClassInfoCache = _config.GetOrCreateClassInfoCache(source.GetType());
			//var toHandleProps = orCreateClassInfoCache
			//	.Propertys
			//	.Where(f => f.Value.AnonymousObjectGenerationAttribute != null && f.Value.Setter != null && f.Value.Getter != null)
			//	.ToArray();
			//if (!toHandleProps.Any())
			//{
			//	return source;
			//}

			//var anoClass = new AnonymousClass(orCreateClassInfoCache, new WeakReference(source));
			//AnonymousClasses.Add(anoClass);

			//foreach (var result in toHandleProps)
			//{
			//	var fod =
			//		DefaultGenerator.FirstOrDefault(
			//			f => f.TargetPropType != null && f.TargetPropType == result.Value.PropertyType);
			//	if (fod == null)
			//	{
			//		throw new NotImplementedException("There is no Generator for this type " + result.Value.PropertyType);
			//	}

			//	var original = result.Value.Getter.Invoke(source);
			//	var generateAnoymousAlias = fod.GenerateAnoymousAlias(orCreateClassInfoCache,
			//		_config.GetOrCreateClassInfoCache(result.Value.PropertyType), original);
			//	result.Value.Setter.Invoke(source, generateAnoymousAlias);
			//	anoClass.Objects.Add(new AnonymousObject(generateAnoymousAlias, original, result.Key));
			//}
			//return source;
		}

		public object GenerateOriginalClass(object source)
		{
			return source;

			//var orCreateClassInfoCache = _config.GetOrCreateClassInfoCache(source.GetType());
			//var anoClass = AnonymousClasses.FirstOrDefault(f => f.Reference.IsAlive && f.Reference.Target == source);
			//if (anoClass == null)
			//{
			//	return source;
			//}

			//var toHandleProps = orCreateClassInfoCache.Propertys.Where(
			//	f =>
			//		f.Value.AnonymousObjectGenerationAttribute != null && f.Value.Setter != null &&
			//		f.Value.Getter != null).ToArray();
			//if (toHandleProps.Any())
			//{
			//	return source;
			//}

			//foreach (var result in anoClass.Objects)
			//{
			//	var fod =
			//		DefaultGenerator.FirstOrDefault(
			//			f => f.TargetPropType != null && f.TargetPropType == result.SourcePropValue.GetType());
			//	if (fod == null)
			//	{
			//		throw new NotImplementedException("There is no Generator for this type " + result.SourcePropValue.GetType());
			//	}
			//	var prop = toHandleProps.FirstOrDefault(f => f.Key == result.PropName);
			//	if (prop.Value == null)
			//	{
			//		throw new InvalidOperationException();
			//	}
			//	prop.Value.Setter.Invoke(source, result.SourcePropValue);
			//}
			//return source;
		}
	}
}