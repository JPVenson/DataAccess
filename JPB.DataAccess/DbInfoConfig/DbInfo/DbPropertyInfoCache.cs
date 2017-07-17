#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JPB.DataAccess.Contacts.MetaApi;
using JPB.DataAccess.MetaApi.Model;
using JPB.DataAccess.ModelsAnotations;

#endregion

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	internal class DbPropertyInfoCache<TE> : DbPropertyInfoCache
	{
		internal DbPropertyInfoCache(string name, Action<dynamic, TE> setter = null, Func<dynamic, TE> getter = null,
			params AttributeInfoCache[] attributes)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (attributes == null)
			{
				throw new ArgumentNullException("attributes");
			}

			PropertyName = name;

			if (setter != null)
			{
				Setter = new FakePropertyMethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(this,
				(o, objects) =>
				{
					setter(o, (TE) objects[0]);
					return null;
				});
			}

			if (getter != null)
			{
				Getter = new FakePropertyMethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(this,
				(o, objects) => getter(o));
			}
		}
	}

	internal class DbPropertyInfoCache<T, TE> : DbPropertyInfoCache
	{
		internal DbPropertyInfoCache(string name, Action<T, TE> setter = null, Func<T, TE> getter = null,
			params AttributeInfoCache[] attributes)
		{
			if (attributes == null)
			{
				throw new ArgumentNullException("attributes");
			}

			PropertyName = name;

			if (setter != null)
			{
				Setter = new FakePropertyMethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(this,
				(o, objects) =>
				{
					setter((T) o, (TE) objects[0]);
					return null;
				});
			}

			if (getter != null)
			{
				Getter = new FakePropertyMethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(this,
				(o, objects) => getter((T) o));
			}
		}
	}

	/// <summary>
	///     Infos about the Property
	/// </summary>
	public class DbPropertyInfoCache : PropertyInfoCache<DbAttributeInfoCache>
	{
#if !DEBUG
		[DebuggerHidden]
#endif

		/// <summary>
		///     Initializes a new instance of the <see cref="DbPropertyInfoCache" /> class.
		/// </summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbPropertyInfoCache()
		{
		}

		//internal DbPropertyInfoCache(string dbName, string propertyName, Type propertyType)
		//{
		//	this.PropertyName = propertyName;
		//	if (dbName != propertyName)
		//	{
		//		this.Attributes.Add(new DbAttributeInfoCache<ForModelAttribute>(new AttributeInfoCache(new ForModelAttribute(dbName))));
		//	}
		//}

		//public DbClassInfoCache ClassType { get; protected internal set; }

		/// <summary>
		///     The class that owns this Property
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
		///     if known the ForXml attribute
		/// </summary>
		public DbAttributeInfoCache<ForeignKeyDeclarationAttribute> ForginKeyDeclarationAttribute { get;
			protected internal set; }

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
				{
					return ForModelAttribute.Attribute.AlternatingName;
				}

				if (FromXmlAttribute != null)
				{
					return FromXmlAttribute.Attribute.FieldName;
				}
				return PropertyName;
			}
		}

		/// <summary>
		///     if known the RowVersion Attribute
		/// </summary>
		public DbAttributeInfoCache<RowVersionAttribute> RowVersionAttribute { get; private set; }

		/// <summary>
		///     if knwon the PrimaryKey Attribute
		/// </summary>
		public DbAttributeInfoCache<PrimaryKeyAttribute> PrimaryKeyAttribute { get; private set; }

		/// <summary>
		///     if knwon the Ignore Reflection Attribute
		/// </summary>
		public DbAttributeInfoCache<IgnoreReflectionAttribute> IgnoreAnyAttribute { get; private set; }

		/// <summary>
		///     If known the Generator that will mask this Property
		/// </summary>
		public DbAttributeInfoCache<AnonymousObjectGenerationAttribute> AnonymousObjectGenerationAttribute { get; set; }

		/// <summary>
		///     For internal Usage only
		/// </summary>
		public override IPropertyInfoCache<DbAttributeInfoCache> Init(PropertyInfo propertyInfo, bool anon)
		{
			base.Init(propertyInfo, anon);
			Refresh();
			return this;
		}

		//internal static PropertyInfoCache Logical(string info)
		//{
		//    return new PropertyInfoCache(null)
		//    {
		//        PropertyName = info
		//    };
		//}

		/// <summary>
		///     Refreshes all cached attributes
		/// </summary>
		public void Refresh()
		{
			PrimaryKeyAttribute = DbAttributeInfoCache<PrimaryKeyAttribute>.WrapperOrNull(Attributes);
			InsertIgnore = Attributes.Any(f => f.Attribute is InsertIgnoreAttribute);
			if (Getter != null && Getter.MethodInfo != null)
			{
				if (Getter.MethodInfo.IsVirtual)
				{
					ForginKeyAttribute = DbAttributeInfoCache<ForeignKeyAttribute>.WrapperOrNull(Attributes);
				}
				else
				{
					ForginKeyAttribute = null;
				}
			}
			AnonymousObjectGenerationAttribute = DbAttributeInfoCache<AnonymousObjectGenerationAttribute>.WrapperOrNull(Attributes);
			RowVersionAttribute = DbAttributeInfoCache<RowVersionAttribute>.WrapperOrNull(Attributes);
			FromXmlAttribute = DbAttributeInfoCache<FromXmlAttribute>.WrapperOrNull(Attributes);
			ForModelAttribute = DbAttributeInfoCache<ForModelAttribute>.WrapperOrNull(Attributes);
			IgnoreAnyAttribute = DbAttributeInfoCache<IgnoreReflectionAttribute>.WrapperOrNull(Attributes);
			ForginKeyDeclarationAttribute = DbAttributeInfoCache<ForeignKeyDeclarationAttribute>.WrapperOrNull(Attributes);
		}
	}
}