/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Manager;

namespace JPB.DataAccess.Helper
{
	/// <summary>
	///     Example Implimentation of IQueryParameter
	/// </summary>
	public class QueryParameter : IQueryParameter
	{
		private object _value;
		private Type _sourceType;
		// ReSharper disable CSharpWarnings::CS1591
		private QueryParameter()
		{
		}

		public QueryParameter(string name, object value)
		{
			Name = name;
			Value = value;
			SourceType = value.GetType();
		}

		public QueryParameter(string name, object value, Type valType)
		{
			Name = name;
			Value = value;
			SourceType = valType;
		}

		public QueryParameter(string name, object value, DbType valType)
		{
			Name = name;
			Value = value;
			SourceDbType = valType;
		}

		#region IQueryParameter Members

		public string Name { get; set; }

		public object Value
		{
			get { return _value; }
			set
			{
				SourceType = value == null ? DBNull.Value.GetType() : value.GetType();
				_value = value;
			}
		}

		public Type SourceType
		{
			get { return _sourceType; }
			set
			{
				_sourceType = value;
				var dbType = DbAccessLayer.Map(value);
				if (dbType != null)
					SourceDbType = dbType.Value;
			}
		}

		public DbType SourceDbType { get; set; }

		#endregion

		// ReSharper restore CSharpWarnings::CS1591

		/// <summary>
		///     Renders the current object
		/// </summary>
		/// <returns></returns>
		public string Render()
		{
			var sb = new StringBuilderInterlaced();
			Render(sb);
			return sb.ToString();
		}

		internal void Render(StringBuilderInterlaced sb)
		{
			var value = "{Null}";
			if (Value != null)
			{
				value = Value.ToString();
			}
			sb.AppendInterlacedLine("new QueryParameter {")
				.Up()
				.AppendInterlacedLine("Name = {0},", Name)
				.AppendInterlacedLine("Value.ToString = {0}", value)
				.AppendInterlacedLine("SourceType = {0}", SourceType.ToString())
				.AppendInterlacedLine("SourceDbType = {0}", SourceDbType)
				.Down()
				.AppendInterlaced("}");
		}

		public override string ToString()
		{
			return Render();
		}
	}
}