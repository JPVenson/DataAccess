using System;
using JPB.DataAccess.DbInfoConfig.ClassBuilder;
using JPB.DataAccess.ModelsAnotations;
using Morestachio;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Rendering;

namespace JPB.DataAccess.EntityCreator.Core.Compiler
{
	public class MorestachioClassGenerator : ClassInfoGenerator
	{
		public MorestachioClassGenerator(string template)
		{
			var parserOptions = new ParserOptions(template);
			//parserOptions.Formatters.AddFromType(typeof(DynamicLinq));
			
			parserOptions.Formatters.Add(typeof(IBuilderType).GetMethod(nameof(IBuilderType.GetTypeName)),
				new MorestachioFormatterAttribute("GetTypeName", "")
				{
					LinkFunctionTarget = true, 
					IsSourceObjectAware = false
				});
			parserOptions.Formatters.AddSingle((string memberName) => Char.ToLowerInvariant(memberName[0]) + memberName.Substring(1), "ToFieldName");
			parserOptions.Formatters.AddSingle((AttributeInfo attribute) =>
			{
				var attributeType = Type.GetType(typeof(DataAccessAttribute).Namespace + "." + attribute.Name + ", JPB.DataAccess.Framework", false);
				if (attributeType == null)
				{
					return false;
				}
				return typeof(DataAccessAttribute).IsAssignableFrom(attributeType);
			}, "IsFrameworkAttribute");
			var document = Parser.ParseWithOptions(parserOptions);
			_renderer = document.CreateCompiledRenderer();
		}

		private IRenderer _renderer;

		public override string RenderPocoClass(bool notifyPropertysChanged = false)
		{
			var data = new
			{
				NotifyPropertysChanged = notifyPropertysChanged,
				ClassInfo = this
			};

			return _renderer.RenderAndStringify(data);
		}
	}
}
