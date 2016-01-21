using System;
using System.Linq.Expressions;
using System.Reflection;

#if !DEBUG
using System.Diagnostics;
#endif

namespace JPB.DataAccess.Config
{
	public static class GeneralConfigHelper
	{
		/// <summary>
		///     Anonymous type check by naming convention
		/// </summary>
		/// <returns></returns>
		internal static Boolean IsAnonymousType(this Type type)
		{
			//http://stackoverflow.com/questions/1650681/determining-whether-a-type-is-an-anonymous-type
			//awesome!
			return type.Namespace == null;
			//Boolean hasCompilerGeneratedAttribute = type.GetCustomAttributes().Any(s => s is CompilerGeneratedAttribute);
			//Boolean nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
			//Boolean isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;
			//return nameContainsAnonymousType;
		}

		public static string GetPropertyInfoFromLabda<TSource, TProperty>(
			Expression<Func<TSource, TProperty>> propertyLambda)
		{
			var type = typeof(TSource);

			var member = propertyLambda.Body as MemberExpression;
			if (member == null)
				throw new ArgumentException(String.Format(
					"Expression '{0}' refers to a method, not a property.",
					propertyLambda));

			var propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
				throw new ArgumentException(String.Format(
					"Expression '{0}' refers to a field, not a property.",
					propertyLambda));

			return propInfo.Name;
		}

		public static string GetMehtodInfoFromLabda<TSource, TProperty>(
			Expression<Func<TSource, TProperty>> propertyLambda)
		{
			var type = typeof(TSource);

			var member = propertyLambda.Body as MemberExpression;
			if (member != null)
				throw new ArgumentException(String.Format(
					"Expression '{0}' refers to a property, not a method.",
					propertyLambda));

			var propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
				throw new ArgumentException(String.Format(
					"Expression '{0}' refers to a field, not a property.",
					propertyLambda));

			return propInfo.Name;
		}
	}
}