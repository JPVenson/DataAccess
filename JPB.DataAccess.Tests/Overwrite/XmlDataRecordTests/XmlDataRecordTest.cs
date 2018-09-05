#region

using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Tests.Base.TestModels.XmlDataRecordTest;
using NUnit.Framework;

#endregion

namespace JPB.DataAccess.Tests.Overwrite.XmlDataRecordTests

{
	[TestFixture]
	public class XmlDataRecordTest
	{
		[Test]
		public void GetName()
		{
			var xmlSerilizer = new XmlSerializer(typeof(InstanceMock));
			var content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}

			var xmlRecord = new XmlDataRecord(content, typeof(InstanceMock).GetClassInfo());
			Assert.AreEqual(xmlRecord.GetName(0), "MockPropA");
			Assert.AreEqual(xmlRecord.GetName(1), "MockPropB");
		}

		[Test]
		public void GetValue()
		{
			var xmlSerilizer = new XmlSerializer(typeof(InstanceMock));
			var content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}

			var dbConfig = new DbConfig(true);

			var xmlRecord = new XmlDataRecord(content, dbConfig.GetOrCreateClassInfoCache(typeof(InstanceMock)));
			Assert.AreEqual(xmlRecord.GetValue(0), "NAN");
			Assert.AreEqual(xmlRecord.GetValue(1), 0);
		}

		[Test]
		public void InstanceFromString()
		{
			var xmlSerilizer = new XmlSerializer(typeof(InstanceMock));
			var content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}
			var dbConfig = new DbConfig(true);

			new XmlDataRecord(content, dbConfig.GetOrCreateClassInfoCache(typeof(InstanceMock)));
		}

		[Test]
		public void InstanceFromXDocument()
		{
			var xmlSerilizer = new XmlSerializer(typeof(InstanceMock));
			var content = "";
			using (var ms = new MemoryStream())
			{
				xmlSerilizer.Serialize(ms, new InstanceMock());
				content = Encoding.Default.GetString(ms.ToArray());
			}
			Assert.That(() => new XmlDataRecord(XDocument.Parse(content), typeof(InstanceMock), new DbConfig(true)),
				Throws.Nothing);
			;
		}
	}
}