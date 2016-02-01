using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.MetaApi.Model;
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
				Setter = new MethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(setter);
			}

			if (getter != null)
			{
				Getter = new MethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(getter);
			}
		}
	}

	/// <summary>
	///     Infos about the Property
	/// </summary>
	public class DbPropertyInfoCache : PropertyInfoCache<DbAttributeInfoCache>
	{
		[DebuggerHidden]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbPropertyInfoCache()
		{
			base.AttributeInfoCaches = new HashSet<DbAttributeInfoCache>();
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

		//internal DbPropertyInfoCache(string dbName, string propertyName, Type propertyType)
		//{
		//	this.PropertyName = propertyName;
		//	if (dbName != propertyName)
		//	{
		//		this.AttributeInfoCaches.Add(new DbAttributeInfoCache<ForModelAttribute>(new AttributeInfoCache(new ForModelAttribute(dbName))));
		//	}
		//}

		/// <summary>
		///		The class that owns this Property
		/// </summary>
		public DbClassInfoCache DeclaringClass { get; protected internal set; }

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
			PrimaryKeyAttribute = DbAttributeInfoCache<PrimaryKeyAttribute>.WrapperOrNull(AttributeInfoCaches.FirstOrDefault(f => f.Attribute.GetType() == typeof(PrimaryKeyAttribute)));
			InsertIgnore = AttributeInfoCaches.Any(f => f.Attribute is InsertIgnoreAttribute);
			if (PropertyInfo != null)
				ForginKeyAttribute =
					PropertyInfo.GetGetMethod().IsVirtual
					? DbAttributeInfoCache<ForeignKeyAttribute>.WrapperOrNull(AttributeInfoCaches.FirstOrDefault(f => f.Attribute.GetType() == typeof(ForeignKeyAttribute)))
					: null;
			RowVersionAttribute =
				DbAttributeInfoCache<RowVersionAttribute>.WrapperOrNull(
					AttributeInfoCaches.FirstOrDefault(s => s.Attribute is RowVersionAttribute));
			FromXmlAttribute = DbAttributeInfoCache<FromXmlAttribute>.WrapperOrNull(AttributeInfoCaches.FirstOrDefault(f => f.Attribute.GetType() == typeof(FromXmlAttribute)));
			ForModelAttribute = DbAttributeInfoCache<ForModelAttribute>.WrapperOrNull(AttributeInfoCaches.FirstOrDefault(f => f.Attribute.GetType() == typeof(ForModelAttribute)));
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