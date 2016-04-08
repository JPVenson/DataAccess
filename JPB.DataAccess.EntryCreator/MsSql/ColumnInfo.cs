/*
This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/.
Please consider to give some Feedback on CodeProject

http://www.codeproject.com/Articles/818690/Yet-Another-ORM-ADO-NET-Wrapper

*/
using System;
using System.Data;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Helper;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace JPB.DataAccess.EntityCreator.MsSql
{
	[Serializable]
	public class ColumnInfo
	{
		private string _targetType2;

		[SelectFactoryMethod()]
		public static IQueryFactoryResult SelectColumns(string tableName, string database)
		{
			return new QueryFactoryResult("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName AND TABLE_CATALOG = @database", new[]
			{
				new QueryParameter("@tableName", tableName),
				new QueryParameter("@database", database)
			});
		}

		[ForModel("COLUMN_NAME")]
		public string ColumnName { get; set; }

		[ForModel("ORDINAL_POSITION")]
		public int PositionFromTop { get; set; }

		[ForModel("IS_NULLABLE")]
		[ValueConverter(typeof(NoYesConverter))]
		public bool Nullable { get; set; }

		public Type TargetType { get; set; }

		[ForModel("DATA_TYPE")]
		public string TargetType2
		{
			get { return _targetType2; }
			set
			{
				TargetType = DbTypeToCsType.GetClrType(value);
				if (Nullable && !TargetType.IsClass && !TargetType.Name.StartsWith("Nullable"))
				{
					TargetType = typeof (Nullable<>).MakeGenericType(TargetType);
				}
				_targetType2 = value;
			}
		}
	}
}