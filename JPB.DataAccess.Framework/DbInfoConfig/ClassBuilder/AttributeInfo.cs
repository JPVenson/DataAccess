using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess.Framework.Helper;

#pragma warning disable 1591

namespace JPB.DataAccess.Framework.DbInfoConfig.ClassBuilder
{
	public class AttributeInfo
	{
		public AttributeInfo()
		{
			PropertySetters = new Dictionary<string, string>();
			ConstructorSetters = new Dictionary<string, string>();
		}

		public string Name { get; set; }
		public IDictionary<string, string> PropertySetters { get; set; }
		public IDictionary<string, string> ConstructorSetters { get; set; }

		public void Render(ConsoleStringBuilderInterlaced sb)
		{
			sb.AppendInterlaced("[")
				.Append($"{Name}")
				.Append("(");

			var result = RenderAssignments();

			sb.Append(result).AppendLine(")]");
		}

		public string RenderAssignments()
		{
			var result = "";
			if (ConstructorSetters.Any())
			{
				result = ConstructorSetters
					.Select(constructorSetter => $"{constructorSetter.Key}: {constructorSetter.Value}")
					.Aggregate((e, f) => $"{e}, {f}");
			}

			if (PropertySetters.Any())
			{
				var optionals = ConstructorSetters
					.Select(constructorSetter => $"{constructorSetter.Key} = {constructorSetter.Value}");
				if (result != null)
				{
					result = optionals.Aggregate(result, (e, f) => $"{e},{f}");
				}
				else
				{
					result = optionals.Aggregate((e, f) => $"{e},{f}");
				}
			}

			return result;
		}
	}
}