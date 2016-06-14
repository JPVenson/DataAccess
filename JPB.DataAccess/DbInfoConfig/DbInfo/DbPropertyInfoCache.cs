/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
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
	internal class DbPropertyInfoCache<TE> : DbPropertyInfoCache
	{
		internal DbPropertyInfoCache(string name, Action<dynamic, TE> setter = null, Func<dynamic, TE> getter = null,
			params AttributeInfoCache[] attributes)
		{
			if (name == null) 
				throw new ArgumentNullException("name");
			if (attributes == null)
				throw new ArgumentNullException("attributes");

			PropertyName = name;

			if (setter != null)
			{
				Setter = new FakePropertyMethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(this, (o, objects) =>
				{
					setter(o, (TE)objects[0]);
					return null;
				});
			}

			if (getter != null)
			{
				Getter = new FakePropertyMethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(this, (o, objects) => getter(o));
			}
		}
	}

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
				Setter = new FakePropertyMethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(this, (o, objects) =>
				{
					setter((T)o, (TE)objects[0]);
					return null;
				});
			}

			if (getter != null)
			{
				Getter = new FakePropertyMethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(this, (o, objects) => getter((T)o));
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
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DbPropertyInfoCache()
		{
			base.Attributes = new HashSet<DbAttributeInfoCache>();
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
		//		this.Attributes.Add(new DbAttributeInfoCache<ForModelAttribute>(new AttributeInfoCache(new ForModelAttribute(dbName))));
		//	}
		//}


		//public DbClassInfoCache ClassType { get; protected internal set; }

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
		///     if known the ForXml attribute
		/// </summary>
		public DbAttributeInfoCache<ForeignKeyDeclarationAttribute> ForginKeyDeclarationAttribute { get; protected internal set; }

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

				if (FromXmlAttribute != null)
					return FromXmlAttribute.Attribute.FieldName;
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
		///		if knwon the Ignore Reflection Attribute
		/// </summary>
		public DbAttributeInfoCache<IgnoreReflectionAttribute> IgnoreAnyAttribute { get; private set; }

		/// <summary>
		///     For internal Usage only
		/// </summary>
		public void Refresh()
		{
			PrimaryKeyAttribute = DbAttributeInfoCache<PrimaryKeyAttribute>.WrapperOrNull(Attributes.FirstOrDefault(f => f.Attribute.GetType() == typeof(PrimaryKeyAttribute)));
			InsertIgnore = Attributes.Any(f => f.Attribute is InsertIgnoreAttribute);
			if (Getter != null && Getter.MethodInfo != null)
			{
				if (Getter.MethodInfo.IsVirtual)
				{
					ForginKeyAttribute =
						DbAttributeInfoCache<ForeignKeyAttribute>.WrapperOrNull(
							Attributes.FirstOrDefault(f => f.Attribute.GetType() == typeof(ForeignKeyAttribute)));
				}
				else
				{
					ForginKeyAttribute = null;
				}
			}
			RowVersionAttribute =
				DbAttributeInfoCache<RowVersionAttribute>.WrapperOrNull(
					Attributes.FirstOrDefault(s => s.Attribute is RowVersionAttribute));
			FromXmlAttribute = DbAttributeInfoCache<FromXmlAttribute>.WrapperOrNull(Attributes.FirstOrDefault(f => f.Attribute.GetType() == typeof(FromXmlAttribute)));
			ForModelAttribute = DbAttributeInfoCache<ForModelAttribute>.WrapperOrNull(Attributes.FirstOrDefault(f => f.Attribute.GetType() == typeof(ForModelAttribute)));
			IgnoreAnyAttribute = DbAttributeInfoCache<IgnoreReflectionAttribute>.WrapperOrNull(Attributes.FirstOrDefault(f => f.Attribute.GetType() == typeof(IgnoreReflectionAttribute)));
			ForginKeyDeclarationAttribute = DbAttributeInfoCache<ForeignKeyDeclarationAttribute>.WrapperOrNull(Attributes.FirstOrDefault(f => f.Attribute.GetType() == typeof(ForeignKeyDeclarationAttribute)));
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