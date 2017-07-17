using System;
using System.Collections.Generic;
using System.IO;

namespace JPB.DataAccess.Helper
{
	public interface ITextWithColor<TColor>
	{
		TColor Color { get; }
		string Text { get; }
	}

	public interface IStringBuilderInterlaced<TColor> : IEnumerable<string>, IEnumerable<ITextWithColor<TColor>> where TColor : class, new()
	{
		IStringBuilderInterlaced<TColor> Append(string value, params object[] values);
		IStringBuilderInterlaced<TColor> Append(string value, TColor color = null);
		IStringBuilderInterlaced<TColor> Append(string value, TColor color = null, params object[] values);
		IStringBuilderInterlaced<TColor> AppendInterlaced(string value, params object[] values);
		IStringBuilderInterlaced<TColor> AppendInterlaced(string value, TColor color = null);
		IStringBuilderInterlaced<TColor> AppendInterlaced(string value, TColor color = null, params object[] values);
		IStringBuilderInterlaced<TColor> AppendInterlacedLine(string value, params object[] values);
		IStringBuilderInterlaced<TColor> AppendInterlacedLine(string value, TColor color = null);
		IStringBuilderInterlaced<TColor> AppendInterlacedLine(string value, TColor color = null, params object[] values);
		IStringBuilderInterlaced<TColor> AppendLine();
		IStringBuilderInterlaced<TColor> AppendLine(string value, params object[] values);
		IStringBuilderInterlaced<TColor> AppendLine(string value, TColor color = null);
		IStringBuilderInterlaced<TColor> AppendLine(string value, TColor color = null, params object[] values);
		IStringBuilderInterlaced<TColor> Color(TColor color);
		IStringBuilderInterlaced<TColor> Down();
		IStringBuilderInterlaced<TColor> Insert(IStringBuilderInterlaced<TColor> writer);
		IStringBuilderInterlaced<TColor> Insert(Action<IStringBuilderInterlaced<TColor>> del);
		IStringBuilderInterlaced<TColor> RevertColor();
		string ToString();
		IStringBuilderInterlaced<TColor> Up();
		void WriteToSteam(TextWriter output, Action<TColor> changeColor, Action changeColorBack);
	}
}