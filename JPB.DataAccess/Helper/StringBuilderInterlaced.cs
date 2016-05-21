/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Text;

namespace JPB.DataAccess.Helper
{
	public class StringBuilderInterlaced
	{
		private readonly StringBuilder _source;
		private int _interlacedLevel;
		private readonly uint _interlacedSpace;
		private readonly bool _transformInterlaced;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="transformInterlaced">If true an level will be displaced as <paramref name="intedtSize"/> spaces</param>
		/// <param name="intedtSize">ammount of spaces for each level</param> 
		public StringBuilderInterlaced(bool transformInterlaced = false, uint intedtSize = 4)
		{
			_interlacedSpace = intedtSize;
			_transformInterlaced = transformInterlaced;
			_source = new StringBuilder();
		}

		public StringBuilderInterlaced Up()
		{
			_interlacedLevel++;
			return this;
		}

		public StringBuilderInterlaced Down()
		{
			if (_interlacedLevel > 0)
			{
				_interlacedLevel--;
			}
			return this;
		}

		public StringBuilderInterlaced AppendInterlacedLine()
		{
			_source.AppendLine();
			return this;
		}

		private void ApplyLevel()
		{
			if (_transformInterlaced)
			{
				for (var i = 0; i < _interlacedLevel; i++)
				{
					_source.Append(" ");
				}
			}
			else
			{
				for (var i = 0; i < _interlacedLevel; i++)
				{
					_source.Append("\t");
				}
			}
		}

		public StringBuilderInterlaced AppendInterlacedLine(string value)
		{
			ApplyLevel();
			_source.AppendLine(value);
			return this;
		}

		public StringBuilderInterlaced AppendInterlaced(string value)
		{
			ApplyLevel();
			_source.Append(value);
			return this;
		}


		public StringBuilderInterlaced Insert(Action<StringBuilderInterlaced> del)
		{
			del(this);
			return this;
		}

		public StringBuilderInterlaced Append(string value)
		{
			_source.Append(value);
			return this;
		}

		public StringBuilderInterlaced AppendLine(string value)
		{
			_source.AppendLine(value);
			return this;
		}

		public StringBuilderInterlaced AppendInterlacedLine(string value, params object[] values)
		{
			ApplyLevel();
			_source.AppendLine(string.Format(value, values));
			return this;
		}

		public StringBuilderInterlaced AppendInterlaced(string value, params object[] values)
		{
			ApplyLevel();
			_source.Append(string.Format(value, values));
			return this;
		}

		public override string ToString()
		{
			return _source.ToString();
		}
	}
}