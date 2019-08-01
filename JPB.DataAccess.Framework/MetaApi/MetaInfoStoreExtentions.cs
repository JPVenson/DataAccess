#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#if !DEBUG
using System.Diagnostics;
#endif

#endregion

namespace JPB.DataAccess.Framework.MetaApi
{
	/// <summary>
	///     Extention methods for easy access to meta infos
	/// </summary>
	public static class MetaInfoStoreExtentions
	{
		internal static MethodInfo _expressionLambdaInfo;

		internal static MethodInfo GetCompileMethodFromExpression(Type createLambda)
		{
			return createLambda
				.GetMethods()
				.Where(e => e.Name == nameof(Expression<object>.Compile))
				.Where(e => e.DeclaringType != typeof(LambdaExpression))
				.Single(e => e.GetParameters().Length == 0);
		}

		internal static MethodInfo GetExpressionLambda()
		{
			return _expressionLambdaInfo ?? (_expressionLambdaInfo = typeof(Expression)
				       .GetMethods()
				       .Where(e => e.Name == nameof(Expression.Lambda) && e.ContainsGenericParameters)
				       .Single(e =>
				       {
					       var genericArguments = e.GetParameters();
					       return genericArguments.Length == 2
					              && genericArguments[0].ParameterType == typeof(Expression)
					              && genericArguments[1].ParameterType == typeof(IEnumerable<ParameterExpression>);
				       }));
		}

		/// <summary>
		///     Anonymous type check by naming convention
		/// </summary>
		/// <returns></returns>
		internal static bool IsAnonymousType(this Type type)
		{
			//http://stackoverflow.com/questions/1650681/determining-whether-a-type-is-an-anonymous-type
			return type.Namespace == null;
		}

		/// <summary>
		///     Gets the property information from labda.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <typeparam name="TProperty">The type of the property.</typeparam>
		/// <param name="propertyLambda">The property lambda.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		/// </exception>
		public static string GetPropertyInfoFromLamdba<TSource, TProperty>(
			this Expression<Func<TSource, TProperty>> propertyLambda)
		{
			var type = typeof(TSource);

			var member = propertyLambda.Body as MemberExpression;
			if (member == null)
			{
				throw new ArgumentException(string.Format(
				"Expression '{0}' refers to a method, not a property.",
				propertyLambda));
			}

			var propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
			{
				throw new ArgumentException(string.Format(
				"Expression '{0}' refers to a field, not a property.",
				propertyLambda));
			}

			return propInfo.Name;
		}

		//public static DbPropertyInfoCache GetFullPropertyInfoFromLabda<TSource, TProperty>(
		//	this Expression<Func<TSource, TProperty>> propertyLambda)
		//{

		//	var member = propertyLambda.Body as MemberExpression;
		//	if (member == null)
		//		throw new ArgumentException(String.Format(
		//			"Expression '{0}' refers to a method, not a property.",
		//			propertyLambda));

		//	var propInfo = member.Member as PropertyInfo;
		//	if (propInfo == null)
		//		throw new ArgumentException(String.Format(
		//			"Expression '{0}' refers to a field, not a property.",
		//			propertyLambda));

		//	var type = typeof(TSource).GetClassInfo();

		//	return type.Propertys[propInfo.Name];
		//}

		/// <summary>
		///     Gets the mehtod information from labda.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <typeparam name="TProperty">The type of the property.</typeparam>
		/// <param name="propertyLambda">The property lambda.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		/// </exception>
		public static string GetMehtodInfoFromLabda<TSource, TProperty>(
			this Expression<Func<TSource, TProperty>> propertyLambda)
		{
			var type = typeof(TSource);

			var member = propertyLambda.Body as MemberExpression;
			if (member != null)
			{
				throw new ArgumentException(string.Format(
				"Expression '{0}' refers to a property, not a method.",
				propertyLambda));
			}

			var propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
			{
				throw new ArgumentException(string.Format(
				"Expression '{0}' refers to a field, not a property.",
				propertyLambda));
			}

			return propInfo.Name;
		}
	}
}