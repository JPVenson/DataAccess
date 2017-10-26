#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using JPB.DataAccess.MetaApi.Model;

#endregion

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	/// </summary>
	public class DbAttributeInfoCache : AttributeInfoCache
	{
		/// <summary>
		/// </summary>
#if !DEBUG
		[DebuggerHidden]
#endif
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbAttributeInfoCache()
		{
		}

		/// <summary>
		/// </summary>
		public DbAttributeInfoCache(Attribute attribute)
			: base(attribute)
		{
		}
	}

	/// <summary>
	///     Attributes with Database spezifc members
	/// </summary>
	/// <typeparam name="TAttr"></typeparam>
	public class DbAttributeInfoCache<TAttr> : DbAttributeInfoCache
		where TAttr : Attribute
	{
		/// <summary>
		/// </summary>
		public DbAttributeInfoCache(AttributeInfoCache firstOrDefault)
		{
			Attribute = (TAttr) firstOrDefault.Attribute;
			AttributeName = firstOrDefault.AttributeName;
		}

		/// <summary>
		///     Strongly typed Attribute
		/// </summary>
		public new TAttr Attribute
		{
			get { return base.Attribute as TAttr; }
			set { base.Attribute = value; }
		}

		/// <summary>
		///     Wraps and Attribute into an strong typed DbAttribute
		/// </summary>
		/// <param name="firstOrDefault"></param>
		/// <returns></returns>
		public static DbAttributeInfoCache<TAttr> WrapperOrNull(AttributeInfoCache firstOrDefault)
		{
			if (firstOrDefault == null)
			{
				return null;
			}
			if (typeof(TAttr) != firstOrDefault.Attribute.GetType())
			{
				throw new ArgumentException(string.Format("Wrong type supplyed expected '{0}' got '{1}'", typeof(TAttr).Name,
				firstOrDefault.Attribute.GetType().Name));
			}

			return new DbAttributeInfoCache<TAttr>(firstOrDefault);
		}

		/// <summary>
		///     Wraps the first occurence of Attribute into an strong typed DbAttribute
		/// </summary>
		/// <param name="elements"></param>
		/// <returns></returns>
		public static DbAttributeInfoCache<TAttr> WrapperOrNull(IEnumerable<AttributeInfoCache> elements)
		{
			return WrapperOrNull(elements.FirstOrDefault(f => f.Attribute.GetType() == typeof(TAttr)));
		}
	}
}