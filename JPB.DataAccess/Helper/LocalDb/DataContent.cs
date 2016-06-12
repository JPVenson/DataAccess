﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Transactions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace JPB.DataAccess.Helper.LocalDb
{
	public class DataContent : IXmlSerializable
	{
		private readonly LocalDbManager _instance;

		public DataContent()
		{

		}

		internal DataContent(LocalDbManager instance)
		{
			_instance = instance;
		}

		private const string DatabaseName = "DatabaseScope";
		private const string ReprosIncluded = "Types";
		private const string ReproIncluded = "Type";
		private const string DatabaseContent = "Tables";
		private const string TableContentList = "Table";

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			reader.Read();
			reader.ReadStartElement(DatabaseName);
			reader.ReadStartElement(ReprosIncluded);

			var elements = new Dictionary<string, Type>();
			var index = 0;
			do
			{
				var indexOfElementString = reader.GetAttribute("Index");
				var typeString = reader.GetAttribute("Type");
				if (indexOfElementString == null)
					throw new InvalidDataException("Invalid XML document for Db import. index is unset");

				if (typeString == null)
					throw new InvalidDataException("Invalid XML document for Db import. type is unset");

				var type = Type.GetType(typeString);

				if (type == null)
					throw new InvalidDataException(string.Format("Invalid XML document for Db import. type is invalid '{0}'", typeString));

				elements.Add(indexOfElementString, type);
			} while (reader.Read() && reader.Name == ReproIncluded);
			reader.ReadEndElement();
			reader.ReadStartElement(DatabaseContent);
			using (var transaction = new TransactionScope())
			{
				using (new IdentityInsertScope())
				{
					do
					{
						var indexOfElementString = reader.GetAttribute("Index");
						reader.ReadStartElement(TableContentList);
						var type = elements[indexOfElementString];
						var table = LocalDbManager.Scope.Database[type];
						table.IsMigrating = true;
						do
						{
							object emptyElement = table.TypeInfo.DefaultFactory();
							reader.ReadStartElement(table.TypeInfo.TableName);
							do
							{
								var propName = reader.Name;
								var isNumm = reader.IsEmptyElement;
								
								var value = reader.ReadElementContentAsString();
								var dbPropertyInfoCache = table.TypeInfo.Propertys[table.TypeInfo.SchemaMappingDatabaseToLocal(propName)];

								object contvertedValue = null;
								if (!isNumm)
								{
									contvertedValue = Convert.ChangeType(value, dbPropertyInfoCache.PropertyType);
								}
						
								dbPropertyInfoCache.Setter.Invoke(emptyElement, contvertedValue);
							} while (reader.Name != table.TypeInfo.TableName);
							reader.ReadEndElement();

							table.Add(emptyElement);
						} while (reader.Name == table.TypeInfo.TableName);

						reader.ReadEndElement();
					} while (reader.Name != DatabaseContent);
				}

				transaction.Complete();
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(DatabaseName);
			writer.WriteStartElement(ReprosIncluded);
			var index = 0;
			foreach (var localDbReposetoryBase in _instance.Database)
			{
				writer.WriteStartElement(ReproIncluded);
				writer.WriteAttributeString("Index", index++.ToString());
				writer.WriteAttributeString("Type", localDbReposetoryBase.Key.AssemblyQualifiedName);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement(DatabaseContent);
			index = 0;
			foreach (var localDbReposetoryBase in _instance.Database)
			{
				writer.WriteStartElement(TableContentList);
				writer.WriteAttributeString("Index", index++.ToString());

				foreach (var poco in localDbReposetoryBase.Value)
				{
					writer.WriteStartElement(localDbReposetoryBase.Value.TypeInfo.TableName);
					foreach (var dbPropertyInfoCach in localDbReposetoryBase
						.Value
						.TypeInfo
						.Propertys
						.Values
						.Where(f => f.ForginKeyAttribute == null && f.FromXmlAttribute == null))
					{
						writer.WriteStartElement(dbPropertyInfoCach.DbName);
						var value = dbPropertyInfoCach.Getter.Invoke(poco);
						if (value != null)
						{
							writer.WriteValue(value);
						}
						writer.WriteEndElement();
					}
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteEndElement();
		}
	}
}