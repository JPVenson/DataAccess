#region

using System;
using JPB.DataAccess.MetaApi.Model;

#endregion

namespace JPB.DataAccess.DbInfoConfig.DbInfo
{
	internal class DbAutoStaticPropertyInfoCache<TE> : DbPropertyInfoCache
	{
		private TE _value;

		internal DbAutoStaticPropertyInfoCache(string name, Type declaringType, params AttributeInfoCache[] attributes)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (attributes == null)
				throw new ArgumentNullException("attributes");

			PropertyName = name;

			Setter = new FakePropertyMethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(this,
				(o, objects) =>
				{
					Value = (TE) objects[0];
					return null;
				});

			Getter = new FakePropertyMethodInfoCache<DbAttributeInfoCache, MethodArgsInfoCache<DbAttributeInfoCache>>(this,
				(o, objects) => { return Value; });

			//Setter.UseILWrapper = true;
			//Getter.UseILWrapper = true;
		}

		public TE Value
		{
			get { return _value; }
			set { _value = value; }
		}
	}
}