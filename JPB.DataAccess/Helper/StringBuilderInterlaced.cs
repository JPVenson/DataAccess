#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#endregion

namespace JPB.DataAccess.Helper
{
	/// <summary>
	///     Allows building of strings in a interlaced and colored way
	/// </summary>
	public class StringBuilderInterlaced
	{
		private readonly uint _interlacedSpace = 4;

		private readonly List<ColoredString> _source;
		private readonly bool _transformInterlaced;
		private ConsoleColor? _color;
		private int _interlacedLevel;

		/// <summary>
		/// </summary>
		/// <param name="transformInterlaced">If true an level will be displaced as <paramref name="intedtSize" /> spaces</param>
		/// <param name="intedtSize">ammount of spaces for each level</param>
		public StringBuilderInterlaced(bool transformInterlaced = false, uint intedtSize = 4)
		{
			_interlacedSpace = intedtSize;
			_transformInterlaced = transformInterlaced;
			_source = new List<ColoredString>();
		}

		/// <summary>
		///     Sets the color for all Folloring Text parts
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public StringBuilderInterlaced Color(ConsoleColor color)
		{
			_color = color;
			return this;
		}

		/// <summary>
		///     Reverts the color.
		/// </summary>
		/// <returns></returns>
		public StringBuilderInterlaced RevertColor()
		{
			_color = null;
			return this;
		}

		/// <summary>
		///     increases all folloring Text parts by 1
		/// </summary>
		/// <returns></returns>
		public StringBuilderInterlaced Up()
		{
			_interlacedLevel++;
			return this;
		}

		/// <summary>
		///     decreases all folloring Text parts by 1
		/// </summary>
		/// <returns></returns>
		public StringBuilderInterlaced Down()
		{
			if (_interlacedLevel > 0)
				_interlacedLevel--;
			return this;
		}

		/// <summary>
		///     Appends the line.
		/// </summary>
		/// <returns></returns>
		public StringBuilderInterlaced AppendLine()
		{
			return Append(Environment.NewLine);
		}

		private void ApplyLevel()
		{
			var text = "";
			if (_transformInterlaced)
				for (var i = 0; i < _interlacedLevel; i++)
				for (var j = 0; j < _interlacedSpace; j++)
					text += " ";
			else
				for (var i = 0; i < _interlacedLevel; i++)
					text += "\t";
			_source.Add(new ColoredString(text));
		}

		/// <summary>
		///     Appends the interlaced line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public StringBuilderInterlaced AppendInterlacedLine(string value, ConsoleColor? color = null)
		{
			ApplyLevel();
			return AppendLine(value, color);
		}

		/// <summary>
		///     Appends the interlaced.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public StringBuilderInterlaced AppendInterlaced(string value, ConsoleColor? color = null)
		{
			ApplyLevel();
			return Append(value, color);
		}

		/// <summary>
		///     Inserts the specified delete.
		/// </summary>
		/// <param name="del">The delete.</param>
		/// <returns></returns>
		public StringBuilderInterlaced Insert(Action<StringBuilderInterlaced> del)
		{
			del(this);
			return this;
		}

		/// <summary>
		///     Inserts the specified writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <returns></returns>
		public StringBuilderInterlaced Insert(StringBuilderInterlaced writer)
		{
			foreach (var coloredString in writer._source)
				_source.Add(coloredString);
			return this;
		}

		/// <summary>
		///     Appends the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public StringBuilderInterlaced Append(string value, ConsoleColor? color = null)
		{
			if (color == null)
				color = _color;
			_source.Add(new ColoredString(value, color));
			return this;
		}

		/// <summary>
		///     Appends the line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		public StringBuilderInterlaced AppendLine(string value, ConsoleColor? color = null)
		{
			return Append(value + Environment.NewLine, color);
		}

		/// <summary>
		///     Appends the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public StringBuilderInterlaced Append(string value, params object[] values)
		{
			return Append(string.Format(value, values));
		}

		/// <summary>
		///     Appends the line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public StringBuilderInterlaced AppendLine(string value, params object[] values)
		{
			return Append(string.Format(value, values) + Environment.NewLine);
		}

		/// <summary>
		///     Appends the interlaced line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public StringBuilderInterlaced AppendInterlacedLine(string value, params object[] values)
		{
			return AppendInterlacedLine(string.Format(value, values));
		}

		/// <summary>
		///     Appends the interlaced.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public StringBuilderInterlaced AppendInterlaced(string value, params object[] values)
		{
			return AppendInterlaced(string.Format(value, values));
		}

		/// <summary>
		///     Appends the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public StringBuilderInterlaced Append(string value, ConsoleColor? color = null, params object[] values)
		{
			return Append(string.Format(value, values), color);
		}

		/// <summary>
		///     Appends the line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public StringBuilderInterlaced AppendLine(string value, ConsoleColor? color = null, params object[] values)
		{
			Append(string.Format(value, values) + Environment.NewLine, color);
			return this;
		}

		/// <summary>
		///     Appends the interlaced line.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public StringBuilderInterlaced AppendInterlacedLine(string value, ConsoleColor? color = null, params object[] values)
		{
			return AppendInterlacedLine(string.Format(value, values), color);
		}

		/// <summary>
		///     Appends the interlaced.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public StringBuilderInterlaced AppendInterlaced(string value, ConsoleColor? color = null, params object[] values)
		{
			return AppendInterlaced(string.Format(value, values), color);
		}

		/// <summary>
		///     Writes to steam.
		/// </summary>
		/// <param name="output">The output.</param>
		/// <param name="changeColor">Color of the change.</param>
		/// <param name="changeColorBack">The change color back.</param>
		public void WriteToSteam(TextWriter output, Action<ConsoleColor> changeColor, Action changeColorBack)
		{
			ConsoleColor? color = null;
			var sb = new StringBuilder();
			foreach (var coloredString in _source)
			{
				var nColor = coloredString.GetColor();
				if (nColor != color && sb.Length > 0)
				{
					//write buffer to output
					if (color.HasValue)
						changeColor(color.Value);
					output.Write(sb.ToString());
					if (color.HasValue)
						changeColorBack();
					sb.Clear();
				}

				sb.Append(coloredString);
				color = nColor;
			}
			if (color.HasValue)
				changeColor(color.Value);
			output.Write(sb.ToString());
			if (color.HasValue)
				changeColorBack();
		}

		/// <summary>
		///     Writes to console.
		/// </summary>
		public void WriteToConsole()
		{
			WriteToSteam(Console.Out, color => Console.ForegroundColor = color,
				() => Console.ForegroundColor = ConsoleColor.White);
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents all text parts without any color
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return _source.Select(f => f.ToString()).Aggregate((e,f) => e + f).ToString();
		}

		struct ColoredString
		{
			private string _text;
			private ConsoleColor? _color;

			public ColoredString(string text, ConsoleColor? color = null)
			{
				_color = color;
				_text = text;
			}

			public ConsoleColor? GetColor()
			{
				return _color;
			}

			public override string ToString()
			{
				return _text;
			}
		}
	}
}