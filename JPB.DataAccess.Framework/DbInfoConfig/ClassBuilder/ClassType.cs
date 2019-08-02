using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.DbInfoConfig.DbInfo;
using JPB.DataAccess.Framework;

#pragma warning disable 1591

namespace JPB.DataAccess.DbInfoConfig.ClassBuilder
{
	public class ClassType
	{
		static ClassType()
		{
			_buildInTypes = new Dictionary<Type, string>();
			_buildInTypes[typeof(bool)] = "bool";
			_buildInTypes[typeof(byte)] = "byte";
			_buildInTypes[typeof(sbyte)] = "sbyte";
			_buildInTypes[typeof(char)] = "char";
			_buildInTypes[typeof(decimal)] = "decimal";
			_buildInTypes[typeof(double)] = "double";
			_buildInTypes[typeof(float)] = "float";
			_buildInTypes[typeof(int)] = "int";
			_buildInTypes[typeof(uint)] = "uint";
			_buildInTypes[typeof(long)] = "long";
			_buildInTypes[typeof(ulong)] = "ulong";
			_buildInTypes[typeof(short)] = "short";
			_buildInTypes[typeof(ushort)] = "ushort";
			_buildInTypes[typeof(string)] = "string";
			_buildInTypes[typeof(object)] = "object";
		}

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

		private static IDictionary<Type, string> _buildInTypes;

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
				if (_buildInTypes.ContainsKey(type))
				{
					csType.Name = _buildInTypes[type];
				}
				else
				{
					csType.Name = type.Name;
				}
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