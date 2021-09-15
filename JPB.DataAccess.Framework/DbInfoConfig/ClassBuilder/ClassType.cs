using System;
using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.DbInfoConfig.DbInfo;

#pragma warning disable 1591

namespace JPB.DataAccess.DbInfoConfig.ClassBuilder
{
	public interface IBuilderType
	{
		string Name { get; set; }
		bool IsList { get; set; }
		bool IsNullable { get; set; }
		IList<IBuilderType> GenericTypes { get; set; }
		string GetTypeName();
	}

	public class BuilderType : IBuilderType
	{
		static BuilderType()
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

		public BuilderType()
		{
			GenericTypes = new List<IBuilderType>();
		}

		public string Name { get; set; }
		public bool IsList { get; set; }
		public bool IsNullable { get; set; }
		public IList<IBuilderType> GenericTypes { get; set; }

		public virtual string GetTypeName()
		{
			var name = Name;
			if (GenericTypes.Any())
			{
				name += $"<{GenericTypes.Select(e => e.GetTypeName()).Aggregate((e, f) => $"{e},{f}")}>";
			}

			return name;
		}

		private static IDictionary<Type, string> _buildInTypes;

		public static IBuilderType FromCsType(Type type)
		{
			BuilderType csType;
			var underlyingType = Nullable.GetUnderlyingType(type);
			if (underlyingType != null)
			{
				csType = new NullableType(underlyingType);
			}
			else
			{
				csType = new BuilderType();
				csType.GenericTypes = type.GetGenericArguments().Select(f => new BuilderType
				{
					Name = f.Name
				}).ToArray();
				csType.IsList = type.CheckForListInterface();
				if (_buildInTypes.ContainsKey(type))
				{
					csType.Name = _buildInTypes[type];
				}
				else
				{
					csType.Name = type.Name;
				}
			}

			return csType;
		}

		public static IBuilderType FromProperty(DbPropertyInfoCache dbPropertyInfoCach)
		{
			return FromCsType(dbPropertyInfoCach.PropertyType);
		}
	}

	public class NullableType : BuilderType
	{
		public NullableType(Type innerType)
		{
			IsNullable = true;
			Type = FromCsType(innerType);
		}

		public IBuilderType Type { get; set; }

		public override string GetTypeName()
		{
			return Type.GetTypeName() + "?";
		}
	}
}