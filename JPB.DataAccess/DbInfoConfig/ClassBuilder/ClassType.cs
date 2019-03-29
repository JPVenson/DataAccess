using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.DbInfoConfig.DbInfo;
#pragma warning disable 1591

namespace JPB.DataAccess.DbInfoConfig.ClassBuilder
{
	public class ClassType
	{
		public ClassType()
		{
			GenericTypes = new List<ClassType>();
		}

		public string Name { get; set; }
		public bool IsList { get; set; }
		public bool IsNullable { get; set; }
		public IList<ClassType> GenericTypes { get; set; }

		public string GetTypeName()
		{
			var name = Name;
			if (GenericTypes.Any())
			{
				name += $"<{GenericTypes.Select(e => e.GetTypeName()).Aggregate((e, f) => $"{e},{f}")}>";
			}

			return name;
		}

		public static ClassType FromCsType(Type type)
		{
			var csType = new ClassType();
			csType.IsNullable = Nullable.GetUnderlyingType(type) != null;
			csType.GenericTypes = type.GetGenericArguments().Select(f => new ClassType
			{
				Name = f.Name
			}).ToArray();
			if (csType.IsNullable)
			{
				csType.Name = $"Nullable";
			}
			else
			{
				csType.Name = type.Name;
			}

			csType.IsList = type.CheckForListInterface();
			return csType;
		}

		public static ClassType FromProperty(DbPropertyInfoCache dbPropertyInfoCach)
		{
			return FromCsType(dbPropertyInfoCach.PropertyType);
		}
	}
}