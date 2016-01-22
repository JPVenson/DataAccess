using System;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.Config.Model;
using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	internal class DbPropertyInfoCache<T, TE> : DbPropertyInfoCache
	{
		internal DbPropertyInfoCache(string name, Action<T, TE> setter = null, Func<T, TE> getter = null,
			params AttributeInfoCache[] attributes)
		{
			if (attributes == null)
				throw new ArgumentNullException("attributes");

			PropertyName = name;

			if (setter != null)
			{
				Setter = new MethodInfoCache(setter);
			}

			if (getter != null)
			{
				Getter = new MethodInfoCache(getter);
			}
		}
	}

	/// <summary>
	///     Infos about the Property
	/// </summary>
	public class DbPropertyInfoCache : PropertyInfoCache
	{
		public DbPropertyInfoCache()
		{
		}

		/// <summary>
		/// </summary>
		internal DbPropertyInfoCache(PropertyInfo propertyInfo, bool anon)
			: base(propertyInfo, anon)
		{
			if (propertyInfo != null)
			{
				Refresh();
			}
		}

		/// <summary>
		///     if known the ForModelAttribute attribute
		/// </summary>
		public DbAttributeInfoCache<ForModelAttribute> ForModelAttribute { get; protected internal set; }

		/// <summary>
		///     if known the ForXml attribute
		/// </summary>
		public DbAttributeInfoCache<FromXmlAttribute> FromXmlAttribute { get; protected internal set; }

		/// <summary>
		///     Should this property not be inserterd
		/// </summary>
		public bool InsertIgnore { get; protected internal set; }

		/// <summary>
		///     if known the ForginKey attribute
		/// </summary>
		public DbAttributeInfoCache<ForeignKeyAttribute> ForginKeyAttribute { get; protected internal set; }

		/// <summary>
		///     Returns the For Model name if known or the Propertyname
		/// </summary>
		public string DbName
		{
			get
			{
				if (ForModelAttribute != null)
					return ForModelAttribute.Attribute.AlternatingName;
				return PropertyName;
			}
		}

		/// <summary>
		///		if known the RowVersion Attribute
		/// </summary>
		public DbAttributeInfoCache<RowVersionAttribute> RowVersionAttribute { get; private set; }

		/// <summary>
		///		if knwon the PrimaryKey Attribute
		/// </summary>
		public DbAttributeInfoCache<PrimaryKeyAttribute> PrimaryKeyAttribute { get; private set; }

		/// <summary>
		///     For internal Usage only
		/// </summary>
		public void Refresh()
		{
			PrimaryKeyAttribute = DbAttributeInfoCache<PrimaryKeyAttribute>.WrapperOrNull(AttributeInfoCaches.FirstOrDefault(f => f.Attribute is PrimaryKeyAttribute));
			InsertIgnore = AttributeInfoCaches.Any(f => f.Attribute is InsertIgnoreAttribute);
			ForginKeyAttribute =
				PropertyInfo.GetGetMethod().IsVirtual
				? DbAttributeInfoCache<ForeignKeyAttribute>.WrapperOrNull(AttributeInfoCaches.FirstOrDefault(f => f.Attribute is ForeignKeyAttribute))
				: null;
			RowVersionAttribute =
				DbAttributeInfoCache<ModelsAnotations.RowVersionAttribute>.WrapperOrNull(
					AttributeInfoCaches.FirstOrDefault(s => s.Attribute is RowVersionAttribute));
			FromXmlAttribute = DbAttributeInfoCache<FromXmlAttribute>.WrapperOrNull(AttributeInfoCaches.FirstOrDefault(f => f.Attribute is FromXmlAttribute));
			ForModelAttribute = DbAttributeInfoCache<ForModelAttribute>.WrapperOrNull(AttributeInfoCaches.FirstOrDefault(f => f.Attribute is ForModelAttribute));
		}

		//internal static PropertyInfoCache Logical(string info)
		//{
		//    return new PropertyInfoCache(null)
		//    {
		//        PropertyName = info
		//    };
		//}
	}
}