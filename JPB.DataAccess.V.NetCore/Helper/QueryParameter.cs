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


		/// <summary>
		/// Wraps a Query Parameter with a name and value. This defines the type based on the value
		/// </summary>
		public QueryParameter(string name, object value)
		{
			Name = name;
			Value = value;
			if (value != null)
			{
				SourceType = value.GetType();
			}
			else
			{
				SourceType = DBNull.Value.GetType();
			}
		}

		/// <summary>
		/// Wraps a Query Parameter with a name and value
		/// </summary>
		public QueryParameter(string name, object value, Type valType)
		{
			Name = name;
			Value = value;
			SourceType = valType;
		}

		/// <summary>
		/// Wraps a Query Parameter with a name and value
		/// </summary>
		public QueryParameter(string name, object value, DbType valType)
		{
			Name = name;
			Value = value;
			SourceDbType = valType;
		}

		#region IQueryParameter Members

		/// <summary>
		/// The name of the Parameter
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///	The value of the Parameter
		/// </summary>
		public object Value
		{
			get { return _value; }
			set
			{
				SourceType = value == null ? DBNull.Value.GetType() : value.GetType();
				_value = value;
			}
		}

		/// <summary>
		/// The C# Type of the Parameter generated from SourceDbType
		/// </summary>
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

		/// <summary>
		/// The SQL Type of the Parameter generated from SourceType
		/// </summary>
		public DbType SourceDbType { get; set; }

		#endregion


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

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return Render();
		}
	}
}