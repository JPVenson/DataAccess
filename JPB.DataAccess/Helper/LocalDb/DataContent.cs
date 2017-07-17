#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JPB.DataAccess.Helper.LocalDb.Scopes;

#endregion

namespace JPB.DataAccess.Helper.LocalDb
{
	/// <summary>
	///     Provieds the IXmlSerializable interface for an entire database.
	///     Not thread save
	/// </summary>
	public class DataContent : IXmlSerializable
	{
		private const string DatabaseName = "DatabaseScope";
		private const string ReprosIncluded = "Types";
		private const string ReproIncluded = "Type";
		private const string DatabaseContent = "Tables";
		private const string TableContentList = "Table";
		private const string TableContentElementsList = "Items";
		private const string IndexerIncluded = "Indexer";
		private const string IndexerValues = "Index";
		private readonly LocalDbManager _instance;

		/// <summary>
		///     Initializes a new instance of the <see cref="DataContent" /> class.
		/// </summary>
		public DataContent()
		{
		}

		internal DataContent(LocalDbManager instance)
		{
			_instance = instance;
		}

		/// <summary>
		///     Gets the schema.
		/// </summary>
		/// <returns></returns>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		///     Reads the XML.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <exception cref="InvalidDataException">
		///     Invalid XML document for Db import. index is unset
		///     or
		///     Invalid XML document for Db import. type is unset
		///     or
		///     or
		///     Invalid XML document for Db import. index for a table is unset
		/// </exception>
		public void ReadXml(XmlReader reader)
		{
			reader.Read();
			reader.ReadStartElement(DatabaseName);
			reader.ReadStartElement(ReprosIncluded);

			var elements = new Dictionary<string, Type>();
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
					throw new InvalidDataException(string.Format("Invalid XML document for Db import. type is invalid '{0}'",
						typeString));

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
						if (indexOfElementString == null)
							throw new InvalidDataException("Invalid XML document for Db import. index for a table is unset");
						reader.ReadStartElement(TableContentList);
						var type = elements[indexOfElementString];
						var table =
							LocalDbManager.Scope.Database.First(s => s.Key.AssemblyQualifiedName == type.AssemblyQualifiedName).Value;
						if (table == null)
							throw new InvalidDataException("Invalid Database config for Db import. There is no Table for the type " + type);
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
									contvertedValue = DataConverterExtensions.ChangeType(value, dbPropertyInfoCache.PropertyType);

								dbPropertyInfoCache.Setter.Invoke(emptyElement, contvertedValue);
							} while (reader.Name != table.TypeInfo.TableName);
							reader.ReadEndElement();

							table.Add(emptyElement);
						} while (reader.Name == table.TypeInfo.TableName);

						reader.ReadEndElement();
					} while (reader.Name != DatabaseContent);
					transaction.Complete();
				}
			}
		}

		/// <summary>
		///     Writes the XML.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public void WriteXml(XmlWriter writer)
		{
			//write Root
			writer.WriteStartElement(DatabaseName);
			//write Repro
			writer.WriteStartElement(ReprosIncluded);
			var index = 0;
			foreach (var localDbReposetoryBase in _instance.Database)
			{
				if (localDbReposetoryBase.Value.Count == 0)
					continue;

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
				if (localDbReposetoryBase.Value.Count == 0)
					continue;

				//write table
				writer.WriteStartElement(TableContentList);
				writer.WriteAttributeString("Index", index++.ToString());

				//write content
				writer.WriteStartElement(TableContentElementsList);

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
							writer.WriteValue(value);
						writer.WriteEndElement();
					}
					writer.WriteEndElement();
				}

				//end write content
				writer.WriteEndElement();

				//write indexer
				writer.WriteStartElement(IndexerIncluded);

				//end write indexer
				writer.WriteEndElement();
				//end write table
				writer.WriteEndElement();
			}
			//end write Repro
			writer.WriteEndElement();

			//end write Root
			writer.WriteEndElement();
		}

		/// <summary>
		///     Returns the current Databases in this scope as an XML string inside an MemoryStream
		/// </summary>
		/// <returns></returns>
		public MemoryStream ReadAsXml()
		{
			var ms = new MemoryStream();
			var xmlSerilzer = new XmlSerializer(GetType());
			xmlSerilzer.Serialize(ms, this);
			return ms;
		}

		/// <summary>
		///     Returns the current Databases in this scope as an XML string
		/// </summary>
		/// <returns></returns>
		public string ReadAsXmlString()
		{
			return Encoding.UTF8.GetString(ReadAsXml().ToArray());
		}
	}
}