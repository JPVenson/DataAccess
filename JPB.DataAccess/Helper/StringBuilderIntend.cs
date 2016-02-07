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
	internal class StringBuilderIntend
	{
		private readonly StringBuilder _source;
		private int _inted;

		public StringBuilderIntend()
		{
			_source = new StringBuilder();
		}

		public StringBuilderIntend Up()
		{
			_inted++;
			return this;
		}

		public StringBuilderIntend Down()
		{
			if (_inted > 0)
			{
				_inted--;
			}
			return this;
		}

		public StringBuilderIntend AppendIntedLine()
		{
			_source.AppendLine();
			return this;
		}

		public StringBuilderIntend AppendIntedLine(string value)
		{
			for (var i = 0; i < _inted; i++)
			{
				_source.Append("\t");
			}
			_source.AppendLine(value);
			return this;
		}

		public StringBuilderIntend AppendInted(string value)
		{
			for (var i = 0; i < _inted; i++)
			{
				_source.Append("\t");
			}
			_source.Append(value);
			return this;
		}


		public StringBuilderIntend Insert(Action<StringBuilderIntend> del)
		{
			del(this);
			return this;
		}

		public StringBuilderIntend Append(string value)
		{
			_source.Append(value);
			return this;
		}

		public StringBuilderIntend AppendLine(string value)
		{
			_source.AppendLine(value);
			return this;
		}

		public StringBuilderIntend AppendIntedLine(string value, params object[] values)
		{
			for (var i = 0; i < _inted; i++)
			{
				_source.Append("\t");
			}
			_source.AppendLine(string.Format(value, values));
			return this;
		}

		public StringBuilderIntend AppendInted(string value, params object[] values)
		{
			for (var i = 0; i < _inted; i++)
			{
				_source.Append("\t");
			}
			_source.Append(string.Format(value, values));
			return this;
		}

		public override string ToString()
		{
			return _source.ToString();
		}
	}
}