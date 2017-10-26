#region

using System;
using System.Linq.Expressions;
using System.Reflection;
#if !DEBUG
using System.Diagnostics;
#endif

#endregion

namespace JPB.DataAccess.MetaApi
{
	/// <summary>
	///     Extention methods for easy access to meta infos
	/// </summary>
	public static class MetaInfoStoreExtentions
	{
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