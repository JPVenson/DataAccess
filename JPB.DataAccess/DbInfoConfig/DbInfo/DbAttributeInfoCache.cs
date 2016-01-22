using System;
using JPB.DataAccess.Config.Model;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	/// <summary>
	/// </summary>
	public class DbAttributeInfoCache : AttributeInfoCache
	{
		public DbAttributeInfoCache()
		{
		}

		public DbAttributeInfoCache(Attribute attribute)
			: base(attribute)
		{
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TAttr"></typeparam>
	public class DbAttributeInfoCache<TAttr> : DbAttributeInfoCache
			where TAttr : Attribute
	{
		public DbAttributeInfoCache(AttributeInfoCache firstOrDefault)
		{
			this.Attribute = (TAttr)firstOrDefault.Attribute;
			this.AttributeName = firstOrDefault.AttributeName;
		}

		/// <summary>
		/// Strongly typed Attribute
		/// </summary>
		public new TAttr Attribute
		{
			get
			{
				return _attribute as TAttr;
			}
			set
			{
				_attribute = value;
			}
		}

		public static DbAttributeInfoCache<TAttr> WrapperOrNull(AttributeInfoCache firstOrDefault)
		{
			if (firstOrDefault == null)
				return null;
			if (typeof(TAttr) != firstOrDefault.Attribute.GetType())
				throw new ArgumentException(string.Format("Wrong type supplyed expected {0}", typeof(TAttr).Name));

			return new DbAttributeInfoCache<TAttr>(firstOrDefault as AttributeInfoCache);
		}
	}
}