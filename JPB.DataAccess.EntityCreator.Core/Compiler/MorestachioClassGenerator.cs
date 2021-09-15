using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig.ClassBuilder;
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
			parserOptions.Formatters.AddSingle(
				(string memberName) => Char.ToLowerInvariant(memberName[0]) + memberName.Substring(1), "ToFieldName");
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
